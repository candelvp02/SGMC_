using SGMC.Domain.Entities.Appointments;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAppointmentNotificationService
    {
        Task NotifyAppointmentRescheduledAsync(Appointment appointment, DateTime oldAppointmentDate);
    }
}