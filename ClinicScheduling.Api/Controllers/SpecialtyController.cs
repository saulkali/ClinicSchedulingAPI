using AutoMapper;
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
    private readonly IMapper _mapper;

    public SpecialtyController(ISpecialtyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todas las especialidades médicas registradas.
    /// </summary>
    /// <returns>Listado de especialidades.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var specialties = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<SpecialtyDtos.Response>>(specialties));
    }

    /// <summary>
    /// Obtiene una especialidad médica por su identificador.
    /// </summary>
    /// <param name="id">Identificador único de la especialidad.</param>
    /// <returns>La especialidad encontrada o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecialtyDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var specialty = await _repository.GetByIdAsync(id);
        return specialty is null ? NotFound() : Ok(_mapper.Map<SpecialtyDtos.Response>(specialty));
    }

    /// <summary>
    /// Crea una nueva especialidad médica.
    /// </summary>
    /// <param name="request">Datos de la especialidad a registrar.</param>
    /// <returns>La especialidad creada.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SpecialtyDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] SpecialtyDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<SpecialtyEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<SpecialtyDtos.Response>(created));
    }

    /// <summary>
    /// Actualiza una especialidad médica existente.
    /// </summary>
    /// <param name="id">Identificador único de la especialidad a actualizar.</param>
    /// <param name="request">Nuevos datos de la especialidad.</param>
    /// <returns>La especialidad actualizada o un resultado 404 si no existe.</returns>
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

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(_mapper.Map<SpecialtyDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina una especialidad médica por su identificador.
    /// </summary>
    /// <param name="id">Identificador único de la especialidad a eliminar.</param>
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
