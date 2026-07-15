using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IAppointmentService _appointmentService;

        public PatientsController(IPatientService patientService, IAppointmentService appointmentService)
        {
            _patientService = patientService;
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<OperationResult<List<PatientDto>>>> GetAll()
        {
            var result = await _patientService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperationResult<PatientDto>>> GetById(int id)
        {
            var result = await _patientService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/details")]
        public async Task<ActionResult<OperationResult<PatientDto>>> GetByIdWithDetails(int id)
        {
            var result = await _patientService.GetByIdWithDetailsAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult<PatientDto>>> Create([FromBody] RegisterPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _patientService.CreateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Datos?.PatientId }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<OperationResult<PatientDto>>> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (id != dto.PatientId)
                return BadRequest(OperationResult.Fallo("ID no coincide"));

            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            var result = await _patientService.UpdateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<OperationResult<PatientDto>>> PatchContactInfo(int id, [FromBody] PatchPatientContactDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos"));

            var result = await _patientService.PatchContactInfoAsync(id, dto);
            if (!result.Exitoso)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPatch("{id}/insurance")]
        public async Task<ActionResult<OperationResult<PatientDto>>> PatchInsurance(int id, [FromBody] PatchPatientInsuranceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos"));

            var result = await _patientService.PatchInsuranceProviderAsync(id, dto);
            if (!result.Exitoso)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            var result = await _patientService.DeleteAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("active")]
        public async Task<ActionResult<OperationResult<List<PatientDto>>>> GetActive()
        {
            var result = await _patientService.GetActiveAsync();
            return Ok(result);
        }

        [HttpGet("phone/{phoneNumber}")]
        public async Task<ActionResult<OperationResult<PatientDto>>> GetByPhoneNumber(string phoneNumber)
        {
            var result = await _patientService.GetByPhoneNumberAsync(phoneNumber);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("insurance/{insuranceProviderId}")]
        public async Task<ActionResult<OperationResult<List<PatientDto>>>> GetByInsuranceProvider(int insuranceProviderId)
        {
            var result = await _patientService.GetByInsuranceProviderAsync(insuranceProviderId);
            return Ok(result);
        }

        [HttpGet("{id}/appointments")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetAppointments(int id)
        {
            var result = await _appointmentService.GetByPatientIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("{id}/exists")]
        public async Task<ActionResult<OperationResult<bool>>> Exists(int id)
        {
            var result = await _patientService.ExistsAsync(id);
            return Ok(result);
        }
    }
}