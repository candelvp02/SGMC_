using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Domain.Repositories.Appointments;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorAvailabilityRepository _availabilityRepository;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IDoctorAvailabilityRepository availabilityRepository,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _availabilityRepository = availabilityRepository;
            _logger = logger;
        }

        // ── GET: api/appointments ─────────────────────────────────────────────
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
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener las citas."));
            }
        }

        // ── GET: api/appointments/5 ───────────────────────────────────────────
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
                _logger.LogError(ex, "Error al obtener cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener la cita."));
            }
        }

        // ── GET: api/appointments/doctor/5/pending ────────────────────────────
        // Task 138: listado de solicitudes Pendientes para el panel del médico
        [HttpGet("doctor/{doctorId:int}/pending")]
        public async Task<ActionResult<OperationResult<List<AppointmentDto>>>> GetPendingByDoctor(int doctorId)
        {
            if (doctorId <= 0)
                return BadRequest(OperationResult.Fallo("El ID del doctor debe ser mayor que cero."));

            try
            {
                var filter = new AppointmentFilterDto { DoctorId = doctorId, StatusId = 1 };
                var result = await _appointmentService.GetFilteredAppointmentsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitudes pendientes del doctor {Id}", doctorId);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener las solicitudes pendientes."));
            }
        }

        // ── PUT: api/appointments/5/confirm ───────────────────────────────────
        // Task 138: endpoint rápido de estados — confirmar solicitud
        [HttpPut("{id:int}/confirm")]
        public async Task<ActionResult<OperationResult>> Confirm(int id)
        {
            if (id <= 0)
                return BadRequest(OperationResult.Fallo("El ID debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.ConfirmAsync(id);
                if (!result.Exitoso)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al confirmar la cita."));
            }
        }

        // ── PUT: api/appointments/5/reject ────────────────────────────────────
        // Task 138: endpoint rápido de estados — rechazar solicitud
        [HttpPut("{id:int}/reject")]
        public async Task<ActionResult<OperationResult>> Reject(int id)
        {
            if (id <= 0)
                return BadRequest(OperationResult.Fallo("El ID debe ser mayor que cero."));

            try
            {
                var result = await _appointmentService.RejectAsync(id);
                if (!result.Exitoso)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al rechazar la cita."));
            }
        }

        // ── POST: api/appointments ────────────────────────────────────────────
        // Task 130: endpoint transaccional atómico
        [HttpPost]
        public async Task<ActionResult<OperationResult<AppointmentDto>>> Create([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos."));

            try
            {
                var result = await _appointmentService.CreateAsync(dto);
                if (!result.Exitoso || result.Datos is null)
                    return BadRequest(result);

                return CreatedAtAction(nameof(GetById), new { id = result.Datos.AppointmentId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cita");
                return StatusCode(500, OperationResult.Fallo("Error inesperado al crear la cita."));
            }
        }

        // ── GET: api/appointments/availability ───────────────────────────────
        // Task 131/132: el wizard consulta disponibilidad antes de mostrar horarios
        // y también permite re-verificar justo antes de confirmar
        [HttpGet("availability")]
        public async Task<ActionResult> GetAvailability(
            [FromQuery] int doctorId,
            [FromQuery] DateOnly date)
        {
            if (doctorId <= 0)
                return BadRequest(OperationResult.Fallo("El ID del doctor es inválido."));

            try
            {
                var slots = await _availabilityRepository.GetByDoctorAndDateRangeAsync(
                    doctorId, date, date);

                var available = slots
                    .Where(s => s.IsActive)
                    .Select(s => new
                    {
                        s.AvailabilityId,
                        s.AvailableDate,
                        StartTime = s.StartTime.ToString("HH:mm"),
                        EndTime = s.EndTime.ToString("HH:mm"),
                        Mode = s.AvailabilityMode?.AvailabilityMode1 ?? string.Empty
                    })
                    .ToList();

                return Ok(OperationResult<object>.Exito(available,
                    available.Count == 0
                        ? "El médico no tiene disponibilidad en esa fecha."
                        : $"{available.Count} horario(s) disponible(s)."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener disponibilidad del doctor {Id}", doctorId);
                return StatusCode(500, OperationResult.Fallo("Error al obtener la disponibilidad."));
            }
        }

        // ── GET: api/appointments/check-slot ─────────────────────────────────
        // Task 132: verificación ultramicro — el wizard llama este endpoint
        // justo antes de mostrar el botón "Confirmar" para validar en tiempo real
        [HttpGet("check-slot")]
        public async Task<ActionResult> CheckSlot(
            [FromQuery] int doctorId,
            [FromQuery] DateTime appointmentDate)
        {
            if (doctorId <= 0)
                return BadRequest(OperationResult.Fallo("El ID del doctor es inválido."));

            try
            {
                var date = DateOnly.FromDateTime(appointmentDate);
                var time = TimeOnly.FromDateTime(appointmentDate);

                var isAvailable = await _availabilityRepository.IsAvailableAsync(doctorId, date, time);
                var hasConflict = await _appointmentService.GetByDoctorIdAsync(doctorId);

                // Verificar si el slot exacto ya fue tomado
                var slotTaken = hasConflict.Datos?
                    .Any(a => a.AppointmentDate == appointmentDate &&
                              a.StatusId != 3) // excluir canceladas
                    ?? false;

                if (!isAvailable || slotTaken)
                    return Ok(new
                    {
                        disponible = false,
                        mensaje = "Este horario ya no está disponible. Por favor selecciona otro."
                    });

                return Ok(new
                {
                    disponible = true,
                    mensaje = "Horario disponible."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar slot para doctor {Id}", doctorId);
                return StatusCode(500, OperationResult.Fallo("Error al verificar disponibilidad."));
            }
        }

        // ── PUT: api/appointments/5 ───────────────────────────────────────────
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OperationResult<AppointmentDto>>> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos."));

            if (id != dto.Id)
                return BadRequest(OperationResult.Fallo("El ID de la ruta no coincide con el del cuerpo."));

            try
            {
                var result = await _appointmentService.UpdateAsync(dto);
                if (!result.Exitoso)
                    return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al actualizar la cita."));
            }
        }

        // ── PUT: api/appointments/5/reschedule ────────────────────────────────
        // Task 140: endpoint transaccional — fuerza el retorno a Pendiente
        [HttpPut("{id:int}/reschedule")]
        public async Task<ActionResult<OperationResult>> Reschedule(int id, [FromBody] RescheduleAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos."));

            if (id != dto.AppointmentId)
                return BadRequest(OperationResult.Fallo("El ID de la ruta no coincide con el del cuerpo."));

            try
            {
                var result = await _appointmentService.RescheduleAsync(id, dto.NewAppointmentDate);
                if (!result.Exitoso)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reprogramar cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al reprogramar la cita."));
            }
        }

        // ── DELETE: api/appointments/5 ────────────────────────────────────────
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
                _logger.LogError(ex, "Error al eliminar cita {Id}", id);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al eliminar la cita."));
            }
        }

        // ── GET: api/appointments/patient/1 ──────────────────────────────────
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
                _logger.LogError(ex, "Error al obtener citas del paciente {Id}", patientId);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener citas del paciente."));
            }
        }

        // ── GET: api/appointments/doctor/1 ────────────────────────────────────
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
                _logger.LogError(ex, "Error al obtener citas del doctor {Id}", doctorId);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener citas del doctor."));
            }
        }

        // ── GET: api/appointments/me ──────────────────────────────────────────
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
                if (!result.Exitoso || result.Datos is null)
                    return Ok(result);

                var appointments = result.Datos.AsEnumerable();

                if (statusId.HasValue) appointments = appointments.Where(a => a.StatusId == statusId.Value);
                if (from.HasValue) appointments = appointments.Where(a => a.AppointmentDate >= from.Value);
                if (to.HasValue) appointments = appointments.Where(a => a.AppointmentDate <= to.Value);

                var list = appointments.OrderByDescending(a => a.AppointmentDate).ToList();

                return Ok(OperationResult<List<AppointmentDto>>.Exito(list,
                    list.Count == 0 ? "No tienes citas registradas." : $"{list.Count} cita(s) encontrada(s)."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener historial del paciente {Id}", patientId);
                return StatusCode(500, OperationResult.Fallo("Error inesperado al obtener tu historial."));
            }
        }
    }
}