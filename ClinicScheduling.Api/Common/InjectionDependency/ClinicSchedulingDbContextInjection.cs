using ClinicScheduling.Api.Common.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class ClinicSchedulingDbContextInjection
{
    public static void AddClinicSchedulingDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_CONNECTION_STRING") ?? configuration.GetConnectionString("ClinicSchedulingDb") ?? string.Empty;
        services.AddDbContext<ClinicSchedulingDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}