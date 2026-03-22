using ClinicScheduling.Api.Models.IRepositories;
using ClinicScheduling.Api.Models.Repositories;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class RepositoriesInjection
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository,AuthRepository>();
        services.AddScoped<IUserRepository,UserRepository>();
    }
}