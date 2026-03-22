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
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetById_ShouldReturnOk_WhenExists()
    {
        var response = await _client.GetAsync($"/api/appointment/{_factory.ExistingAppointmentId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenStartDateIsInThePast()
    {
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.AvailablePatientId,
            StartDateTime = DateTime.UtcNow.AddHours(-1),
            EndDateTime = DateTime.UtcNow,
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenDoctorHasAnOverlappingAppointment_AndSuggestSlots()
    {
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.AvailablePatientId,
            StartDateTime = _factory.ExistingAppointmentStartUtc.AddMinutes(15),
            EndDateTime = _factory.ExistingAppointmentStartUtc.AddMinutes(45),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var suggestions = body.GetProperty("suggestedSlots").EnumerateArray().ToList();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(suggestions, Has.Count.GreaterThanOrEqualTo(1));
        });
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenRequestedOutsideDoctorSchedule()
    {
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.AvailablePatientId,
            StartDateTime = _factory.GetNextWeekdayStartUtc(DayOfWeek.Monday, 13, 45),
            EndDateTime = _factory.GetNextWeekdayStartUtc(DayOfWeek.Monday, 14, 15),
            DurationMinutes = 30,
            Status = "Scheduled"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldAssignDurationFromSpecialty_AndIncludeCancellationAlert()
    {
        var requestedStart = _factory.GetNextWeekdayStartUtc(DayOfWeek.Tuesday, 9, 0);
        var response = await _client.PostAsJsonAsync("/api/appointment", new AppointmentDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            PatientId = _factory.ExistingPatientId,
            StartDateTime = requestedStart,
            EndDateTime = requestedStart.AddMinutes(10),
            DurationMinutes = 10,
            Status = "Scheduled",
            Reason = "Control"
        });

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDtos.Response>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(appointment, Is.Not.Null);
            Assert.That(appointment!.DurationMinutes, Is.EqualTo(30));
            Assert.That(appointment.EndDateTime, Is.EqualTo(requestedStart.AddMinutes(30)));
            Assert.That(appointment.HasCancellationAlert, Is.True);
            Assert.That(appointment.RecentCancellationCount, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExistsAndRequestedSlotIsValid()
    {
        var start = _factory.GetNextWeekdayStartUtc(DayOfWeek.Wednesday, 11, 0);
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

        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDtos.Response>();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(appointment, Is.Not.Null);
            Assert.That(appointment!.StartDateTime, Is.EqualTo(start));
            Assert.That(appointment.DurationMinutes, Is.EqualTo(30));
        });
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.DeleteAsync($"/api/appointment/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
