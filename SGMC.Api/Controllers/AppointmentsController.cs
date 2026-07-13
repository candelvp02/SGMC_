using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetAll()
        {
            try
            {
                var result = await _appointmentService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las citas");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al obtener las citas.")

                );
            }

        }

        //GET: api/appointments/5

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OperationResult<AppointmentDto>>> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(OperationResult.Fallo("El ID debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.GetByIdAsync(id);
                if (!result.Exitoso || result.Datos is null)
                    return NotFound(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la cita con ID {id}.", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al obtener la cita.")
                );
            }
        }

        //POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<OperationResult<AppointmentDto>>> Create([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            try
            {
                var result = await _appointmentService.CreateAsync(dto);
                if (!result.Exitoso || result.Datos is null)
                    return BadRequest(result);

                var newId = result.Datos.AppointmentId;

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = newId },
                    result
                );
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear una nueva cita.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al crear la cita.")
                );
            }
        }

        // PUT: api/appointments/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OperationResult<AppointmentDto>>> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos invalidos"));

            if (id != dto.Id)
                return BadRequest(OperationResult.Fallo("El ID de la ruta no coincide con el ID del cuerpo."));

            try
            {
                var result = await _appointmentService.UpdateAsync(dto);
                if (!result.Exitoso)
                    return BadRequest(result);

                return Ok(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la cita con ID {Id}.", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al ")
                );
            }
        }

        //Delete: api/Appointments/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<OperationResult>> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(OperationResult.Fallo("El ID debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.DeleteAsync(id);
                if (!result.Exitoso)
                    return BadRequest(result);
                return Ok(result);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la cita con ID {Id}.", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al eliminar la cita.")
                );
            }
        }

        //GET: api/Appointments/patient/1
        [HttpGet("patient/{patientId:int}")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetByPatient(int patientId)
        {
            if (patientId <= 0)
                return BadRequest(OperationResult.Fallo("El ID del paciente debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.GetByPatientIdAsync(patientId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas para el paciente con ID {PatientId}.", patientId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al obtener las citas del paciente.")
                );
            }
        }

        // GET: api/appointments/me
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Paciente")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetMyAppointments(
            [FromQuery] int? statusId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out int patientId))
                return Unauthorized(OperationResult.Fallo("No se pudo identificar al paciente autenticado."));

            try
            {
                var result = await _appointmentService.GetByPatientIdAsync(patientId);
                if (!result.Exitoso || result.Datos == null)
                    return Ok(result);

                // Filtros opcionales aplicados en memoria (los datos ya vienen del paciente)
                var appointments = result.Datos.AsEnumerable();

                if (statusId.HasValue)
                    appointments = appointments.Where(a => a.StatusId == statusId.Value);

                if (from.HasValue)
                    appointments = appointments.Where(a => a.AppointmentDate >= from.Value);

                if (to.HasValue)
                    appointments = appointments.Where(a => a.AppointmentDate <= to.Value);

                var list = appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();

                return Ok(OperationResult<List<AppointmentDto>>.Exito(
                    list,
                    list.Count == 0
                        ? "No tienes citas registradas."
                        : $"{list.Count} cita(s) encontrada(s)."
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial de citas del paciente {PatientId}.", patientId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al obtener tu historial de citas.")
                );
            }
        }

        // GET: api/Appointments/doctor/1
        [HttpGet("doctor/{doctorId:int}")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetByDoctor(int doctorId)
        {
            if (doctorId <= 0)
                return BadRequest(OperationResult.Fallo("El ID del doctor debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.GetByDoctorIdAsync(doctorId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas para el doctor con ID {DoctorId}.", doctorId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    OperationResult.Fallo("Se produjo un error inesperado al obtener las citas del doctor.")
                );
            }
        }
    }
}