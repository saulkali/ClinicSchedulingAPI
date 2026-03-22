using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IAuthRepository
{
    Task<UserEntity?> GetUserByEmailAsync(string email);
}