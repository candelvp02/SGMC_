using Microsoft.Extensions.Logging;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Entities.Appointments;

namespace SGMC.Application.Services
{
    //Notificación de reprogramación de citas — PBI-34.
    public class AppointmentNotificationService : IAppointmentNotificationService
    {
        private readonly ILogger<AppointmentNotificationService> _logger;

        public AppointmentNotificationService(ILogger<AppointmentNotificationService> logger)
        {
            _logger = logger;
        }

        public Task NotifyAppointmentRescheduledAsync(Appointment appointment, DateTime oldAppointmentDate)
        {
            _logger.LogInformation(
                "SIMULACIÓN: Notificación de reprogramación — cita {Id}: {Old} → {New}. " +
                "Se notifica al paciente {PatientId} y al médico {DoctorId}.",
                appointment.AppointmentId, oldAppointmentDate, appointment.AppointmentDate,
                appointment.PatientId, appointment.DoctorId);

            return Task.CompletedTask;
        }

        public Task NotifyAppointmentConfirmedAsync(Appointment appointment)
        {
            if (appointment is null)
                throw new ArgumentNullException(nameof(appointment));

            var patientName = appointment.Patient?.PatientNavigation != null
                ? $"{appointment.Patient.PatientNavigation.FirstName} {appointment.Patient.PatientNavigation.LastName}"
                : $"Paciente #{appointment.PatientId}";

            // SIMULACIÓN: aquí iría la llamada real al proveedor de correo
            _logger.LogInformation(
                "SIMULACIÓN: Notificación de confirmación — Cita {AppointmentId} confirmada por el médico. " +
                "{PatientName} notificado para {AppointmentDate}.",
                appointment.AppointmentId, patientName, appointment.AppointmentDate);

            return Task.CompletedTask;
        }

        public Task NotifyAppointmentRejectedAsync(Appointment appointment)
        {
            if (appointment is null)
                throw new ArgumentNullException(nameof(appointment));

            var patientName = appointment.Patient?.PatientNavigation != null
                ? $"{appointment.Patient.PatientNavigation.FirstName} {appointment.Patient.PatientNavigation.LastName}"
                : $"Paciente #{appointment.PatientId}";

            // SIMULACIÓN: aquí iría la llamada real al proveedor de correo
            _logger.LogInformation(
                "SIMULACIÓN: Notificación de rechazo — Cita {AppointmentId} rechazada por el médico. " +
                "{PatientName} notificado para que seleccione una nueva fecha (horario anterior: {AppointmentDate}).",
                appointment.AppointmentId, patientName, appointment.AppointmentDate);

            return Task.CompletedTask;
        }
    }
}