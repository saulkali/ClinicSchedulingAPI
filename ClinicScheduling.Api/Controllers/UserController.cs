using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;
using ClinicScheduling.Api.Models.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace ClinicScheduling.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UserController(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <summary>
    /// Obtiene todos los usuarios registrados en la plataforma.
    /// </summary>
    /// <returns>Listado de usuarios.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<UserDtos.Response>>(users));
    }

    /// <summary>
    /// Obtiene un usuario específico por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <returns>El usuario encontrado o un resultado 404 si no existe.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);

        if (user is null)
            return NotFound();

        return Ok(_mapper.Map<UserDtos.Response>(user));
    }

    /// <summary>
    /// Crea un nuevo usuario con su información base y contraseña inicial.
    /// </summary>
    /// <param name="request">Datos del usuario a registrar.</param>
    /// <returns>El usuario creado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<UserEntity>(request);
        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<UserDtos.Response>(created));
    }

    /// <summary>
    /// Actualiza la información de un usuario existente.
    /// </summary>
    /// <param name="id">Identificador único del usuario a actualizar.</param>
    /// <param name="request">Nuevos datos del usuario.</param>
    /// <returns>El usuario actualizado o un resultado 404 si no existe.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserDtos.Update request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return NotFound();

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);

        if (updated is null)
            return NotFound();

        return Ok(_mapper.Map<UserDtos.Response>(updated));
    }

    /// <summary>
    /// Elimina un usuario por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del usuario a eliminar.</param>
    /// <returns>204 si se elimina correctamente o 404 si no existe.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
