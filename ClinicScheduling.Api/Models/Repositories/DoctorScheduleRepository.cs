using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class DoctorScheduleRepository : IDoctorScheduleRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public DoctorScheduleRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<DoctorScheduleEntity>> GetAllAsync() =>
        await _dbContext.DoctorSchedules
            .Include(x => x.Doctor)
            .AsNoTracking()
            .ToListAsync();

    public async Task<DoctorScheduleEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.DoctorSchedules
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<DoctorScheduleEntity> CreateAsync(DoctorScheduleEntity entity)
    {
        _dbContext.DoctorSchedules.Add(entity);
        await _dbContext.SaveChangesAsync();
        return await GetByIdAsync(entity.Id) ?? entity;
    }

    public async Task<DoctorScheduleEntity?> UpdateAsync(DoctorScheduleEntity entity)
    {
        var existing = await _dbContext.DoctorSchedules.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.DoctorId = entity.DoctorId;
        existing.DayOfWeek = entity.DayOfWeek;
        existing.StartTime = entity.StartTime;
        existing.EndTime = entity.EndTime;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.DoctorSchedules.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.DoctorSchedules.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
