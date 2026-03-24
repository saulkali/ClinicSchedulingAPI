using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class AppointmentControllerTests
{
    private ClinicWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new ClinicWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        var response = await _client.GetAsync($"/api/appointment/{_factory.ExistingAppointmentId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetByPatientId_ShouldReturnOkWithAppointments_WhenExists()
    {
        var response = await _client.GetAsync($"/api/appointment/patient/{_factory.ExistingPatientId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadFromJsonAsync<List<AppointmentDtos.Response>>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload, Is.Not.Empty);
        Assert.That(payload!.Any(x => x.PatientId == _factory.ExistingPatientId), Is.True);
    }

    [Test]
    public async Task GetByDoctorId_ShouldReturnOkWithAppointments_WhenExists()
    {
        var response = await _client.GetAsync($"/api/appointment/doctor/{_factory.ExistingDoctorId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);

        Assert.That(json.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(json.RootElement.GetArrayLength(), Is.GreaterThan(0));

        var firstAppointment = json.RootElement[0];
        Assert.That(firstAppointment.TryGetProperty("doctorId", out var doctorIdProperty), Is.True);
        Assert.That(doctorIdProperty.GetGuid(), Is.EqualTo(_factory.ExistingDoctorId));
        Assert.That(firstAppointment.TryGetProperty("reason", out _), Is.False);
        Assert.That(firstAppointment.TryGetProperty("cancellationReason", out _), Is.False);
        Assert.That(firstAppointment.TryGetProperty("patientId", out _), Is.False);
    }

    [Test]
    public async Task GetDoctorAvailability_ShouldReturnOkWithSlots_WhenValidDayOfWeek()
    {
        var response = await _client.GetAsync($"/api/appointment/doctor/{_factory.ExistingDoctorId}/availability/1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var payload = await response.Content.ReadFromJsonAsync<List<AppointmentDtos.AppointmentAviableDoctorDto>>();
        Assert.That(payload, Is.Not.Null);
        Assert.That(payload, Is.Not.Empty);

        Assert.That(payload!.All(x => x.DoctorId == _factory.ExistingDoctorId), Is.True);
        Assert.That(payload.All(x => x.DayOfWeek == 1), Is.True);
        Assert.That(payload.All(x => x.DurationMinutes == 30), Is.True);
        Assert.That(payload.First().StartTime, Is.EqualTo(new TimeSpan(9, 0, 0)));
        Assert.That(payload.Last().EndTime, Is.EqualTo(new TimeSpan(13, 0, 0)));
    }

    [Test]
    public async Task GetDoctorAvailability_ShouldReturnBadRequest_WhenDayOfWeekIsInvalid()
    {
        var response = await _client.GetAsync($"/api/appointment/doctor/{_factory.ExistingDoctorId}/availability/9");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("dayOfWeek"));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDateRangeIsInvalid()
    {
        var start = new DateTime(2026, 4, 1, 9, 0, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(-30),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnCreated_WhenWithinDoctorSchedule()
    {
        var start = new DateTime(2026, 3, 23, 13, 0, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenOutsideDoctorSchedule()
    {
        var start = new DateTime(2026, 3, 23, 12, 50, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("horario laboral"));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDurationDoesNotMatchDoctorSpecialty()
    {
        var start = new DateTime(2026, 3, 23, 11, 30, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(20),
            DurationMinutes = 20,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("30 minutos"));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDoctorHasAppointmentAtSameTime()
    {
        var start = new DateTime(2026, 3, 23, 10, 0, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("ya tiene una cita reservada en ese horario"));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDoctorAppointmentOverlapsExistingOne()
    {
        var start = new DateTime(2026, 3, 23, 10, 15, 0, DateTimeKind.Utc);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(30),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("ya tiene una cita reservada en ese horario"));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExists()
    {
        var start = new DateTime(2026, 4, 2, 11, 0, 0, DateTimeKind.Utc);
        var response = await _client.PutAsJsonAsync($"/api/appointment/{_factory.ExistingAppointmentId}", new AppointmentDtos.Update
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = start,
            EndDateTime = start.AddMinutes(45),
            DurationMinutes = 45,
            Reason = "Follow up",
            Status = "Confirmed",
            CancellationReason = null,
            IsActive = true
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.DeleteAsync($"/api/appointment/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
