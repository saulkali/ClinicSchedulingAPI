using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorScheduleController : ControllerBase
{
    private readonly IDoctorScheduleRepository _repository;

    public DoctorScheduleController(IDoctorScheduleRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduleDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _repository.GetAllAsync();
        return Ok(schedules.Select(MapResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schedule = await _repository.GetByIdAsync(id);
        return schedule is null ? NotFound() : Ok(MapResponse(schedule));
    }

    [HttpPost]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DoctorScheduleDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndTime <= request.StartTime)
            return BadRequest("EndTime debe ser mayor que StartTime.");

        var entity = new DoctorScheduleEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] DoctorScheduleDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndTime <= request.StartTime)
            return BadRequest("EndTime debe ser mayor que StartTime.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.DoctorId = request.DoctorId;
        existing.DayOfWeek = request.DayOfWeek;
        existing.StartTime = request.StartTime;
        existing.EndTime = request.EndTime;
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

    private static DoctorScheduleDtos.Response MapResponse(DoctorScheduleEntity entity) => new()
    {
        Id = entity.Id,
        DoctorId = entity.DoctorId,
        DoctorName = entity.Doctor?.Name ?? string.Empty,
        DayOfWeek = entity.DayOfWeek,
        StartTime = entity.StartTime,
        EndTime = entity.EndTime,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt
    };
}
