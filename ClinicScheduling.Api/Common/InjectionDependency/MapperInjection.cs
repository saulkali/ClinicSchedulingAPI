using ClinicScheduling.Api.Common.MapperProfiles;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class MapperInjection
{
    public static void AddMapperProfiles(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(cfg => { }, typeof(AppointmentProfileMapper));
    }
}