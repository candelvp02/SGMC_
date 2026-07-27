using SGMC.Domain.Entities.Appointments;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAppointmentNotificationService
    {
        Task NotifyAppointmentCreatedAsync(Appointment appointment); // solo médico
        Task NotifyAppointmentConfirmedAsync(Appointment appointment); // paciente + médico
        Task NotifyAppointmentCancelledAsync(Appointment appointment); // paciente + médico
        Task NotifyAppointmentRescheduledAsync(Appointment appointment, DateTime oldAppointmentDate); // paciente + médico
    }
}