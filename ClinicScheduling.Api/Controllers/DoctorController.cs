using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorRepository _repository;

    public DoctorController(IDoctorRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var doctors = await _repository.GetAllAsync();
        return Ok(doctors.Select(MapResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var doctor = await _repository.GetByIdAsync(id);
        return doctor is null ? NotFound() : Ok(MapResponse(doctor));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DoctorDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DoctorDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new DoctorEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SpecialtyId = request.SpecialtyId,
            Name = request.Name,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DoctorDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DoctorDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.UserId = request.UserId;
        existing.SpecialtyId = request.SpecialtyId;
        existing.Name = request.Name;
        existing.Phone = request.Phone;
        existing.IsActive = request.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(MapResponse(updated));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    private static DoctorDtos.Response MapResponse(DoctorEntity entity) => new()
    {
        Id = entity.Id,
        UserId = entity.UserId,
        UserEmail = entity.User?.Email ?? string.Empty,
        SpecialtyId = entity.SpecialtyId,
        SpecialtyName = entity.Specialty?.Name ?? string.Empty,
        Name = entity.Name,
        Phone = entity.Phone,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt
    };
}
