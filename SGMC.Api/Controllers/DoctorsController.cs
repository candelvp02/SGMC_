using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<OperationResult<List<DoctorDto>>>> Search(
    [FromQuery] string? name, [FromQuery] short? specialtyId)
        {
            var result = await _doctorService.SearchAsync(name, specialtyId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<OperationResult<List<DoctorDto>>>> GetAll()
        {
            var result = await _doctorService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("with-details")]
        public async Task<ActionResult<OperationResult<List<DoctorDto>>>> GetAllWithDetails()
        {
            var result = await _doctorService.GetAllWithDetailsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperationResult<DoctorDto>>> GetById(int id)
        {
            var result = await _doctorService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/with-details")]
        public async Task<ActionResult<OperationResult<DoctorDto>>> GetByIdWithDetails(int id)
        {
            var result = await _doctorService.GetByIdWithDetailsAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetAppointments(int id)
        {
            var result = await _doctorService.GetAppointmentsByDoctorIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult<DoctorDto>>> Create([FromBody] RegisterDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _doctorService.CreateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Datos?.DoctorId }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OperationResult<DoctorDto>>> Update(int id, [FromBody] UpdateDoctorDto dto)
        {
            if (id != dto.DoctorId)
                return BadRequest(OperationResult.Fallo("ID no coincide"));

            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _doctorService.UpdateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("specialty/{specialtyId}")]
        public async Task<ActionResult<OperationResult<List<DoctorDto>>>> GetBySpecialty(short specialtyId)
        {
            var result = await _doctorService.GetBySpecialtyIdAsync(specialtyId);
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<ActionResult<OperationResult<List<DoctorDto>>>> GetActive()
        {
            var result = await _doctorService.GetActiveDoctorsAsync();
            return Ok(result);
        }

        [HttpGet("license/{licenseNumber}")]
        public async Task<ActionResult<OperationResult<DoctorDto>>> GetByLicenseNumber(string licenseNumber)
        {
            var result = await _doctorService.GetByLicenseNumberAsync(licenseNumber);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("license/{licenseNumber}/exists")]
        public async Task<ActionResult<OperationResult<bool>>> ExistsByLicenseNumber(string licenseNumber)
        {
            var result = await _doctorService.ExistsByLicenseNumberAsync(licenseNumber);
            return Ok(result);
        }
    }
}