using System.Net;
using System.Net.Http.Json;
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
