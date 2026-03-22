using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class DoctorControllerTests
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
        var response = await _client.GetAsync($"/api/doctor/{_factory.ExistingDoctorId}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/doctor", new DoctorDtos.Create { UserId = Guid.Empty, SpecialtyId = Guid.Empty, Name = "" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExists()
    {
        var response = await _client.PutAsJsonAsync($"/api/doctor/{_factory.ExistingDoctorId}", new DoctorDtos.Update
        {
            UserId = _factory.AvailableDoctorUserId,
            SpecialtyId = _factory.ExistingSpecialtyId,
            Name = "Dr. Banner",
            Phone = "5553339999",
            IsActive = true
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.DeleteAsync($"/api/doctor/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
