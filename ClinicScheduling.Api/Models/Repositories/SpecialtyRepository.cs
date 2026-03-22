using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class SpecialtyRepository : ISpecialtyRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public SpecialtyRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<SpecialtyEntity>> GetAllAsync() =>
        await _dbContext.Specialties
            .AsNoTracking()
            .ToListAsync();

    public async Task<SpecialtyEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Specialties.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<SpecialtyEntity> CreateAsync(SpecialtyEntity entity)
    {
        _dbContext.Specialties.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<SpecialtyEntity?> UpdateAsync(SpecialtyEntity entity)
    {
        var existing = await _dbContext.Specialties.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.Name = entity.Name;
        existing.AppointmentDurationMinutes = entity.AppointmentDurationMinutes;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Specialties.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Specialties.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
