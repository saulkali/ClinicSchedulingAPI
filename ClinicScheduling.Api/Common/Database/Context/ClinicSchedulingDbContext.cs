using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicScheduling.Api.Common.Database.Context;

public class ClinicSchedulingDbContext:DbContext
{
    public ClinicSchedulingDbContext(DbContextOptions<ClinicSchedulingDbContext> options): base(options)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppointmentEntity>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AppointmentEntity>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.NoAction);
    }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<DoctorEntity> Doctors { get; set; }
    public DbSet<PatientEntity> Patients { get; set; }
    public DbSet<SpecialtyEntity> Specialties { get; set; }
    public DbSet<DoctorScheduleEntity> DoctorSchedules { get; set; }
    public DbSet<AppointmentEntity> Appointments { get; set; }
}