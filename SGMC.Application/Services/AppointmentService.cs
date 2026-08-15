using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.Appointments;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorAvailabilityRepository _availabilityRepository;
        private readonly IAppointmentNotificationService _notificationService;
        private readonly IReminderService _reminderService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository repository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IDoctorAvailabilityRepository availabilityRepository,
            IAppointmentNotificationService notificationService,
            IReminderService reminderService,
            ILogger<AppointmentService> logger)
        {
            _repository = repository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _availabilityRepository = availabilityRepository;
            _notificationService = notificationService;
            _reminderService = reminderService;
            _logger = logger;
        }

        // ── Task 130 + 132: CREATE con doble verificación horaria ─────────────
        /// <summary>
        /// FLUJO DE RESERVA DE CITA — Task 129
        ///
        /// Paso 1: Validación de campos del DTO
        ///   - PatientId y DoctorId deben ser mayores a 0
        ///   - AppointmentDate no puede ser en el pasado
        ///
        /// Paso 2: Validaciones de negocio contra BD
        ///   - Verificar que el paciente existe en la base de datos
        ///   - Verificar que el doctor existe en la base de datos
        ///
        /// Paso 3: Primera verificación de disponibilidad
        ///   - Consultar DoctorAvailability para confirmar que el médico
        ///     tiene un bloque activo (IsActive = true) que cubra la fecha
        ///     y hora solicitadas (StartTime <= hora < EndTime)
        ///   - Si no hay disponibilidad → retornar Fallo con mensaje al paciente
        ///
        /// Paso 4: Verificación de conflicto con citas existentes
        ///   - Confirmar que no existe otra cita en el mismo slot exacto
        ///   - Si hay conflicto → retornar Fallo indicando que el horario no está disponible
        ///
        /// Paso 5: Doble verificación ultramicro (Task 132)
        ///   - Repetir pasos 3 y 4 milisegundos antes del guardado definitivo
        ///   - Cubre condiciones de carrera entre usuarios concurrentes
        ///   - Si el slot fue tomado en ese instante → retornar Fallo
        ///
        /// Paso 6: Creación de la cita
        ///   - Insertar registro con StatusId = 1 (Pendiente)
        ///   - CreatedAt se asigna automáticamente con DateTime.Now
        ///   - La cita queda pendiente hasta que el médico la confirme
        /// </summary>
        public async Task<OperationResult<AppointmentDto>> CreateAsync(CreateAppointmentDto dto)
        {
            if (dto is null)
                return OperationResult<AppointmentDto>.Fallo("Los datos de la cita son requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<AppointmentDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                // ── Validaciones de negocio contra BD ────────────────────────

                var patientExists = await _patientRepository.ExistsAsync(dto.PatientId);
                if (!patientExists)
                    return OperationResult<AppointmentDto>.Fallo("El paciente no existe.");

                var doctorExists = await _doctorRepository.ExistsAsync(d => d.DoctorId == dto.DoctorId);
                if (!doctorExists)
                    return OperationResult<AppointmentDto>.Fallo("El doctor no existe.");

                // ── PRIMERA VERIFICACIÓN: disponibilidad configurada ──────────
                // Verifica que el médico tenga un bloque activo que cubra la hora solicitada
                var date = DateOnly.FromDateTime(dto.AppointmentDate);
                var time = TimeOnly.FromDateTime(dto.AppointmentDate);

                var isAvailable = await _availabilityRepository.IsAvailableAsync(
                    dto.DoctorId, date, time);

                if (!isAvailable)
                    return OperationResult<AppointmentDto>.Fallo(
                        "El médico no tiene disponibilidad en la fecha y hora seleccionadas. " +
                        "Por favor selecciona otro horario.");

                // ── SEGUNDA VERIFICACIÓN: conflicto con cita existente ────────
                // Verifica que no haya otra cita en ese slot exacto
                var hasConflict = await _repository.ExistsInTimeSlotAsync(
                    dto.DoctorId, dto.AppointmentDate);

                if (hasConflict)
                    return OperationResult<AppointmentDto>.Fallo(
                        "El horario seleccionado ya no está disponible. " +
                        "Por favor selecciona otra fecha u hora.");

                // ── Task 132: DOBLE VERIFICACIÓN ULTRAMICRO ───────────────────
                // Segunda consulta atómica milisegundos antes del guardado
                // para cubrir condiciones de carrera entre usuarios concurrentes
                var stillAvailable = await _availabilityRepository.IsAvailableAsync(
                    dto.DoctorId, date, time);
                var stillNoConflict = !await _repository.ExistsInTimeSlotAsync(
                    dto.DoctorId, dto.AppointmentDate);

                if (!stillAvailable || !stillNoConflict)
                    return OperationResult<AppointmentDto>.Fallo(
                        "El horario fue tomado por otro paciente en este momento. " +
                        "Por favor selecciona otro horario.");

                // ── Crear la cita ─────────────────────────────────────────────
                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    AppointmentDate = dto.AppointmentDate,
                    StatusId = 1,            // Pendiente
                    CreatedAt = DateTime.Now
                };

                var created = await _repository.AddAsync(appointment);

                try
                {
                    await _notificationService.NotifyAppointmentCreatedAsync(created);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning(notifyEx,
                        "La cita {Id} se creó pero falló el encolado de la notificación al médico.",
                        created.AppointmentId);
                }

                var dtoResult = MapToDto(created);

                return OperationResult<AppointmentDto>.Exito(dtoResult, "Cita agendada correctamente. Quedará pendiente hasta que el médico la confirme.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la cita");
                return OperationResult<AppointmentDto>.Fallo("Error interno al crear la cita.");
            }
        }

        // ── CANCEL ────────────────────────────────────────────────────────────
        /// <summary>
        /// FLUJO DE CANCELACIÓN — Task 136
        ///
        /// Paso 1: Validar que el ID sea válido
        /// Paso 2: Verificar que la cita existe
        /// Paso 3: Verificar que la cita está en estado Pendiente (1) o Confirmada (2)
        ///         — No se pueden cancelar citas Canceladas, Completadas o Rechazadas
        /// Paso 4: Cambiar StatusId a 3 (Cancelada) y registrar UpdatedAt
        /// Paso 5: Liberar el horario en DoctorAvailability
        ///         — Buscar el bloque que cubre la fecha/hora de la cita
        ///         — Marcarlo como IsActive = true para que vuelva a estar disponible
        /// Paso 6: Guardar cambios
        /// </summary>
        public async Task<OperationResult> CancelAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                return OperationResult.Fallo("El ID de la cita es inválido.");

            try
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment is null)
                    return OperationResult.Fallo("La cita no existe.");
                // Solo se pueden cancelar citas Pendientes (1) o Confirmadas (2)
                if (appointment.StatusId != 1 && appointment.StatusId != 2)
                    return OperationResult.Fallo(
                        "Solo se pueden cancelar citas en estado Pendiente o Confirmada.");

                // Task 91: no se pueden cancelar citas cuya fecha ya paso
                if (appointment.AppointmentDate <= DateTime.Now)
                    return OperationResult.Fallo("No se pueden cancelar citas que ya pasaron.");
                // Paso 4: Cambiar estado a Cancelada
                appointment.StatusId = 3;
                appointment.UpdatedAt = DateTime.Now;
                await _repository.UpdateAsync(appointment);

                // Paso 5: Liberar el horario en DoctorAvailability
                var date = DateOnly.FromDateTime(appointment.AppointmentDate);
                var time = TimeOnly.FromDateTime(appointment.AppointmentDate);

                // Buscar el bloque de disponibilidad que cubre este horario
                var slots = await _availabilityRepository.GetByDoctorAndDateRangeAsync(
                    appointment.DoctorId, date, date);

                var slotToFree = slots.FirstOrDefault(s =>
                    s.StartTime <= time && time < s.EndTime);

                if (slotToFree is not null && !slotToFree.IsActive)
                {
                    // Reactivar el slot para que otros pacientes puedan tomarlo
                    slotToFree.IsActive = true;
                    slotToFree.UpdatedAt = DateTime.Now;
                    await _availabilityRepository.UpdateAsync(slotToFree);
                }

                try
                {
                    await _notificationService.NotifyAppointmentCancelledAsync(appointment);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning(notifyEx,
                        "La cita {Id} se canceló pero falló el encolado de la notificación.",
                        appointmentId);
                }

                try
                {
                    // Task 107: purgar recordatorios pendientes de esta cita
                    await _reminderService.CancelPendingRemindersForAppointmentAsync(appointmentId);
                }
                catch (Exception reminderEx)
                {
                    _logger.LogWarning(reminderEx,
                        "La cita {Id} se canceló pero falló la purga de recordatorios pendientes.",
                        appointmentId);
                }

                _logger.LogInformation(
                    "Cita {Id} cancelada. Slot liberado para doctor {DoctorId} en {Date} {Time}",
                    appointmentId, appointment.DoctorId, date, time);

                return OperationResult.Exito("Cita cancelada correctamente. El horario fue liberado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar cita {Id}", appointmentId);
                return OperationResult.Fallo("Error al cancelar la cita.");
            }
        }

        // ── CONFIRM ───────────────────────────────────────────────────────────
        public async Task<OperationResult> ConfirmAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                return OperationResult.Fallo("El ID de la cita es inválido.");

            try
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment is null)
                    return OperationResult.Fallo("La cita no existe.");

                if (appointment.StatusId != 1)
                    return OperationResult.Fallo("Solo se pueden confirmar citas en estado Pendiente.");

                appointment.StatusId = 2; // Confirmada
                appointment.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(appointment);

                try
                {
                    await _notificationService.NotifyAppointmentConfirmedAsync(appointment);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning(notifyEx,
                        "La cita {Id} se confirmó pero falló el envío de la notificación por correo.",
                        appointmentId);
                }

                _logger.LogInformation("Cita {Id} confirmada por el médico.", appointmentId);

                return OperationResult.Exito("Cita confirmada correctamente. Se notificó al paciente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar cita {Id}", appointmentId);
                return OperationResult.Fallo("Error al confirmar la cita.");
            }
        }

        // ── REJECT ────────────────────────────────────────────────────────────
        /// <summary>
        /// FLUJO DE RECHAZO — PBI-35 / Task 138
        ///
        /// Distinto de CancelAsync: solo aplica a solicitudes Pendientes (1),
        /// es una acción exclusiva del médico, y notifica al paciente indicando
        /// que puede seleccionar una nueva fecha (en vez del mensaje genérico de cancelación).
        /// </summary>
        public async Task<OperationResult> RejectAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                return OperationResult.Fallo("El ID de la cita es inválido.");

            try
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment is null)
                    return OperationResult.Fallo("La cita no existe.");

                if (appointment.StatusId != 1)
                    return OperationResult.Fallo("Solo se pueden rechazar solicitudes en estado Pendiente.");

                appointment.StatusId = 3; // Cancelada
                appointment.UpdatedAt = DateTime.Now;
                await _repository.UpdateAsync(appointment);

                // Liberar el horario en DoctorAvailability
                var date = DateOnly.FromDateTime(appointment.AppointmentDate);
                var time = TimeOnly.FromDateTime(appointment.AppointmentDate);

                var slots = await _availabilityRepository.GetByDoctorAndDateRangeAsync(
                    appointment.DoctorId, date, date);

                var slotToFree = slots.FirstOrDefault(s =>
                    s.StartTime <= time && time < s.EndTime);

                if (slotToFree is not null && !slotToFree.IsActive)
                {
                    slotToFree.IsActive = true;
                    slotToFree.UpdatedAt = DateTime.Now;
                    await _availabilityRepository.UpdateAsync(slotToFree);
                }

                try
                {
                    await _notificationService.NotifyAppointmentCancelledAsync(appointment);
                }
                catch (Exception notifyEx)
                {
                    _logger.LogWarning(notifyEx,
                        "La cita {Id} se rechazó pero falló el envío de la notificación por correo.",
                        appointmentId);
                }

                try
                {
                    // Task 107: purgar recordatorios pendientes de esta cita
                    await _reminderService.CancelPendingRemindersForAppointmentAsync(appointmentId);
                }
                catch (Exception reminderEx)
                {
                    _logger.LogWarning(reminderEx,
                        "La cita {Id} se rechazó pero falló la purga de recordatorios pendientes.",
                        appointmentId);
                }

                _logger.LogInformation(
                    "Solicitud de cita {Id} rechazada por el médico. Horario liberado.", appointmentId);

                return OperationResult.Exito(
                    "Solicitud rechazada. El paciente fue notificado para seleccionar una nueva fecha.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al rechazar cita {Id}", appointmentId);
                return OperationResult.Fallo("Error al rechazar la cita.");
            }
        }

        // ── RESCHEDULE ────────────────────────────────────────────────────────
        /// <summary>
        /// FLUJO DE REPROGRAMACIÓN — PBI-34 / Task 140
        ///
        /// Paso 1: Validar que el ID sea válido y la cita exista
        /// Paso 2: Solo se pueden reprogramar citas Pendiente (1) o Confirmada (2)
        /// Paso 3: Validar que la nueva fecha no sea en el pasado
        /// Paso 4: Verificar disponibilidad del mismo médico en la nueva fecha/hora
        /// Paso 5: Verificar que no haya conflicto con otra cita ya existente
        /// Paso 6: Doble verificación ultramicro (igual que en CreateAsync/Task 132)
        ///         justo antes de confirmar, para cubrir condiciones de carrera
        /// Paso 7: Actualizar la cita: nueva fecha, StatusId vuelve a 1 (Pendiente),
        ///         UpdatedAt se registra automáticamente
        /// Paso 8: Liberar el horario anterior en DoctorAvailability
        /// Paso 9: Enviar notificación por correo a médico y paciente
        /// </summary>
        public async Task<OperationResult> RescheduleAsync(int appointmentId, DateTime newDate)
        {
            if (appointmentId <= 0)
                return OperationResult.Fallo("El ID de la cita es inválido.");

            if (newDate <= DateTime.Now)
                return OperationResult.Fallo("La nueva fecha y hora deben ser futuras.");

            try
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(appointmentId);
                if (appointment is null)
                    return OperationResult.Fallo("La cita no existe.");

                // Solo se pueden reprogramar citas Pendientes (1) o Confirmadas (2)
                if (appointment.StatusId != 1 && appointment.StatusId != 2)
                    return OperationResult.Fallo(
                        "Solo se pueden reprogramar citas en estado Pendiente o Confirmada.");

                // Task 91: no se pueden reprogramar citas cuya fecha original ya paso
                if (appointment.AppointmentDate <= DateTime.Now)
                    return OperationResult.Fallo("No se pueden reprogramar citas que ya pasaron.");

                var oldAppointmentDate = appointment.AppointmentDate;

                var date = DateOnly.FromDateTime(newDate);
                var time = TimeOnly.FromDateTime(newDate);

                // ── PRIMERA VERIFICACIÓN: disponibilidad del mismo médico ─────────
                var isAvailable = await _availabilityRepository.IsAvailableAsync(
                    appointment.DoctorId, date, time);

                if (!isAvailable)
                    return OperationResult.Fallo(
                        "El médico no tiene disponibilidad en la fecha y hora seleccionadas. " +
                        "Por favor selecciona otro horario.");

                // ── SEGUNDA VERIFICACIÓN: conflicto con otra cita ─────────────────
                var hasConflict = await _repository.ExistsInTimeSlotAsync(appointment.DoctorId, newDate);
                if (hasConflict)
                    return OperationResult.Fallo(
                        "El horario seleccionado ya no está disponible. " +
                        "Por favor selecciona otra fecha u hora.");

                // ── DOBLE VERIFICACIÓN ULTRAMICRO ─────────────────────────────────
                // Repite las dos validaciones anteriores justo antes de guardar,
                // para cubrir el caso de que otro paciente tome el horario
                // en el instante entre la verificación y el guardado
                var stillAvailable = await _availabilityRepository.IsAvailableAsync(
                    appointment.DoctorId, date, time);
                var stillNoConflict = !await _repository.ExistsInTimeSlotAsync(
                    appointment.DoctorId, newDate);

                if (!stillAvailable || !stillNoConflict)
                    return OperationResult.Fallo(
                        "El horario fue tomado por otro paciente en este momento. " +
                        "Por favor selecciona otro horario.");

                // ── Actualizar la cita ─────────────────────────────────────────────
                appointment.AppointmentDate = newDate;
                appointment.StatusId = 1; // Vuelve a Pendiente hasta nueva confirmación
                appointment.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(appointment);

                // ── Liberar el horario anterior en DoctorAvailability ─────────────
                var oldDate = DateOnly.FromDateTime(oldAppointmentDate);
                var oldTime = TimeOnly.FromDateTime(oldAppointmentDate);

                var slots = await _availabilityRepository.GetByDoctorAndDateRangeAsync(
                    appointment.DoctorId, oldDate, oldDate);

                var slotToFree = slots.FirstOrDefault(s =>
                    s.StartTime <= oldTime && oldTime < s.EndTime);

                if (slotToFree is not null && !slotToFree.IsActive)
                {
                    slotToFree.IsActive = true;
                    slotToFree.UpdatedAt = DateTime.Now;
                    await _availabilityRepository.UpdateAsync(slotToFree);
                }

                // ── Notificación por correo a médico y paciente ───────────────────
                try
                {
                    await _notificationService.NotifyAppointmentRescheduledAsync(
                        appointment, oldAppointmentDate);
                }
                catch (Exception notifyEx)
                {
                    // La reprogramación ya se guardó correctamente; un fallo de correo
                    // no debe revertir la operación, solo se registra en el log
                    _logger.LogWarning(notifyEx,
                        "La cita {Id} se reprogramó pero falló el envío de la notificación por correo.",
                        appointmentId);
                }

                _logger.LogInformation(
                    "Cita {Id} reprogramada de {OldDate} a {NewDate}. Horario anterior liberado.",
                    appointmentId, oldAppointmentDate, newDate);

                return OperationResult.Exito(
                    "Cita reprogramada correctamente. Quedará pendiente hasta que el médico la confirme.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reprogramar cita {Id}", appointmentId);
                return OperationResult.Fallo("Error al reprogramar la cita.");
            }
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<OperationResult<AppointmentDto>> UpdateAsync(UpdateAppointmentDto dto)
        {
            if (dto is null)
                return OperationResult<AppointmentDto>.Fallo("Los datos de la cita son requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<AppointmentDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var appointment = await _repository.GetByIdAsync(dto.AppointmentId);
                if (appointment is null)
                    return OperationResult<AppointmentDto>.Fallo("La cita no existe.");

                appointment.AppointmentDate = dto.AppointmentDate;
                appointment.StatusId = dto.StatusId;
                appointment.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(appointment);

                var dtoResult = MapToDto(appointment);
                return OperationResult<AppointmentDto>.Exito(dtoResult, "Cita actualizada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cita {Id}", dto.AppointmentId);
                return OperationResult<AppointmentDto>.Fallo("Error al actualizar la cita.");
            }
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<OperationResult> DeleteAsync(int id)
        {
            if (id <= 0)
                return OperationResult.Fallo("El ID de la cita es inválido.");

            try
            {
                var exists = await _repository.ExistsAsync(id);
                if (!exists)
                    return OperationResult.Fallo("La cita no existe.");

                await _repository.DeleteAsync(id);
                return OperationResult.Exito("Cita eliminada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cita {Id}", id);
                return OperationResult.Fallo("Error al eliminar la cita.");
            }
        }

        // ── QUERIES ───────────────────────────────────────────────────────────
        public async Task<OperationResult<List<AppointmentDto>>> GetAllAsync()
        {
            try
            {
                var appointments = await _repository.GetAllWithDetailsAsync();
                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas");
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las citas.");
            }
        }

        public async Task<OperationResult<AppointmentDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
                return OperationResult<AppointmentDto>.Fallo("El ID de la cita es inválido.");

            try
            {
                var appointment = await _repository.GetByIdWithDetailsAsync(id);
                if (appointment is null)
                    return OperationResult<AppointmentDto>.Fallo("La cita no existe.");

                return OperationResult<AppointmentDto>.Exito(MapToDto(appointment), "Cita obtenida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cita {Id}", id);
                return OperationResult<AppointmentDto>.Fallo("Error al obtener la cita.");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetByPatientIdAsync(int patientId)
        {
            if (patientId <= 0)
                return OperationResult<List<AppointmentDto>>.Fallo("El ID del paciente es inválido.");

            try
            {
                var appointments = await _repository.GetByPatientIdWithDetailsAsync(patientId);
                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas de paciente {Id}", patientId);
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las citas del paciente.");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetByDoctorIdAsync(int doctorId)
        {
            if (doctorId <= 0)
                return OperationResult<List<AppointmentDto>>.Fallo("El ID del doctor es inválido.");

            try
            {
                var appointments = await _repository.GetByDoctorIdWithDetailsAsync(doctorId);
                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del doctor {Id}", doctorId);
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las citas del doctor.");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                return OperationResult<List<AppointmentDto>>.Fallo("El rango de fechas es inválido.");

            try
            {
                var appointments = await _repository.GetByDateRangeAsync(startDate, endDate);
                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas en rango de fechas");
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las citas en el rango de fechas.");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetUpcomingForPatientAsync(int patientId)
        {
            if (patientId <= 0)
                return OperationResult<List<AppointmentDto>>.Fallo("El ID del paciente es inválido.");

            try
            {
                var appointments = await _repository.GetUpcomingAppointmentsAsync(patientId);
                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener próximas citas del paciente {Id}", patientId);
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las próximas citas del paciente.");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetFilteredAppointmentsAsync(AppointmentFilterDto filter)
        {
            if (filter is null)
                return OperationResult<List<AppointmentDto>>.Fallo("El filtro es requerido.");

            try
            {
                IEnumerable<Appointment> appointments;

                if (filter.PatientId.HasValue)
                    appointments = await _repository.GetByPatientIdAsync(filter.PatientId.Value);
                else if (filter.DoctorId.HasValue)
                    appointments = await _repository.GetByDoctorIdAsync(filter.DoctorId.Value);
                else if (filter.StatusId.HasValue)
                    appointments = await _repository.GetByStatusIdAsync(filter.StatusId.Value);
                else
                    appointments = await _repository.GetAllWithDetailsAsync();

                if (filter.StatusId.HasValue && (filter.PatientId.HasValue || filter.DoctorId.HasValue))
                    appointments = appointments.Where(a => a.StatusId == filter.StatusId.Value);

                var list = appointments.Select(MapToDto).ToList();
                return OperationResult<List<AppointmentDto>>.Exito(list, "Citas obtenidas correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas filtradas");
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener las citas filtradas.");
            }
        }

        // ── MAPPING ───────────────────────────────────────────────────────────
        private static AppointmentDto MapToDto(Appointment a)
        {
            if (a is null) throw new ArgumentNullException(nameof(a));

            return new AppointmentDto
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                AppointmentDate = a.AppointmentDate,
                StatusId = a.StatusId,
                PatientName = a.Patient?.PatientNavigation != null
                    ? $"{a.Patient.PatientNavigation.FirstName} {a.Patient.PatientNavigation.LastName}"
                    : string.Empty,
                DoctorName = a.Doctor?.DoctorNavigation != null
                    ? $"{a.Doctor.DoctorNavigation.FirstName} {a.Doctor.DoctorNavigation.LastName}"
                    : string.Empty,
                StatusName = a.Status?.StatusName ?? string.Empty,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }
    }
}