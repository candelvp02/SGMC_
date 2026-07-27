using SGMC.Application.Dto.System;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAppointmentNotificationEventQueue
    {
        void Enqueue(AppointmentNotificationEventDto item);
        IAsyncEnumerable<AppointmentNotificationEventDto> DequeueAllAsync(CancellationToken cancellationToken);
    }
}