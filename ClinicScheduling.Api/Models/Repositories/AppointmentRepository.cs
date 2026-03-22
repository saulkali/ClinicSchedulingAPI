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
