using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
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

    public async Task<AppointmentEntity> CreateAsync(AppointmentEntity entity)
    {
        var doctor = await _dbContext.Doctors
            .Include(x => x.Specialty)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entity.DoctorId && x.IsActive);

        if (doctor is null)
            throw new InvalidOperationException("No se encontró un doctor activo para la cita.");

        if (entity.EndDateTime <= entity.StartDateTime)
            throw new InvalidOperationException("EndDateTime debe ser mayor que StartDateTime.");

        var appointmentDuration = doctor.Specialty.AppointmentDurationMinutes;
        var expectedEndDateTime = entity.StartDateTime.AddMinutes(appointmentDuration);

        if (entity.DurationMinutes != 0 && entity.DurationMinutes != appointmentDuration)
            throw new InvalidOperationException($"La duración de la cita para la especialidad del doctor debe ser de {appointmentDuration} minutos.");

        if (entity.EndDateTime != expectedEndDateTime)
            throw new InvalidOperationException($"La cita debe durar {appointmentDuration} minutos y finalizar a las {expectedEndDateTime:HH:mm}.");

        var appointmentDayOfWeek = entity.StartDateTime.DayOfWeek == DayOfWeek.Sunday
            ? 7
            : (int)entity.StartDateTime.DayOfWeek;

        var startTime = entity.StartDateTime.TimeOfDay;
        var endTime = expectedEndDateTime.TimeOfDay;
        
        var scheduleList = await _dbContext.DoctorSchedules.Where(x => x.DoctorId == entity.DoctorId && x.IsActive).ToListAsync();
        var scheduleExists = scheduleList
            .Where(x =>
                x.DayOfWeek == appointmentDayOfWeek &&
                startTime >= x.StartTime &&
                endTime <= x.EndTime)
            .FirstOrDefault() != null;
        if (!scheduleExists)
            throw new InvalidOperationException("No se puede agendar una cita con el doctor porque no está dentro del horario laboral registrado.");

        entity.DurationMinutes = appointmentDuration;
        entity.EndDateTime = expectedEndDateTime;

        _dbContext.Appointments.Add(entity);
        await _dbContext.SaveChangesAsync();
        return await GetByIdAsync(entity.Id) ?? entity;
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
