using AutoMapper;
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
    private readonly IMapper _mapper;

    public AppointmentController(IAppointmentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene la lista completa de citas registradas en el sistema.
    /// </summary>
    /// <returns>Listado de citas médicas.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<AppointmentDtos.Response>>(appointments));
    }

    /// <summary>
    /// Obtiene el detalle de una cita específica mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador único de la cita.</param>
    /// <returns>La cita encontrada o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        return Ok(_mapper.Map<AppointmentDtos.Response>(appointment));
    }

    /// <summary>
    /// Obtiene el historial de citas de un paciente usando su identificador.
    /// </summary>
    /// <param name="patientId">Identificador único del paciente.</param>
    /// <returns>Listado de citas médicas del paciente.</returns>
    [HttpGet("patient/{patientId:guid}")]
    public async Task<IActionResult> GetByPatientId(Guid patientId)
    {
        var appointments = await _repository.GetByPatientIdAsync(patientId);
        var response = _mapper.Map<IEnumerable<AppointmentDtos.Response>>(appointments);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene las citas activas de un doctor usando su identificador.
    /// </summary>
    /// <param name="doctorId">Identificador único del doctor.</param>
    /// <returns>Listado de citas médicas del doctor.</returns>
    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<IActionResult> GetByDoctorId(Guid doctorId)
    {
        var appointments = await _repository.GetByDoctorIdAsync(doctorId);
        var response = _mapper.Map<IEnumerable<AppointmentDtos.DoctorBusySlotResponse>>(appointments);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene los bloques de horario disponibles para un doctor en una fecha específica,
    /// considerando su horario laboral, la duración de la especialidad y las citas ya ocupadas.
    /// </summary>
    /// <param name="doctorId">Identificador único del doctor.</param>
    /// <param name="date">Fecha exacta a consultar (yyyy-MM-dd).</param>
    /// <returns>Listado de horarios disponibles para agendar.</returns>
    [HttpGet("doctor/{doctorId:guid}/availability")]
    public async Task<IActionResult> GetDoctorAvailability(Guid doctorId, [FromQuery] DateTime date)
    {
        if (date == default)
            return BadRequest("date es requerido y debe tener formato yyyy-MM-dd.");

        var availability = await _repository.GetDoctorAvailabilityAsync(doctorId, date.Date);
        return Ok(availability);
    }

    /// <summary>
    /// Crea una nueva cita médica con la información enviada en la solicitud.
    /// </summary>
    /// <param name="request">Datos necesarios para registrar la cita.</param>
    /// <returns>La cita creada con su identificador generado.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<AppointmentEntity>(request);

        try
        {
            var created = await _repository.CreateAsync(entity);

            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                _mapper.Map<AppointmentDtos.Response>(created));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza la información de una cita existente.
    /// </summary>
    /// <param name="id">Identificador único de la cita a actualizar.</param>
    /// <param name="request">Nuevos datos de la cita.</param>
    /// <returns>La cita actualizada o un resultado 404 si no existe.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndDateTime <= request.StartDateTime)
            return BadRequest("EndDateTime debe ser mayor que StartDateTime.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);

        return Ok(_mapper.Map<AppointmentDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina una cita médica del sistema por su identificador.
    /// </summary>
    /// <param name="id">Identificador único de la cita a eliminar.</param>
    /// <returns>204 si se elimina correctamente o 404 si no existe.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
