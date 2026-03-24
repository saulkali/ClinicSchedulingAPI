using AutoMapper;
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
    private readonly IMapper _mapper;

    public DoctorScheduleController(IDoctorScheduleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todos los horarios de atención médica registrados.
    /// </summary>
    /// <returns>Listado de horarios de doctores.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduleDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<DoctorScheduleDtos.Response>>(schedules));
    }

    /// <summary>
    /// Obtiene el detalle de un horario de doctor por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del horario.</param>
    /// <returns>El horario encontrado o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schedule = await _repository.GetByIdAsync(id);
        return schedule is null ? NotFound() : Ok(_mapper.Map<DoctorScheduleDtos.Response>(schedule));
    }

    /// <summary>
    /// Crea un nuevo horario de atención para un doctor.
    /// </summary>
    /// <param name="request">Datos del horario a registrar.</param>
    /// <returns>El horario creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] DoctorScheduleDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndTime <= request.StartTime)
            return BadRequest("EndTime debe ser mayor que StartTime.");

        var entity = _mapper.Map<DoctorScheduleEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<DoctorScheduleDtos.Response>(created));
    }

    /// <summary>
    /// Actualiza la información de un horario de doctor existente.
    /// </summary>
    /// <param name="id">Identificador único del horario a actualizar.</param>
    /// <param name="request">Nuevos datos del horario.</param>
    /// <returns>El horario actualizado o un resultado 404 si no existe.</returns>
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

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(_mapper.Map<DoctorScheduleDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina un horario de doctor por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del horario a eliminar.</param>
    /// <returns>204 si se elimina correctamente o 404 si no existe.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
