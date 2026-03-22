using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public RoleRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<RoleEntity>> GetAllAsync() =>
        await _dbContext.Roles
            .AsNoTracking()
            .ToListAsync();

    public async Task<RoleEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<RoleEntity> CreateAsync(RoleEntity entity)
    {
        _dbContext.Roles.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<RoleEntity?> UpdateAsync(RoleEntity entity)
    {
        var existing = await _dbContext.Roles.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.Name = entity.Name;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Roles.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Roles.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
