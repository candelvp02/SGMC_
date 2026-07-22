using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Medical;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMedicalRecordService _medicalRecordService;

        public MedicalRecordsController(IMedicalRecordService medicalRecordService)
        {
            _medicalRecordService = medicalRecordService;
        }

        [HttpGet]
        public async Task<ActionResult<OperationResult<List<MedicalRecordDto>>>> GetAll()
        {
            var result = await _medicalRecordService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OperationResult<MedicalRecordDto>>> GetById(int id)
        {
            var result = await _medicalRecordService.GetByIdAsync(id);
            if (!result.Exitoso)
                return NotFound(result);
            return Ok(result);
        }

        [HttpGet("patient/{patientId:int}")]
        public async Task<ActionResult<OperationResult<List<MedicalRecordDto>>>> GetByPatient(int patientId)
        {
            var result = await _medicalRecordService.GetByPatientIdAsync(patientId);
            return Ok(result);
        }

        [HttpGet("doctor/{doctorId:int}")]
        public async Task<ActionResult<OperationResult<List<MedicalRecordDto>>>> GetByDoctor(int doctorId)
        {
            var result = await _medicalRecordService.GetByDoctorIdAsync(doctorId);
            return Ok(result);
        }

        // Acceso al historial desde la Agenda del Médico (solo lectura)
        [HttpGet("doctor/{doctorId:int}/patient/{patientId:int}/history")]
        public async Task<ActionResult<OperationResult<List<MedicalRecordDto>>>> GetPatientHistoryForDoctor(int doctorId, int patientId)
        {
            var result = await _medicalRecordService.GetPatientHistoryForDoctorAsync(doctorId, patientId);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult<MedicalRecordDto>>> Create([FromBody] CreateMedicalRecordDto dto)
        {
            var result = await _medicalRecordService.CreateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Datos?.RecordId }, result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<OperationResult<MedicalRecordDto>>> Update(int id, [FromBody] UpdateMedicalRecordDto dto)
        {
            if (id != dto.RecordId)
                return BadRequest(OperationResult.Fallo("El ID de la ruta no coincide con el ID del cuerpo."));

            var result = await _medicalRecordService.UpdateAsync(dto);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            var result = await _medicalRecordService.DeleteAsync(id);
            if (!result.Exitoso)
                return BadRequest(result);
            return Ok(result);
        }
    }
}