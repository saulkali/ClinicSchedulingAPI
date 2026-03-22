using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Common.Database.Context;

public class ClinicSchedulingDbContext:DbContext
{
    public ClinicSchedulingDbContext(DbContextOptions<ClinicSchedulingDbContext> options): base(options)
    {
        
    }
    
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<DoctorEntity> Doctors { get; set; }
    public DbSet<PatientEntity> Patients { get; set; }
    public DbSet<SpecialtyEntity> Specialties { get; set; }
    public DbSet<DoctorScheduleEntity> DoctorSchedules { get; set; }
    public DbSet<AppointmentEntity> Appointments { get; set; }
}