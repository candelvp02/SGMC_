using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly INotificationService _notificationService;

        public AuthController(IPatientService patientService, INotificationService notificationService)
        {
            _patientService = patientService;
            _notificationService = notificationService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<OperationResult<PatientDto>>> Register([FromBody] RegisterPatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos"));

            var result = await _patientService.CreateAsync(dto);

            if (!result.Exitoso)
                return BadRequest(result);

            // Enviar correo de activación tras el registro exitoso
            if (result.Datos != null)
            {
                await _notificationService.SendAccountActivationEmailAsync(
                    dto.Email,
                    result.Datos.PatientId
                );
            }

            return CreatedAtAction(nameof(GetRegistrationStatus),
                new { id = result.Datos?.PatientId }, result);
        }

        [HttpGet("status/{id}")]
        public async Task<ActionResult<OperationResult<bool>>> GetRegistrationStatus(int id)
        {
            var result = await _patientService.ExistsAsync(id);
            return Ok(result);
        }
    }
}