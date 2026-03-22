using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Models.Repositories;

public class AuthRepository:IAuthRepository
{
    private readonly ILogger<AuthRepository> _logger;
    private readonly ClinicSchedulingDbContext _dbContext;
    
    public AuthRepository(ClinicSchedulingDbContext dbContext, ILogger<AuthRepository> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<UserEntity?> GetUserByEmailAsync(string email) => await _dbContext.Users.AsNoTracking()
        .Include(x => x.Role)
        .FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
}