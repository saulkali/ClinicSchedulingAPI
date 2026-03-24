using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientController : ControllerBase
{
    private readonly IPatientRepository _repository;
    private readonly IMapper _mapper;

    public PatientController(IPatientRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene la lista de pacientes registrados.
    /// </summary>
    /// <returns>Listado de pacientes.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<PatientDtos.Response>>(patients));
    }

    /// <summary>
    /// Obtiene un paciente por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del paciente.</param>
    /// <returns>El paciente encontrado o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var patient = await _repository.GetByIdAsync(id);
        return patient is null ? NotFound() : Ok(_mapper.Map<PatientDtos.Response>(patient));
    }

    /// <summary>
    /// Crea un nuevo paciente en el sistema.
    /// </summary>
    /// <param name="request">Datos del paciente a registrar.</param>
    /// <returns>El paciente creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PatientDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] PatientDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<PatientEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<PatientDtos.Response>(created));
    }

    /// <summary>
    /// Actualiza la información de un paciente existente.
    /// </summary>
    /// <param name="id">Identificador único del paciente a actualizar.</param>
    /// <param name="request">Nuevos datos del paciente.</param>
    /// <returns>El paciente actualizado o un resultado 404 si no existe.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PatientDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] PatientDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(_mapper.Map<PatientDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina un paciente por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del paciente a eliminar.</param>
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
