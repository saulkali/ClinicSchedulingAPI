using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class PatientControllerTests
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
        var response = await _client.GetAsync("/api/patient");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var response = await _client.PostAsJsonAsync("/api/patient", new PatientDtos.Create
        {
            UserId = _factory.AvailablePatientUserId,
            Name = "Miles Morales",
            BirthDate = new DateTime(2005, 1, 1),
            Phone = "5551234567"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExists()
    {
        var response = await _client.PutAsJsonAsync($"/api/patient/{_factory.ExistingPatientId}", new PatientDtos.Update
        {
            UserId = _factory.AvailablePatientUserId,
            Name = "Peter B. Parker",
            BirthDate = new DateTime(2001, 8, 10),
            Phone = "5550001234",
            IsActive = false
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenMissing()
    {
        var response = await _client.DeleteAsync($"/api/patient/{Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
