using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecialtyController : ControllerBase
{
    private readonly ISpecialtyRepository _repository;

    public SpecialtyController(ISpecialtyRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var specialties = await _repository.GetAllAsync();
        return Ok(specialties.Select(MapResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecialtyDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var specialty = await _repository.GetByIdAsync(id);
        return specialty is null ? NotFound() : Ok(MapResponse(specialty));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SpecialtyDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SpecialtyDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = new SpecialtyEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            AppointmentDurationMinutes = request.AppointmentDurationMinutes,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SpecialtyDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpecialtyDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.Name = request.Name;
        existing.AppointmentDurationMinutes = request.AppointmentDurationMinutes;
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

    private static SpecialtyDtos.Response MapResponse(SpecialtyEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        AppointmentDurationMinutes = entity.AppointmentDurationMinutes,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt
    };
}
