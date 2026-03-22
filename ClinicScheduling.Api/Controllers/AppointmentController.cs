using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using ClinicScheduling.Api.Models.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

/// <summary>
/// Expone operaciones para consultar, crear, actualizar y eliminar citas médicas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _repository;
    private readonly AppointmentSchedulingService _appointmentSchedulingService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de citas.
    /// </summary>
    public AppointmentController(IAppointmentRepository repository, AppointmentSchedulingService appointmentSchedulingService)
    {
        _repository = repository;
        _appointmentSchedulingService = appointmentSchedulingService;
    }

    /// <summary>
    /// Obtiene todas las citas registradas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _repository.GetAllAsync();
        return Ok(appointments.Select(entity => MapResponse(entity, false, 0)));
    }

    /// <summary>
    /// Obtiene una cita por su identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        return appointment is null ? NotFound() : Ok(MapResponse(appointment, false, 0));
    }

    /// <summary>
    /// Crea una cita médica aplicando las reglas de agenda del negocio.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] AppointmentDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _appointmentSchedulingService.ScheduleAsync(
            request.DoctorId,
            request.PatientId,
            request.StartDateTime,
            request.Status,
            request.Reason,
            request.CancellationReason);

        if (!result.Succeeded)
            return BadRequest(new { message = result.ErrorMessage, suggestedSlots = result.SuggestedSlots });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Appointment!.Id },
            MapResponse(result.Appointment!, result.HasCancellationAlert, result.RecentCancellationCount));
    }

    /// <summary>
    /// Actualiza una cita existente aplicando las reglas de agenda del negocio.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        var result = await _appointmentSchedulingService.UpdateAsync(
            id,
            request.DoctorId,
            request.PatientId,
            request.StartDateTime,
            request.Status,
            request.Reason,
            request.CancellationReason,
            request.IsActive);

        if (!result.Succeeded)
            return BadRequest(new { message = result.ErrorMessage, suggestedSlots = result.SuggestedSlots });

        return Ok(MapResponse(result.Appointment!, result.HasCancellationAlert, result.RecentCancellationCount));
    }

    /// <summary>
    /// Elimina una cita por su identificador.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Convierte una entidad de cita a su DTO de respuesta.
    /// </summary>
    private static AppointmentDtos.Response MapResponse(AppointmentEntity entity, bool hasCancellationAlert, int recentCancellationCount) => new()
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
        ModifiedAt = entity.ModifiedAt,
        HasCancellationAlert = hasCancellationAlert,
        RecentCancellationCount = recentCancellationCount
    };
}
