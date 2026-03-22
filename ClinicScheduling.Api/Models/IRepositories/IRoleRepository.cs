using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IRoleRepository
{
    Task<IEnumerable<RoleEntity>> GetAllAsync();
    Task<RoleEntity?> GetByIdAsync(Guid id);
    Task<RoleEntity> CreateAsync(RoleEntity entity);
    Task<RoleEntity?> UpdateAsync(RoleEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
