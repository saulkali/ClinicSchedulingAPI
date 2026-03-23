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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<AppointmentDtos.Response>>(appointments));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var appointment = await _repository.GetByIdAsync(id);
        if (appointment is null)
            return NotFound();

        return Ok(_mapper.Map<AppointmentDtos.Response>(appointment));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AppointmentDtos.Create request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.EndDateTime <= request.StartDateTime)
            return BadRequest("EndDateTime debe ser mayor que StartDateTime.");

        var entity = _mapper.Map<AppointmentEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<AppointmentDtos.Response>(created));
    }

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}