using ClinicScheduling.Api.Common.Security;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class JwtServiceInjection
{
    public static void AddJwtServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
    }
}