using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Common.Security;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthRepository _authRepository;
    private readonly IJwtTokenService _jwtTokenService;
    
    public AuthController(IAuthRepository authRepository, ILogger<AuthController> logger, IJwtTokenService jwtTokenService)
    {
        _authRepository = authRepository;
        _logger = logger;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthDtos.LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] AuthDtos.LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest("Invalid request data.");

        try
        {
            var result = await _authRepository.GetUserByEmailAsync(request.Email);

            if (result is null)
                return Unauthorized(new { message = "Usuario o contraseña inválidos." });

            if (!result.VerifyPassword(request.Password))
                return Unauthorized(new { message = "Usuario o contraseña inválidos." });

            string token = _jwtTokenService.GenerateToken(
                result.Id,
                result.Email,
                result.Role.Name);

            return Ok(new AuthDtos.LoginResponse
            {
                UserId = result.Id,
                Email = result.Email,
                Role = result.Role.Name,
                Token = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(120)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error inesperado durante el login." });
        }
    }
}