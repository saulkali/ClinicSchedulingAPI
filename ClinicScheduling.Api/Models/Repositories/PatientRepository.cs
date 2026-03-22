using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public PatientRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<PatientEntity>> GetAllAsync() =>
        await _dbContext.Patients
            .Include(x => x.User)
            .AsNoTracking()
            .ToListAsync();

    public async Task<PatientEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Patients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<PatientEntity> CreateAsync(PatientEntity entity)
    {
        _dbContext.Patients.Add(entity);
        await _dbContext.SaveChangesAsync();
        return await GetByIdAsync(entity.Id) ?? entity;
    }

    public async Task<PatientEntity?> UpdateAsync(PatientEntity entity)
    {
        var existing = await _dbContext.Patients.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.UserId = entity.UserId;
        existing.Name = entity.Name;
        existing.BirthDate = entity.BirthDate;
        existing.Phone = entity.Phone;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(existing.Id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Patients.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Patients.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
