using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ClinicSchedulingDbContext _dbContext;

    public UserRepository(ClinicSchedulingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<UserEntity>> GetAllAsync() =>
        await _dbContext.Users
            .Include(x => x.Role)
            .AsNoTracking()
            .ToListAsync();

    public async Task<UserEntity?> GetByIdAsync(Guid id) =>
        await _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<UserEntity> CreateAsync(UserEntity entity)
    {
        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<UserEntity?> UpdateAsync(UserEntity entity)
    {
        var existing = await _dbContext.Users.FindAsync(entity.Id);

        if (existing is null)
            return null;

        existing.Email = entity.Email;
        existing.RoleId = entity.RoleId;
        existing.IsActive = entity.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _dbContext.Users.FindAsync(id);

        if (entity is null)
            return false;

        _dbContext.Users.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}