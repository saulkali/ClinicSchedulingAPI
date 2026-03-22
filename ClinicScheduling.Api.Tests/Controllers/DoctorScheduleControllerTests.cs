using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class DoctorScheduleControllerTests
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
    public async Task GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/doctorschedule");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenTimeRangeIsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/doctorschedule", new DoctorScheduleDtos.Create
        {
            DoctorId = _factory.ExistingDoctorId,
            DayOfWeek = 2,
            StartTime = new TimeSpan(12, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExists()
    {
        var response = await _client.PutAsJsonAsync($"/api/doctorschedule/{_factory.ExistingDoctorScheduleId}", new DoctorScheduleDtos.Update
        {
            DoctorId = _factory.ExistingDoctorId,
            DayOfWeek = 3,
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            IsActive = false
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
