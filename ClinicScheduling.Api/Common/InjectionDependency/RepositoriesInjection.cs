using ClinicScheduling.Api.Models.IRepositories;
using ClinicScheduling.Api.Models.Repositories;

namespace ClinicScheduling.Api.Common.InjectionDependency;

public static class RepositoriesInjection
{
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
    }
}
