using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IUserRepository
{
    Task<IEnumerable<UserEntity>> GetAllAsync();
    Task<UserEntity?> GetByIdAsync(Guid id);
    Task<UserEntity> CreateAsync(UserEntity entity);
    Task<UserEntity?> UpdateAsync(UserEntity entity);
    Task<bool> DeleteAsync(Guid id);
}