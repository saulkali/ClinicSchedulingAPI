using ClinicScheduling.Api.Common.Database.Context;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Models.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Repositories;

[TestFixture]
public class AppointmentRepositoryTests
{
    private SqliteConnection _connection = null!;
    private ClinicSchedulingDbContext _dbContext = null!;
    private AppointmentRepository _repository = null!;
    private Guid _doctorId;
    private Guid _patientId;

    [SetUp]
    public void SetUp()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ClinicSchedulingDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ClinicSchedulingDbContext(options);
        _dbContext.Database.EnsureCreated();

        SeedDoctorAndPatient();

        _repository = new AppointmentRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Test]
    public void CreateAsync_ShouldThrowInvalidOperationException_WhenSameScheduleAlreadyBooked()
    {
        var start = new DateTime(2026, 3, 23, 10, 0, 0, DateTimeKind.Utc);
        SeedScheduledAppointment(start, start.AddMinutes(30));

        var newAppointment = new AppointmentEntity
        {
            DoctorId = _doctorId,
            PatientId = _patientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled",
            IsActive = true
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.CreateAsync(newAppointment));

        Assert.That(exception!.Message, Does.Contain("ya tiene una cita reservada en ese horario"));
    }

    [Test]
    public void CreateAsync_ShouldThrowInvalidOperationException_WhenAppointmentOverlapsExistingOne()
    {
        var existingStart = new DateTime(2026, 3, 23, 10, 0, 0, DateTimeKind.Utc);
        SeedScheduledAppointment(existingStart, existingStart.AddMinutes(30));

        var overlappingStart = new DateTime(2026, 3, 23, 10, 15, 0, DateTimeKind.Utc);
        var overlappingAppointment = new AppointmentEntity
        {
            DoctorId = _doctorId,
            PatientId = _patientId,
            StartDateTime = overlappingStart,
            EndDateTime = overlappingStart.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled",
            IsActive = true
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _repository.CreateAsync(overlappingAppointment));

        Assert.That(exception!.Message, Does.Contain("ya tiene una cita reservada en ese horario"));
    }

    [Test]
    public async Task CreateAsync_ShouldCreateAppointment_WhenNewAppointmentStartsAtExistingEndTime()
    {
        var existingStart = new DateTime(2026, 3, 23, 10, 0, 0, DateTimeKind.Utc);
        SeedScheduledAppointment(existingStart, existingStart.AddMinutes(30));

        var nextStart = new DateTime(2026, 3, 23, 10, 30, 0, DateTimeKind.Utc);
        var newAppointment = new AppointmentEntity
        {
            DoctorId = _doctorId,
            PatientId = _patientId,
            StartDateTime = nextStart,
            EndDateTime = nextStart.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled",
            IsActive = true
        };

        var result = await _repository.CreateAsync(newAppointment);

        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.StartDateTime, Is.EqualTo(nextStart));
        Assert.That(result.EndDateTime, Is.EqualTo(nextStart.AddMinutes(30)));
    }

    private void SeedDoctorAndPatient()
    {
        var specialty = new SpecialtyEntity
        {
            Id = Guid.NewGuid(),
            Name = "Cardiology",
            AppointmentDurationMinutes = 30,
            IsActive = true
        };

        var doctorUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "doctor-repository@clinic.com",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = true
        };

        var patientUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "patient-repository@clinic.com",
            PasswordHash = "hash",
            RoleId = Guid.NewGuid(),
            IsActive = true
        };

        var doctor = new DoctorEntity
        {
            Id = Guid.NewGuid(),
            UserId = doctorUser.Id,
            SpecialtyId = specialty.Id,
            Name = "Dr. Repo",
            IsActive = true
        };

        var patient = new PatientEntity
        {
            Id = Guid.NewGuid(),
            UserId = patientUser.Id,
            Name = "Patient Repo",
            IsActive = true
        };

        var mondaySchedule = new DoctorScheduleEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            DayOfWeek = 1,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(13, 0, 0),
            IsActive = true
        };

        _dbContext.Specialties.Add(specialty);
        _dbContext.Users.AddRange(doctorUser, patientUser);
        _dbContext.Doctors.Add(doctor);
        _dbContext.Patients.Add(patient);
        _dbContext.DoctorSchedules.Add(mondaySchedule);
        _dbContext.SaveChanges();

        _doctorId = doctor.Id;
        _patientId = patient.Id;
    }

    private void SeedScheduledAppointment(DateTime start, DateTime end)
    {
        _dbContext.Appointments.Add(new AppointmentEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = _doctorId,
            PatientId = _patientId,
            StartDateTime = start,
            EndDateTime = end,
            DurationMinutes = 30,
            Status = "Scheduled",
            IsActive = true
        });

        _dbContext.SaveChanges();
    }
}
