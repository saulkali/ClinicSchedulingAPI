namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class CORSInjection
{
    public static void AddCorsInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin();
            });
        });
    }
}