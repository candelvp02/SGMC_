using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    // Notificaciones de citas ------------------------
    public class AppointmentNotificationService : IAppointmentNotificationService
    {
        private readonly ILogger<AppointmentNotificationService> _logger;
        private readonly IAppointmentNotificationEventQueue _queue;
        private readonly IUserRepository _userRepository;

        public AppointmentNotificationService(
            ILogger<AppointmentNotificationService> logger,
            IAppointmentNotificationEventQueue queue,
            IUserRepository userRepository)
        {
            _logger = logger;
            _queue = queue;
            _userRepository = userRepository;
        }

        // — solo notifica al médico
        public Task NotifyAppointmentCreatedAsync(Appointment appointment)
            => EnqueueForDoctorAsync(appointment, AppointmentNotificationEventType.NuevaCita);

        // — notifica a ambos
        public async Task NotifyAppointmentConfirmedAsync(Appointment appointment)
        {
            await EnqueueForPatientAsync(appointment, AppointmentNotificationEventType.CitaConfirmada);
            await EnqueueForDoctorAsync(appointment, AppointmentNotificationEventType.CitaConfirmada);
        }

        public async Task NotifyAppointmentCancelledAsync(Appointment appointment)
        {
            await EnqueueForPatientAsync(appointment, AppointmentNotificationEventType.CitaCancelada);
            await EnqueueForDoctorAsync(appointment, AppointmentNotificationEventType.CitaCancelada);
        }

        public async Task NotifyAppointmentRescheduledAsync(Appointment appointment, DateTime oldAppointmentDate)
        {
            await EnqueueForPatientAsync(appointment, AppointmentNotificationEventType.CitaReprogramada, oldAppointmentDate);
            await EnqueueForDoctorAsync(appointment, AppointmentNotificationEventType.CitaReprogramada, oldAppointmentDate);
        }

        // ── Helpers ────────────────────────────────────────────────────────

        private Task EnqueueForDoctorAsync(Appointment appointment, AppointmentNotificationEventType eventType, DateTime? oldDate = null)
        {
            var doctorPerson = appointment.Doctor?.DoctorNavigation;
            var doctorDisplayName = doctorPerson != null
                ? $"Dr(a). {doctorPerson.FirstName} {doctorPerson.LastName}"
                : $"Médico #{appointment.DoctorId}";

            return EnqueueAsync(
                personUserId: doctorPerson?.UserId,
                recipientType: NotificationRecipientType.Doctor,
                recipientDisplayName: doctorDisplayName,
                counterpartName: GetPatientName(appointment),
                appointment: appointment,
                eventType: eventType,
                oldDate: oldDate);
        }

        private Task EnqueueForPatientAsync(Appointment appointment, AppointmentNotificationEventType eventType, DateTime? oldDate = null)
        {
            var patientPerson = appointment.Patient?.PatientNavigation;
            var patientDisplayName = patientPerson != null
                ? $"{patientPerson.FirstName} {patientPerson.LastName}"
                : $"Paciente #{appointment.PatientId}";

            var doctorPerson = appointment.Doctor?.DoctorNavigation;
            var doctorName = doctorPerson != null
                ? $"Dr(a). {doctorPerson.FirstName} {doctorPerson.LastName}"
                : $"Médico #{appointment.DoctorId}";

            return EnqueueAsync(
                personUserId: patientPerson?.UserId,
                recipientType: NotificationRecipientType.Patient,
                recipientDisplayName: patientDisplayName,
                counterpartName: doctorName,
                appointment: appointment,
                eventType: eventType,
                oldDate: oldDate);
        }

        private async Task EnqueueAsync(
            int? personUserId,
            NotificationRecipientType recipientType,
            string recipientDisplayName,
            string counterpartName,
            Appointment appointment,
            AppointmentNotificationEventType eventType,
            DateTime? oldDate)
        {
            if (personUserId is null)
            {
                _logger.LogWarning(
                    "No se pudo notificar a {RecipientType} de la cita {Id}: faltan datos de la persona en el objeto Appointment.",
                    recipientType, appointment.AppointmentId);
                return;
            }

            var user = await _userRepository.GetByIdAsync(personUserId.Value);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning(
                    "No se pudo notificar a {RecipientType} (UserId {UserId}) de la cita {Id}: no tiene un email registrado.",
                    recipientType, personUserId, appointment.AppointmentId);
                return;
            }

            var dto = new AppointmentNotificationEventDto
            {
                EventType = eventType,
                RecipientType = recipientType,
                AppointmentId = appointment.AppointmentId,
                RecipientUserId = user.UserId,
                RecipientEmail = user.Email,
                RecipientName = recipientDisplayName,
                CounterpartName = counterpartName,
                AppointmentDate = appointment.AppointmentDate,
                PreviousAppointmentDate = oldDate,
                QueuedAt = DateTime.Now
            };

            _queue.Enqueue(dto);

            _logger.LogInformation(
                "Evento {EventType} encolado para notificar por correo a {RecipientType} {RecipientName} (cita {AppointmentId}).",
                eventType, recipientType, recipientDisplayName, appointment.AppointmentId);
        }

        private static string GetPatientName(Appointment appointment)
        {
            return appointment.Patient?.PatientNavigation != null
                ? $"{appointment.Patient.PatientNavigation.FirstName} {appointment.Patient.PatientNavigation.LastName}"
                : $"Paciente #{appointment.PatientId}";
        }
    }
}