using System.Data;
using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Common.Enums;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public AppointmentRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AppointmentEntity>> GetAllAsync() =>
        await _dbContext.Appointments
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .AsNoTracking()
            .ToListAsync();

    public async Task<AppointmentEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Appointments
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<IEnumerable<AppointmentEntity>> GetByPatientIdAsync(Guid patientId) =>
        await _dbContext.Appointments
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .Where(x => x.PatientId == patientId)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IEnumerable<AppointmentEntity>> GetByDoctorIdAsync(Guid doctorId)
    {
        var doctorIdParameter = new SqlParameter("@DoctorId", SqlDbType.UniqueIdentifier)
        {
            Value = doctorId
        };

        var appointments = await _dbContext.Appointments
            .FromSqlRaw("EXEC dbo.sp_GetAppointmentsByDoctor @DoctorId", doctorIdParameter)
            .AsNoTracking()
            .ToListAsync();

        var appointmentIds = appointments.Select(x => x.Id).ToList();

        if (!appointmentIds.Any())
            return appointments;

        return await _dbContext.Appointments
            .Where(x => appointmentIds.Contains(x.Id))
            .Include(x => x.Doctor)
            .Include(x => x.Patient)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AppointmentDtos.AppointmentAviableDoctorDto> GetDoctorAvailabilityAsync(Guid doctorId, DateTime date)
    {
    
        var doctorIdParameter = new SqlParameter("@DoctorId", SqlDbType.UniqueIdentifier) { Value = doctorId };
        var dateParameter = new SqlParameter("@Date", SqlDbType.Date) { Value = date.Date };

        await using var connection = _dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.sp_GetDoctorAvailability";
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add(doctorIdParameter);
        command.Parameters.Add(dateParameter);

        var result = new AppointmentDtos.AppointmentAviableDoctorDto
        {
            DoctorId = doctorId,
            Date = date.Date,
            DayOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek
        };

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.DoctorId = reader.GetGuid(reader.GetOrdinal("DoctorId"));
            result.Date = reader.GetDateTime(reader.GetOrdinal("Date"));
            result.DayOfWeek = reader.GetInt32(reader.GetOrdinal("DayOfWeek"));
            result.DurationMinutes = reader.GetInt32(reader.GetOrdinal("DurationMinutes"));
            result.AvailableSlots.Add(new AppointmentDtos.DoctorAvailableSlotDto
            {
                StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
                EndDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime"))
            });
        }
        return result;
    }

    // lo deje este metodo fue el primero en hacer las validaciones a nivel de linq y EF
    // abajo se dejo el que usa directamente el SP de sql server que paso a sustituir este metodo (eliminar esto se dejo con fines de prueba tecnica)
    // public async Task<AppointmentEntity> CreateAsync(AppointmentEntity entity)
    // {
    //     var doctor = await _dbContext.Doctors
    //         .Include(x => x.Specialty)
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(x => x.Id == entity.DoctorId && x.IsActive);
    //
    //     if (doctor is null)
    //         throw new InvalidOperationException("No se encontró un doctor activo para la cita.");
    //
    //     if (entity.EndDateTime <= entity.StartDateTime)
    //         throw new InvalidOperationException("EndDateTime debe ser mayor que StartDateTime.");
    //
    //     var appointmentDuration = doctor.Specialty.AppointmentDurationMinutes;
    //     var expectedEndDateTime = entity.StartDateTime.AddMinutes(appointmentDuration);
    //
    //     if (entity.DurationMinutes != 0 && entity.DurationMinutes != appointmentDuration)
    //         throw new InvalidOperationException($"La duración de la cita para la especialidad del doctor debe ser de {appointmentDuration} minutos.");
    //
    //     if (entity.EndDateTime != expectedEndDateTime)
    //         throw new InvalidOperationException($"La cita debe durar {appointmentDuration} minutos y finalizar a las {expectedEndDateTime:HH:mm}.");
    //
    //     var appointmentDayOfWeek = entity.StartDateTime.DayOfWeek == DayOfWeek.Sunday
    //         ? 7
    //         : (int)entity.StartDateTime.DayOfWeek;
    //
    //     var startTime = entity.StartDateTime.TimeOfDay;
    //     var endTime = expectedEndDateTime.TimeOfDay;
    //     
    //     var scheduleList = await _dbContext.DoctorSchedules.Where(x => x.DoctorId == entity.DoctorId && x.IsActive).ToListAsync();
    //     var scheduleExists = scheduleList
    //         .Where(x =>
    //             x.DayOfWeek == appointmentDayOfWeek &&
    //             startTime >= x.StartTime &&
    //             endTime <= x.EndTime)
    //         .FirstOrDefault() != null;
    //     if (!scheduleExists)
    //         throw new InvalidOperationException("No se puede agendar una cita con el doctor porque no está dentro del horario laboral registrado.");
    //     
    //     var appointmentConflict = await _dbContext.Appointments
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(x =>
    //             x.IsActive &&
    //             x.DoctorId == entity.DoctorId &&
    //             x.Status == nameof(AppointmentStatus.Scheduled) &&
    //             x.StartDateTime < expectedEndDateTime &&
    //             x.EndDateTime > entity.StartDateTime);
    //
    //     if (appointmentConflict != null)
    //         throw new InvalidOperationException(
    //             "No se puede agendar la cita porque el doctor ya tiene una cita reservada en ese horario.");
    //     
    //     entity.DurationMinutes = appointmentDuration;
    //     entity.EndDateTime = expectedEndDateTime;
    //
    //     _dbContext.Appointments.Add(entity);
    //     await _dbContext.SaveChangesAsync();
    //     return await GetByIdAsync(entity.Id) ?? entity;
    // }
    
    public async Task<AppointmentEntity> CreateAsync(AppointmentEntity entity)
    {
        try
        {
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            if (entity.CreatedAt == default)
                entity.CreatedAt = DateTime.UtcNow;

            entity.ModifiedAt = DateTime.UtcNow;
            entity.IsActive = true;

            if (string.IsNullOrWhiteSpace(entity.Status))
                entity.Status = nameof(AppointmentStatus.Scheduled);

            var parameters = new[]
            {
                new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = entity.Id },
                new SqlParameter("@DoctorId", SqlDbType.UniqueIdentifier) { Value = entity.DoctorId },
                new SqlParameter("@PatientId", SqlDbType.UniqueIdentifier) { Value = entity.PatientId },
                new SqlParameter("@StartDateTime", SqlDbType.DateTime2) { Value = entity.StartDateTime },
                new SqlParameter("@EndDateTime", SqlDbType.DateTime2) { Value = entity.EndDateTime },
                new SqlParameter("@DurationMinutes", SqlDbType.Int) { Value = entity.DurationMinutes },
                new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = entity.Status },
                new SqlParameter("@CreatedAt", SqlDbType.DateTime2) { Value = entity.CreatedAt },
                new SqlParameter("@ModifiedAt", SqlDbType.DateTime2) { Value = entity.ModifiedAt },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
            };

            var result = await _dbContext.Appointments
                .FromSqlRaw(
                    @"EXEC dbo.sp_CreateAppointment
                        @Id,
                        @DoctorId,
                        @PatientId,
                        @StartDateTime,
                        @EndDateTime,
                        @DurationMinutes,
                        @Status,
                        @CreatedAt,
                        @ModifiedAt,
                        @IsActive",
                    parameters)
                .AsNoTracking()
                .ToListAsync();

            var created = result.FirstOrDefault()
                ?? throw new InvalidOperationException("No fue posible crear la cita.");

            return await GetByIdAsync(created.Id) ?? created; // solo valida que si se crea el registro o solo seria retornar
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(ex.Message);
        }
    }
    public async Task<AppointmentEntity?> UpdateAsync(AppointmentEntity entity)
    {
        var existing = await _dbContext.Appointments.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.DoctorId = entity.DoctorId;
        existing.PatientId = entity.PatientId;
        existing.StartDateTime = entity.StartDateTime;
        existing.EndDateTime = entity.EndDateTime;
        existing.DurationMinutes = entity.DurationMinutes;
        existing.Reason = entity.Reason;
        existing.Status = entity.Status;
        existing.CancellationReason = entity.CancellationReason;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Appointments.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Appointments.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
