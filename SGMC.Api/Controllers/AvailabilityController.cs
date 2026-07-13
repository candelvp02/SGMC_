using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(IAvailabilityService availabilityService, ILogger<AvailabilityController> logger)
        {
            _availabilityService = availabilityService;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OperationResult<AvailabilityDto>>> GetById(int id)
        {
            var result = await _availabilityService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId:int}")]
        public async Task<ActionResult<OperationResult<List<AvailabilityDto>>>> GetByDoctor(int doctorId)
        {
            var result = await _availabilityService.GetByDoctorIdAsync(doctorId);
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId:int}/range")]
        public async Task<ActionResult<OperationResult<List<AvailabilityDto>>>> GetByDoctorAndRange(
            int doctorId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _availabilityService.GetByDoctorAndDateRangeAsync(doctorId, startDate, endDate);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult<AvailabilityDto>>> Create([FromBody] CreateAvailabilityDto dto)
        {
            var result = await _availabilityService.CreateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Datos?.AvailabilityId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<OperationResult<AvailabilityDto>>> Update(int id, [FromBody] UpdateAvailabilityDto dto)
        {
            if (id != dto.AvailabilityId)
                return BadRequest(OperationResult.Fallo("El ID de la ruta no coincide con el ID del cuerpo."));

            var result = await _availabilityService.UpdateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            var result = await _availabilityService.DeleteAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }
    }
}