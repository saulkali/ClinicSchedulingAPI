using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class RoleControllerTests
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
    public async Task GetAll_ShouldReturnOk() =>
        Assert.That((await _client.GetAsync("/api/role")).StatusCode, Is.EqualTo(HttpStatusCode.OK));

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/role", new RoleDtos.Create { Name = "" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var response = await _client.PostAsJsonAsync("/api/role", new RoleDtos.Create { Name = "Admin" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenExists()
    {
        var response = await _client.PutAsJsonAsync($"/api/role/{_factory.ExistingRoleId}", new RoleDtos.Update { Name = "Doctor Updated", IsActive = false });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenExists()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/role", new RoleDtos.Create { Name = "Temp Role" });
        var created = await createResponse.Content.ReadFromJsonAsync<RoleDtos.Response>();

        var response = await _client.DeleteAsync($"/api/role/{created!.Id}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}
