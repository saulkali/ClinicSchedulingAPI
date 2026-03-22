using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
    public Guid AvailableDoctorId { get; private set; }
    public Guid AvailablePatientId { get; private set; }
    public Guid SchedulableDoctorUserId { get; private set; }
    public Guid SchedulablePatientUserId { get; private set; }
    public Guid ExistingDoctorScheduleId { get; private set; }
    public Guid ExistingAppointmentId { get; private set; }
    public DateTime ExistingAppointmentStartUtc { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ClinicSchedulingDbContext>));
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            var dbContextOptionsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ClinicSchedulingDbContext>));
            if (dbContextOptionsDescriptor is not null)
                services.Remove(dbContextOptionsDescriptor);

            services.RemoveAll<ClinicSchedulingDbContext>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);
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

        var schedulableDoctorUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "schedulable-doctor@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = doctorRole.Id,
            IsActive = true
        };

        var schedulablePatientUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "schedulable-patient@clinic.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            RoleId = patientRole.Id,
            IsActive = true
        };

        ExistingUserId = activeUser.Id;
        ExistingSecondaryUserId = patientUser.Id;
        AvailableDoctorUserId = availableDoctorUser.Id;
        AvailablePatientUserId = availablePatientUser.Id;
        SchedulableDoctorUserId = schedulableDoctorUser.Id;
        SchedulablePatientUserId = schedulablePatientUser.Id;

        dbContext.Users.AddRange(activeUser, inactiveUser, patientUser, availableDoctorUser, availablePatientUser, schedulableDoctorUser, schedulablePatientUser);

        var doctor = new DoctorEntity
        {
            Id = Guid.NewGuid(),
            UserId = activeUser.Id,
            SpecialtyId = specialty.Id,
            Name = "Dr. Strange",
            Phone = "5551112233",
            IsActive = true
        };

        var availableDoctor = new DoctorEntity
        {
            Id = Guid.NewGuid(),
            UserId = schedulableDoctorUser.Id,
            SpecialtyId = specialty.Id,
            Name = "Dr. Banner",
            Phone = "5552223344",
            IsActive = true
        };

        ExistingDoctorId = doctor.Id;
        AvailableDoctorId = availableDoctor.Id;
        dbContext.Doctors.AddRange(doctor, availableDoctor);

        var patient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            Name = "Peter Parker",
            BirthDate = new DateTime(2001, 8, 10),
            Phone = "5559998877",
            IsActive = true
        };

        var availablePatient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            UserId = schedulablePatientUser.Id,
            Name = "Bruce Wayne",
            BirthDate = new DateTime(1980, 2, 19),
            Phone = "5558887766",
            IsActive = true
        };

        ExistingPatientId = patient.Id;
        AvailablePatientId = availablePatient.Id;
        dbContext.Patients.AddRange(patient, availablePatient);

        var nextMonday = GetNextWeekdayUtc(DateTime.UtcNow, DayOfWeek.Monday).Date;

        var schedules = new List<DoctorScheduleEntity>();
        for (var day = 1; day <= 5; day++)
        {
            schedules.Add(new DoctorScheduleEntity
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                DayOfWeek = day,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(14, 0, 0),
                IsActive = true
            });
        }

        ExistingDoctorScheduleId = schedules[0].Id;
        dbContext.DoctorSchedules.AddRange(schedules);

        ExistingAppointmentStartUtc = nextMonday.AddHours(10);
        var appointment = new AppointmentEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            PatientId = patient.Id,
            StartDateTime = ExistingAppointmentStartUtc,
            EndDateTime = ExistingAppointmentStartUtc.AddMinutes(30),
            DurationMinutes = 30,
            Reason = "Annual checkup",
            Status = "Scheduled",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            ModifiedAt = DateTime.UtcNow.AddDays(-3)
        };

        ExistingAppointmentId = appointment.Id;
        dbContext.Appointments.Add(appointment);

        for (var index = 1; index <= 3; index++)
        {
            var cancelledStart = DateTime.UtcNow.Date.AddDays(index).AddHours(9);
            dbContext.Appointments.Add(new AppointmentEntity
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                StartDateTime = cancelledStart,
                EndDateTime = cancelledStart.AddMinutes(30),
                DurationMinutes = 30,
                Reason = $"Cancelled #{index}",
                Status = "Cancelled",
                CancellationReason = "Paciente canceló",
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-index),
                ModifiedAt = DateTime.UtcNow.AddDays(-index)
            });
        }

        dbContext.SaveChanges();
    }

    public DateTime GetNextWeekdayStartUtc(DayOfWeek dayOfWeek, int hour, int minute = 0)
    {
        return GetNextWeekdayUtc(DateTime.UtcNow, dayOfWeek).Date.AddHours(hour).AddMinutes(minute);
    }

    private static DateTime GetNextWeekdayUtc(DateTime currentUtc, DayOfWeek targetDay)
    {
        var start = currentUtc.Date.AddDays(1);
        while (start.DayOfWeek != targetDay)
            start = start.AddDays(1);

        return DateTime.SpecifyKind(start, DateTimeKind.Utc);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _connection?.Dispose();

        base.Dispose(disposing);
    }
}
