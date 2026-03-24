using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly IRoleRepository _repository;
    private readonly IMapper _mapper;

    public RoleController(IRoleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todos los roles disponibles en la aplicación.
    /// </summary>
    /// <returns>Listado de roles.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<RoleDtos.Response>>(roles));
    }

    /// <summary>
    /// Obtiene un rol específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del rol.</param>
    /// <returns>El rol encontrado o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var role = await _repository.GetByIdAsync(id);
        return role is null ? NotFound() : Ok(_mapper.Map<RoleDtos.Response>(role));
    }

    /// <summary>
    /// Crea un nuevo rol dentro del sistema.
    /// </summary>
    /// <param name="request">Datos del rol a registrar.</param>
    /// <returns>El rol creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(RoleDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RoleDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<RoleEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<RoleDtos.Response>(created));
    }

    /// <summary>
    /// Actualiza un rol existente por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del rol a actualizar.</param>
    /// <param name="request">Nuevos datos del rol.</param>
    /// <returns>El rol actualizado o un resultado 404 si no existe.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RoleDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return NotFound();

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(_mapper.Map<RoleDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina un rol por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del rol a eliminar.</param>
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
