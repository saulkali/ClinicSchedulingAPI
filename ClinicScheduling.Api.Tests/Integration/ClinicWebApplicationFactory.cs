using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ClinicScheduling.Api.Tests.Integration;

public class ClinicWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    public Guid ExistingRoleId { get; private set; }
    public Guid ExistingSecondaryRoleId { get; private set; }
    public Guid ExistingUserId { get; private set; }
    public Guid ExistingSecondaryUserId { get; private set; }
    public Guid AvailableDoctorUserId { get; private set; }
    public Guid AvailablePatientUserId { get; private set; }
    public Guid ExistingSpecialtyId { get; private set; }
    public Guid ExistingDoctorId { get; private set; }
    public Guid ExistingPatientId { get; private set; }
    public Guid ExistingDoctorScheduleId { get; private set; }
    public Guid ExistingAppointmentId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ClinicSchedulingDbContext>>();
            services.RemoveAll<ClinicSchedulingDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ClinicSchedulingDbContext>(options => options.UseSqlite(_connection));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClinicSchedulingDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            SeedData(dbContext);
        });
    }

    private void SeedData(ClinicSchedulingDbContext dbContext)
    {
        var doctorRole = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Doctor",
            IsActive = true
        };

        var patientRole = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Patient",
            IsActive = true
        };

        ExistingRoleId = doctorRole.Id;
        ExistingSecondaryRoleId = patientRole.Id;

        dbContext.Roles.AddRange(doctorRole, patientRole);

        var specialty = new SpecialtyEntity
        {
            Id = Guid.NewGuid(),
            Name = "Cardiology",
            AppointmentDurationMinutes = 30,
            IsActive = true
        };

        ExistingSpecialtyId = specialty.Id;
        dbContext.Specialties.Add(specialty);

        var activeUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "doctor@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = true
        };

        var inactiveUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "inactive@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = false
        };

        var patientUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "patient@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = patientRole.Id,
            IsActive = true
        };

        var availableDoctorUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "available-doctor@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = true
        };

        var availablePatientUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "available-patient@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = patientRole.Id,
            IsActive = true
        };

        ExistingUserId = activeUser.Id;
        ExistingSecondaryUserId = patientUser.Id;
        AvailableDoctorUserId = availableDoctorUser.Id;
        AvailablePatientUserId = availablePatientUser.Id;

        dbContext.Users.AddRange(activeUser, inactiveUser, patientUser, availableDoctorUser, availablePatientUser);

        var doctor = new DoctorEntity
        {
            Id = Guid.NewGuid(),
            UserId = activeUser.Id,
            SpecialtyId = specialty.Id,
            Name = "Dr. Strange",
            Phone = "5551112233",
            IsActive = true
        };

        ExistingDoctorId = doctor.Id;
        dbContext.Doctors.Add(doctor);

        var patient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            Name = "Peter Parker",
            BirthDate = new DateTime(2001, 8, 10),
            Phone = "5559998877",
            IsActive = true
        };

        ExistingPatientId = patient.Id;
        dbContext.Patients.Add(patient);

        var schedule = new DoctorScheduleEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(13, 0, 0),
            IsActive = true
        };

        ExistingDoctorScheduleId = schedule.Id;
        dbContext.DoctorSchedules.Add(schedule);

        var appointment = new AppointmentEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            StartDateTime = new DateTime(2026, 3, 23, 10, 0, 0, DateTimeKind.Utc),
            EndDateTime = new DateTime(2026, 3, 23, 10, 30, 0, DateTimeKind.Utc),
            DurationMinutes = 30,
            Reason = "Annual checkup",
            Status = "Scheduled",
            IsActive = true
        };

        ExistingAppointmentId = appointment.Id;
        dbContext.Appointments.Add(appointment);

        dbContext.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection?.Dispose();

        base.Dispose(disposing);
    }
}
