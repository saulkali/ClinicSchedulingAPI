using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository repository, ILogger<UserController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repository.GetAllAsync();

        var response = users.Select(x => new UserDtos.Response
        {
            Id = x.Id,
            Email = x.Email,
            RoleId = x.RoleId,
            RoleName = x.Role?.Name ?? string.Empty,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            ModifiedAt = x.ModifiedAt
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        var response = new UserDtos.Response
        {
            Id = user.Id,
            Email = user.Email,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            ModifiedAt = user.ModifiedAt
        };

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);

        var response = new UserDtos.Response
        {
            Id = created.Id,
            Email = created.Email,
            RoleId = created.RoleId,
            RoleName = created.Role?.Name ?? string.Empty,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt,
            ModifiedAt = created.ModifiedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        existing.Email = request.Email;
        existing.RoleId = request.RoleId;
        existing.IsActive = request.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);

        if (updated is null)
            return NotFound();

        var response = new UserDtos.Response
        {
            Id = updated.Id,
            Email = updated.Email,
            RoleId = updated.RoleId,
            RoleName = updated.Role?.Name ?? string.Empty,
            IsActive = updated.IsActive,
            CreatedAt = updated.CreatedAt,
            ModifiedAt = updated.ModifiedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}