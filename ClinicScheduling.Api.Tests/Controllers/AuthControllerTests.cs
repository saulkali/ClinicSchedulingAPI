using System.Net;
using System.Net.Http.Json;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Tests.Integration;
using NUnit.Framework;

namespace ClinicScheduling.Api.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
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
    public async Task Login_ShouldReturnBadRequest_WhenRequestBodyIsInvalid()
    {
        var request = new AuthDtos.LoginRequest
        {
            Email = "",
            Password = ""
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenEmailDoesNotExist()
    {
        var request = new AuthDtos.LoginRequest
        {
            Email = "missing@clinic.com",
            Password = "123456"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
    {
        var request = new AuthDtos.LoginRequest
        {
            Email = "doctor@clinic.com",
            Password = "wrong-password"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenUserIsInactive()
    {
        var request = new AuthDtos.LoginRequest
        {
            Email = "inactive@clinic.com",
            Password = "123456"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
    {
        var request = new AuthDtos.LoginRequest
        {
            Email = "doctor@clinic.com",
            Password = "123456"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadFromJsonAsync<AuthDtos.LoginResponse>();

        Assert.That(content, Is.Not.Null);
        Assert.That(content!.UserId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(content.Email, Is.EqualTo("doctor@clinic.com"));
        Assert.That(content.Role, Is.EqualTo("Doctor"));
        Assert.That(content.Token, Is.Not.Null.And.Not.Empty);
    }
}