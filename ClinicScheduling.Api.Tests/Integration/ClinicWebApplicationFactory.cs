using System.Data;
using System.Text.RegularExpressions;
using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClinicScheduling.Api.Tests.Integration;

public class ClinicWebApplicationFactory : WebApplicationFactory<Program>
{
    private string? _testConnectionString;
    private string? _masterConnectionString;
    private string? _databaseName;

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

            var serverConnectionString =
                "Server=localhost,1433;User Id=sa;Password=Developer123;TrustServerCertificate=True;MultipleActiveResultSets=True;";

            _databaseName = $"ClinicSchedulingTests_{Guid.NewGuid():N}";
            _masterConnectionString = $"{serverConnectionString}Database=master;";
            _testConnectionString = $"{serverConnectionString}Database={_databaseName};";

            services.AddDbContext<ClinicSchedulingDbContext>(options =>
                options.UseSqlServer(_testConnectionString));

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ClinicSchedulingDbContext>();

            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            CreateStoredProcedures(dbContext);
            SeedData(dbContext);
        });
    }

    private void CreateStoredProcedures(ClinicSchedulingDbContext dbContext)
    {
        var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var storedProcedurePaths = new[]
        {
            Path.Combine(
                solutionRoot,
                "ClinicScheduling.Api",
                "Docs",
                "SqlServer",
                "SpDb",
                "sp_CreateAppointment.sql"),
            Path.Combine(
                solutionRoot,
                "ClinicScheduling.Api",
                "Docs",
                "SqlServer",
                "SpDb",
                "sp_GetAppointmentsByDoctor.sql")
        };

        foreach (var spPath in storedProcedurePaths)
        {
            if (!File.Exists(spPath))
                throw new FileNotFoundException($"No se encontró el archivo del stored procedure en: {spPath}");

            var script = File.ReadAllText(spPath);

            ExecuteSqlScriptInBatches(dbContext, script);
        }
    }

    private static void ExecuteSqlScriptInBatches(ClinicSchedulingDbContext dbContext, string script)
    {
        var batches = Regex.Split(
                script,
                @"^\s*GO\s*;$|^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(batch => !string.IsNullOrWhiteSpace(batch))
            .ToList();

        var connection = (SqlConnection)dbContext.Database.GetDbConnection();

        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose)
            connection.Open();

        try
        {
            foreach (var batch in batches)
            {
                using var command = connection.CreateCommand();
                command.CommandText = batch;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }
        finally
        {
            if (shouldClose)
                connection.Close();
        }
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
        if (disposing && !string.IsNullOrWhiteSpace(_masterConnectionString) && !string.IsNullOrWhiteSpace(_databaseName))
        {
            try
            {
                using var connection = new SqlConnection(_masterConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = $@"
IF DB_ID('{_databaseName}') IS NOT NULL
BEGIN
    ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [{_databaseName}];
END";
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }

        base.Dispose(disposing);
    }
}
