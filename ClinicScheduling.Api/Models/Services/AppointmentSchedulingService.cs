using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Services;

/// <summary>
/// Centraliza las reglas de negocio para agendar y actualizar citas médicas.
/// </summary>
public class AppointmentSchedulingService
{
    private readonly ClinicSchedulingDbContext _dbContext;

    /// <summary>
    /// Inicializa una nueva instancia del servicio de agenda de citas.
    /// </summary>
    /// <param name="dbContext">Contexto de datos utilizado para consultar y persistir citas.</param>
    public AppointmentSchedulingService(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Agenda una nueva cita aplicando validaciones de horario, duración, conflictos y alertas.
    /// </summary>
    /// <param name="doctorId">Identificador del médico.</param>
    /// <param name="patientId">Identificador del paciente.</param>
    /// <param name="requestedStartDateTime">Fecha y hora solicitada para iniciar la cita.</param>
    /// <param name="reason">Motivo de la consulta.</param>
    /// <param name="status">Estado inicial de la cita.</param>
    /// <param name="cancellationReason">Motivo de cancelación si aplica.</param>
    /// <returns>Resultado de la operación de agenda.</returns>
    public async Task<AppointmentSchedulingResult> ScheduleAsync(
        Guid doctorId,
        Guid patientId,
        DateTime requestedStartDateTime,
        string status,
        string? reason,
        string? cancellationReason)
    {
        return await SaveAppointmentAsync(null, doctorId, patientId, requestedStartDateTime, status, reason, cancellationReason, true);
    }

    /// <summary>
    /// Actualiza una cita existente aplicando las mismas reglas de negocio de la agenda.
    /// </summary>
    /// <param name="appointmentId">Identificador de la cita a actualizar.</param>
    /// <param name="doctorId">Identificador del médico.</param>
    /// <param name="patientId">Identificador del paciente.</param>
    /// <param name="requestedStartDateTime">Nueva fecha y hora de inicio solicitada.</param>
    /// <param name="status">Nuevo estado de la cita.</param>
    /// <param name="reason">Nuevo motivo de la consulta.</param>
    /// <param name="cancellationReason">Nuevo motivo de cancelación.</param>
    /// <param name="isActive">Indica si la cita permanece activa.</param>
    /// <returns>Resultado de la operación de actualización.</returns>
    public async Task<AppointmentSchedulingResult> UpdateAsync(
        Guid appointmentId,
        Guid doctorId,
        Guid patientId,
        DateTime requestedStartDateTime,
        string status,
        string? reason,
        string? cancellationReason,
        bool isActive)
    {
        return await SaveAppointmentAsync(appointmentId, doctorId, patientId, requestedStartDateTime, status, reason, cancellationReason, isActive);
    }

    /// <summary>
    /// Guarda una cita nueva o existente aplicando las reglas funcionales definidas para la agenda.
    /// </summary>
    private async Task<AppointmentSchedulingResult> SaveAppointmentAsync(
        Guid? appointmentId,
        Guid doctorId,
        Guid patientId,
        DateTime requestedStartDateTime,
        string status,
        string? reason,
        string? cancellationReason,
        bool isActive)
    {
        var normalizedStart = NormalizeDateTime(requestedStartDateTime);
        var now = DateTime.UtcNow;

        if (normalizedStart <= now)
        {
            return AppointmentSchedulingResult.Failure(
                "No se pueden agendar citas en fechas u horas pasadas.",
                await SuggestNextAvailableSlotsAsync(doctorId, normalizedStart, 5, appointmentId));
        }

        var doctor = await _dbContext.Doctors
            .Include(x => x.Specialty)
            .FirstOrDefaultAsync(x => x.Id == doctorId && x.IsActive);

        if (doctor is null)
            return AppointmentSchedulingResult.Failure("El médico indicado no existe o está inactivo.");

        var patientExists = await _dbContext.Patients.AnyAsync(x => x.Id == patientId && x.IsActive);
        if (!patientExists)
            return AppointmentSchedulingResult.Failure("El paciente indicado no existe o está inactivo.");

        var durationMinutes = ResolveDurationMinutes(doctor.Specialty);
        var calculatedEnd = normalizedStart.AddMinutes(durationMinutes);

        var schedule = await GetMatchingScheduleAsync(doctorId, normalizedStart);
        if (schedule is null)
        {
            return AppointmentSchedulingResult.Failure(
                "No se puede agendar la cita fuera del horario de consulta del médico.",
                await SuggestNextAvailableSlotsAsync(doctorId, normalizedStart, 5, appointmentId));
        }

        if (normalizedStart.TimeOfDay < schedule.StartTime || calculatedEnd.TimeOfDay > schedule.EndTime)
        {
            return AppointmentSchedulingResult.Failure(
                "La cita solicitada queda fuera del horario disponible del médico.",
                await SuggestNextAvailableSlotsAsync(doctorId, normalizedStart, 5, appointmentId));
        }

        var hasConflict = await _dbContext.Appointments
            .AnyAsync(x => x.DoctorId == doctorId
                           && x.Id != appointmentId
                           && x.IsActive
                           && x.Status != "Cancelled"
                           && normalizedStart < x.EndDateTime
                           && calculatedEnd > x.StartDateTime);

        if (hasConflict)
        {
            return AppointmentSchedulingResult.Failure(
                "El médico ya tiene una cita en el horario solicitado.",
                await SuggestNextAvailableSlotsAsync(doctorId, normalizedStart, 5, appointmentId));
        }

        AppointmentEntity entity;
        if (appointmentId.HasValue)
        {
            entity = await _dbContext.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId.Value)
                     ?? throw new InvalidOperationException("Appointment to update was not found.");

            entity.ModifiedAt = now;
        }
        else
        {
            entity = new AppointmentEntity
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
                ModifiedAt = now
            };
            _dbContext.Appointments.Add(entity);
        }

