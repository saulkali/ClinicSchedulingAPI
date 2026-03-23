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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DoctorScheduleDtos.Response>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _repository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<DoctorScheduleDtos.Response>>(schedules));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorScheduleDtos.Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var schedule = await _repository.GetByIdAsync(id);
        return schedule is null ? NotFound() : Ok(_mapper.Map<DoctorScheduleDtos.Response>(schedule));
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

        var entity = _mapper.Map<DoctorScheduleEntity>(request);
        var created = await _repository.CreateAsync(entity);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            _mapper.Map<DoctorScheduleDtos.Response>(created));
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

        _mapper.Map(request, existing);

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? NotFound() : Ok(_mapper.Map<DoctorScheduleDtos.Response>(updated));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
