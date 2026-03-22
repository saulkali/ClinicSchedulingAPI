using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class UserControllerTests
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
        var response = await _client.GetAsync("/api/user");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var users = await response.Content.ReadFromJsonAsync<List<UserDtos.Response>>();

        Assert.That(users, Is.Not.Null);
        Assert.That(users!.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(users.All(x => x.Id != Guid.Empty), Is.True);
        Assert.That(users.All(x => !string.IsNullOrWhiteSpace(x.Email)), Is.True);
    }

    [Test]
    public async Task GetById_ShouldReturnOk_WhenUserExists()
    {
        var response = await _client.GetAsync($"/api/user/{_factory.ExistingUserId}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var user = await response.Content.ReadFromJsonAsync<UserDtos.Response>();

        Assert.That(user, Is.Not.Null);
        Assert.That(user!.Id, Is.EqualTo(_factory.ExistingUserId));
        Assert.That(user.Email, Is.EqualTo("doctor@clinic.com"));
        Assert.That(user.RoleId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/user/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var request = new UserDtos.Create
        {
            Email = "",
            Password = "",
            RoleId = Guid.Empty
        };

        var response = await _client.PostAsJsonAsync("/api/user", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        var request = new UserDtos.Create
        {
            Email = "newuser@clinic.com",
            Password = "mypassword123",
            RoleId = _factory.ExistingRoleId
        };

        var response = await _client.PostAsJsonAsync("/api/user", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdUser = await response.Content.ReadFromJsonAsync<UserDtos.Response>();

        Assert.That(createdUser, Is.Not.Null);
        Assert.That(createdUser!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(createdUser.Email, Is.EqualTo("newuser@clinic.com"));
        Assert.That(createdUser.RoleId, Is.EqualTo(_factory.ExistingRoleId));
        Assert.That(createdUser.RoleName, Is.Not.Null);
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenRequestIsInvalid()
    {
        var request = new UserDtos.Update
        {
            Email = "",
            RoleId = Guid.Empty,
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync($"/api/user/{_factory.ExistingUserId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var missingId = Guid.NewGuid();

        var request = new UserDtos.Update
        {
            Email = "missing@clinic.com",
            RoleId = _factory.ExistingRoleId,
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync($"/api/user/{missingId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldReturnOk_WhenUserExists()
    {
        var request = new UserDtos.Update
        {
            Email = "updateduser@clinic.com",
            RoleId = _factory.ExistingRoleId,
            IsActive = false
        };

        var response = await _client.PutAsJsonAsync($"/api/user/{_factory.ExistingUserId}", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updatedUser = await response.Content.ReadFromJsonAsync<UserDtos.Response>();

        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser!.Id, Is.EqualTo(_factory.ExistingUserId));
        Assert.That(updatedUser.Email, Is.EqualTo("updateduser@clinic.com"));
        Assert.That(updatedUser.RoleId, Is.EqualTo(_factory.ExistingRoleId));
        Assert.That(updatedUser.IsActive, Is.False);
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var response = await _client.DeleteAsync($"/api/user/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenUserExists()
    {
        var createRequest = new UserDtos.Create
        {
            Email = "deleteuser@clinic.com",
            Password = "12345678",
            RoleId = _factory.ExistingRoleId
        };

        var createResponse = await _client.PostAsJsonAsync("/api/user", createRequest);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDtos.Response>();
        Assert.That(createdUser, Is.Not.Null);

        var deleteResponse = await _client.DeleteAsync($"/api/user/{createdUser!.Id}");

        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }
}