        entity.DoctorId = doctorId;
        entity.PatientId = patientId;
        entity.StartDateTime = normalizedStart;
        entity.EndDateTime = calculatedEnd;
        entity.DurationMinutes = durationMinutes;
        entity.Reason = reason;
        entity.Status = status;
        entity.CancellationReason = cancellationReason;
        entity.IsActive = isActive;

        await _dbContext.SaveChangesAsync();

        var cancellationCount = await CountRecentPatientCancellationsAsync(patientId, now);
        var savedAppointment = await _dbContext.Appointments
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .FirstAsync(x => x.Id == entity.Id);

        return AppointmentSchedulingResult.Success(savedAppointment, cancellationCount >= 3, cancellationCount);
    }

    /// <summary>
    /// Obtiene el horario activo del médico para la fecha solicitada.
    /// </summary>
    private async Task<DoctorScheduleEntity?> GetMatchingScheduleAsync(Guid doctorId, DateTime requestedStartDateTime)
    {
        var targetDay = NormalizeDayOfWeek(requestedStartDateTime.DayOfWeek);

        var schedules = await _dbContext.DoctorSchedules
            .Where(x => x.DoctorId == doctorId && x.IsActive && x.DayOfWeek == targetDay)
            .ToListAsync();

        return schedules
            .OrderBy(x => x.StartTime)
            .FirstOrDefault(x => requestedStartDateTime.TimeOfDay >= x.StartTime && requestedStartDateTime.TimeOfDay < x.EndTime);
    }

    /// <summary>
    /// Cuenta las citas canceladas del paciente dentro de los últimos 30 días.
    /// </summary>
    private async Task<int> CountRecentPatientCancellationsAsync(Guid patientId, DateTime now)
    {
        var cutoffDate = now.AddDays(-30);

        return await _dbContext.Appointments.CountAsync(x =>
            x.PatientId == patientId
            && x.Status == "Cancelled"
            && x.ModifiedAt >= cutoffDate);
    }

    /// <summary>
    /// Sugiere los próximos horarios disponibles de un médico considerando su agenda y citas vigentes.
    /// </summary>
    private async Task<IReadOnlyCollection<DateTime>> SuggestNextAvailableSlotsAsync(
        Guid doctorId,
        DateTime requestedStartDateTime,
        int maxSuggestions,
        Guid? ignoredAppointmentId)
    {
        var doctor = await _dbContext.Doctors
            .Include(x => x.Specialty)
            .FirstOrDefaultAsync(x => x.Id == doctorId && x.IsActive);

        if (doctor is null)
            return Array.Empty<DateTime>();

        var durationMinutes = ResolveDurationMinutes(doctor.Specialty);
        var activeSchedules = (await _dbContext.DoctorSchedules
            .Where(x => x.DoctorId == doctorId && x.IsActive)
            .ToListAsync())
            .OrderBy(x => x.DayOfWeek)
            .ThenBy(x => x.StartTime)
            .ToList();

        if (activeSchedules.Count == 0)
            return Array.Empty<DateTime>();

        var searchStart = NormalizeDateTime(requestedStartDateTime);
        var now = DateTime.UtcNow;
        if (searchStart < now)
            searchStart = now;

        var bookedAppointments = await _dbContext.Appointments
            .Where(x => x.DoctorId == doctorId
                        && x.Id != ignoredAppointmentId
                        && x.IsActive
                        && x.Status != "Cancelled"
                        && x.EndDateTime >= searchStart.Date)
            .OrderBy(x => x.StartDateTime)
            .ToListAsync();

        var suggestions = new List<DateTime>();
        var dateCursor = searchStart.Date;
        var endDate = dateCursor.AddDays(60);

        while (dateCursor <= endDate && suggestions.Count < maxSuggestions)
        {
            var daySchedules = activeSchedules
                .Where(x => x.DayOfWeek == NormalizeDayOfWeek(dateCursor.DayOfWeek))
                .OrderBy(x => x.StartTime)
                .ToList();

            foreach (var schedule in daySchedules)
            {
                var slotStart = dateCursor.Add(schedule.StartTime);
                if (dateCursor == searchStart.Date && slotStart < searchStart)
                {
                    var deltaMinutes = (int)Math.Ceiling((searchStart - slotStart).TotalMinutes / durationMinutes);
                    if (deltaMinutes > 0)
                        slotStart = slotStart.AddMinutes(deltaMinutes * durationMinutes);
                }

                while (slotStart.AddMinutes(durationMinutes) <= dateCursor.Add(schedule.EndTime))
                {
                    var slotEnd = slotStart.AddMinutes(durationMinutes);
                    var hasConflict = bookedAppointments.Any(x => slotStart < x.EndDateTime && slotEnd > x.StartDateTime);
                    if (!hasConflict && slotStart > now)
                        suggestions.Add(slotStart);

                    if (suggestions.Count >= maxSuggestions)
                        break;

                    slotStart = slotStart.AddMinutes(durationMinutes);
                }

                if (suggestions.Count >= maxSuggestions)
                    break;
            }

            dateCursor = dateCursor.AddDays(1);
        }

        return suggestions;
    }

    /// <summary>
    /// Resuelve la duración oficial de una cita según la especialidad del médico.
    /// </summary>
    private static int ResolveDurationMinutes(SpecialtyEntity specialty)
    {
        return specialty.Name.Trim().ToLowerInvariant() switch
        {
            "medicina general" => 20,
            "cardiología" => 30,
            "cardiologia" => 30,
            "cirugía" => 45,
            "cirugia" => 45,
            "pediatría" => 20,
            "pediatria" => 20,
            "ginecología" => 30,
            "ginecologia" => 30,
            _ => specialty.AppointmentDurationMinutes
        };
    }

    /// <summary>
    /// Normaliza un <see cref="DateTime"/> al uso de UTC cuando el valor llega sin zona definida.
    /// </summary>
    private static DateTime NormalizeDateTime(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Convierte el enum de .NET al formato 1 = lunes ... 7 = domingo usado en la base de datos.
    /// </summary>
    private static int NormalizeDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
    }
}

