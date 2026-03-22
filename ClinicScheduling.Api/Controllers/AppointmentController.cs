using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _repository;

    public AppointmentController(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _repository.GetAllAsync();
        return Ok(appointments.Select(MapResponse));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        return appointment is null ? NotFound() : Ok(MapResponse(appointment));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AppointmentDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndDateTime <= request.StartDateTime)
            return BadRequest("EndDateTime debe ser mayor que StartDateTime.");

        var entity = new AppointmentEntity
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            PatientId = request.PatientId,
            StartDateTime = request.StartDateTime,
            EndDateTime = request.EndDateTime,
            DurationMinutes = request.DurationMinutes,
            Reason = request.Reason,
            Status = request.Status,
            CancellationReason = request.CancellationReason,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repository.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapResponse(created));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndDateTime <= request.StartDateTime)
            return BadRequest("EndDateTime debe ser mayor que StartDateTime.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        existing.DoctorId = request.DoctorId;
        existing.PatientId = request.PatientId;
        existing.StartDateTime = request.StartDateTime;
        existing.EndDateTime = request.EndDateTime;
        existing.DurationMinutes = request.DurationMinutes;
        existing.Reason = request.Reason;
        existing.Status = request.Status;
        existing.CancellationReason = request.CancellationReason;
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

    private static AppointmentDtos.Response MapResponse(AppointmentEntity entity) => new()
    {
        Id = entity.Id,
        DoctorId = entity.DoctorId,
        DoctorName = entity.Doctor?.Name ?? string.Empty,
        PatientId = entity.PatientId,
        PatientName = entity.Patient?.Name ?? string.Empty,
        StartDateTime = entity.StartDateTime,
        EndDateTime = entity.EndDateTime,
        DurationMinutes = entity.DurationMinutes,
        Reason = entity.Reason,
        Status = entity.Status,
        CancellationReason = entity.CancellationReason,
        IsActive = entity.IsActive,
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt
    };
}
