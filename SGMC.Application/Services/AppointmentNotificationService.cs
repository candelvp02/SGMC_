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
    }
}