/// <summary>
/// Representa el resultado de agendar o actualizar una cita médica.
/// </summary>
public sealed class AppointmentSchedulingResult
{
    private AppointmentSchedulingResult(bool succeeded, string? errorMessage, AppointmentEntity? appointment, IReadOnlyCollection<DateTime>? suggestedSlots, bool hasCancellationAlert, int recentCancellationCount)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        Appointment = appointment;
        SuggestedSlots = suggestedSlots ?? Array.Empty<DateTime>();
        HasCancellationAlert = hasCancellationAlert;
        RecentCancellationCount = recentCancellationCount;
    }

    /// <summary>
    /// Indica si la operación se completó exitosamente.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Obtiene el mensaje de error de negocio cuando la operación falla.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Obtiene la cita persistida cuando la operación se realiza correctamente.
    /// </summary>
    public AppointmentEntity? Appointment { get; }

    /// <summary>
    /// Obtiene la lista de horarios sugeridos cuando el horario solicitado no está disponible.
    /// </summary>
    public IReadOnlyCollection<DateTime> SuggestedSlots { get; }

    /// <summary>
    /// Indica si el paciente alcanzó el umbral de alertas por cancelaciones recientes.
    /// </summary>
    public bool HasCancellationAlert { get; }

    /// <summary>
    /// Obtiene el total de cancelaciones recientes del paciente en los últimos 30 días.
    /// </summary>
    public int RecentCancellationCount { get; }

    /// <summary>
    /// Crea un resultado exitoso con la cita persistida.
    /// </summary>
    public static AppointmentSchedulingResult Success(AppointmentEntity appointment, bool hasCancellationAlert, int recentCancellationCount)
        => new(true, null, appointment, null, hasCancellationAlert, recentCancellationCount);

    /// <summary>
    /// Crea un resultado fallido con un mensaje de validación y sugerencias opcionales.
    /// </summary>
    public static AppointmentSchedulingResult Failure(string errorMessage, IReadOnlyCollection<DateTime>? suggestedSlots = null)
        => new(false, errorMessage, null, suggestedSlots, false, 0);
}
