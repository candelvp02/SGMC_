using SGMC.Application.Dto.System;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAppointmentNotificationEmailBuilder
    {
        AppointmentNotificationEmailContent Build(AppointmentNotificationEventDto data);
    }
}