using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public DoctorRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<DoctorEntity>> GetAllAsync() =>
        await _dbContext.Doctors
            .Include(x => x.User)
            .Include(x => x.Specialty)
            .AsNoTracking()
            .ToListAsync();

    public async Task<DoctorEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Doctors
            .Include(x => x.User)
            .Include(x => x.Specialty)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<DoctorEntity> CreateAsync(DoctorEntity entity)
    {
        _dbContext.Doctors.Add(entity);
        await _dbContext.SaveChangesAsync();
        return await GetByIdAsync(entity.Id) ?? entity;
    }

    public async Task<DoctorEntity?> UpdateAsync(DoctorEntity entity)
    {
        var existing = await _dbContext.Doctors.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.UserId = entity.UserId;
        existing.SpecialtyId = entity.SpecialtyId;
        existing.Name = entity.Name;
        existing.Phone = entity.Phone;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Doctors.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Doctors.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
