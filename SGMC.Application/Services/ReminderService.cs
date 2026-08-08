using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.System;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.System;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    // — Recordatorios personalizados del médico
    public class ReminderService : IReminderService
    {
        private static readonly List<ReminderTemplateDto> Templates = new()
        {
            new ReminderTemplateDto { TemplateId = 1, Name = "Recordatorio estándar",
                MessageTemplate = "Hola {PatientName}, te recordamos tu cita programada para el {AppointmentDate}. ¡Te esperamos!" },
            new ReminderTemplateDto { TemplateId = 2, Name = "Recordatorio de seguimiento",
                MessageTemplate = "Hola {PatientName}, es momento de tu cita de seguimiento el {AppointmentDate}. Por favor confirma tu asistencia." },
            new ReminderTemplateDto { TemplateId = 3, Name = "Indicaciones previas (ayuno)",
                MessageTemplate = "Hola {PatientName}, recuerda que tu cita es el {AppointmentDate} y debes asistir en ayunas." },
        };

        private readonly IReminderRepository _reminderRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(
            IReminderRepository reminderRepository,
            IAppointmentRepository appointmentRepository,
            IUserRepository userRepository,
            ILogger<ReminderService> logger)
        {
            _reminderRepository = reminderRepository;
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public List<ReminderTemplateDto> GetTemplates() => Templates;

        public async Task<OperationResult<ReminderDto>> ScheduleAsync(ScheduleReminderDto dto, int doctorId)
        {
            var validation = dto.IsValidDto();
            if (!validation.Exitoso)
                return OperationResult<ReminderDto>.Fallo(validation.Mensaje);

            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(dto.AppointmentId);
            if (appointment is null)
                return OperationResult<ReminderDto>.Fallo("La cita no existe.");

            if (appointment.DoctorId != doctorId)
                return OperationResult<ReminderDto>.Fallo("No tienes permiso para programar recordatorios de esta cita.");

            if (appointment.StatusId != 1 && appointment.StatusId != 2)
                return OperationResult<ReminderDto>.Fallo("Solo se pueden programar recordatorios para citas Pendientes o Confirmadas.");

            if (dto.ScheduledAt >= appointment.AppointmentDate)
                return OperationResult<ReminderDto>.Fallo("La fecha del recordatorio debe ser anterior a la fecha de la cita.");

            var patientPerson = appointment.Patient?.PatientNavigation;
            if (patientPerson is null)
                return OperationResult<ReminderDto>.Fallo("No se encontraron los datos del paciente de esta cita.");

            var patientUser = await _userRepository.GetByIdAsync(patientPerson.UserId);
            if (patientUser is null || string.IsNullOrWhiteSpace(patientUser.Email))
                return OperationResult<ReminderDto>.Fallo("El paciente no tiene un correo electrónico registrado.");

            var patientName = $"{patientPerson.FirstName} {patientPerson.LastName}";
            var message = ResolveMessage(dto, patientName, appointment.AppointmentDate);

            var reminder = new Reminder
            {
                AppointmentId = appointment.AppointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                PatientEmail = patientUser.Email,
                Message = message,
                ScheduledAt = dto.ScheduledAt,
                Status = "Pendiente",
                CreatedAt = DateTime.Now
            };

            var created = await _reminderRepository.AddAsync(reminder);

            _logger.LogInformation(
                "Recordatorio {ReminderId} programado para la cita {AppointmentId} el {ScheduledAt}.",
                created.ReminderId, appointment.AppointmentId, dto.ScheduledAt);

            return OperationResult<ReminderDto>.Exito(MapToDto(created), "Recordatorio programado correctamente.");
        }

        public async Task<OperationResult> CancelAsync(int reminderId, int doctorId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(reminderId);
            if (reminder is null)
                return OperationResult.Fallo("El recordatorio no existe.");

            if (reminder.DoctorId != doctorId)
                return OperationResult.Fallo("No tienes permiso para cancelar este recordatorio.");

            if (reminder.Status != "Pendiente")
                return OperationResult.Fallo("Solo se pueden cancelar recordatorios pendientes.");

            reminder.Status = "Cancelado";
            reminder.CancelledAt = DateTime.Now;
            await _reminderRepository.UpdateAsync(reminder);

            return OperationResult.Exito("Recordatorio cancelado correctamente.");
        }

        public async Task<OperationResult<List<ReminderDto>>> GetByAppointmentIdAsync(int appointmentId)
        {
            var reminders = await _reminderRepository.GetByAppointmentIdAsync(appointmentId);
            return OperationResult<List<ReminderDto>>.Exito(reminders.Select(MapToDto).ToList());
        }

        public async Task CancelPendingRemindersForAppointmentAsync(int appointmentId)
        {
            var pending = await _reminderRepository.GetPendingByAppointmentIdAsync(appointmentId);
            foreach (var reminder in pending)
            {
                reminder.Status = "Cancelado";
                reminder.CancelledAt = DateTime.Now;
                await _reminderRepository.UpdateAsync(reminder);
            }

            if (pending.Count > 0)
            {
                _logger.LogInformation(
                    "{Count} recordatorio(s) pendiente(s) cancelados automáticamente para la cita {AppointmentId} (cita cancelada/rechazada).",
                    pending.Count, appointmentId);
            }
        }

        private static string ResolveMessage(ScheduleReminderDto dto, string patientName, DateTime appointmentDate)
        {
            if (dto.TemplateId.HasValue)
            {
                var template = Templates.FirstOrDefault(t => t.TemplateId == dto.TemplateId.Value);
                if (template is not null)
                {
                    return template.MessageTemplate
                        .Replace("{PatientName}", patientName)
                        .Replace("{AppointmentDate}", appointmentDate.ToString("dd/MM/yyyy hh:mm tt"));
                }
            }

            return dto.CustomMessage!.Trim();
        }

        private static ReminderDto MapToDto(Reminder reminder) => new()
        {
            ReminderId = reminder.ReminderId,
            AppointmentId = reminder.AppointmentId,
            PatientName = reminder.PatientName,
            Message = reminder.Message,
            ScheduledAt = reminder.ScheduledAt,
            Status = reminder.Status,
            CreatedAt = reminder.CreatedAt,
            SentAt = reminder.SentAt,
            CancelledAt = reminder.CancelledAt
        };
    }
}
