using System.Threading.Channels;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Infrastructure.Services
{
    public class AppointmentNotificationEventQueue : IAppointmentNotificationEventQueue
    {
        private readonly Channel<AppointmentNotificationEventDto> _channel =
            Channel.CreateUnbounded<AppointmentNotificationEventDto>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        public void Enqueue(AppointmentNotificationEventDto item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));
            _channel.Writer.TryWrite(item);
        }

        public IAsyncEnumerable<AppointmentNotificationEventDto> DequeueAllAsync(CancellationToken cancellationToken)
            => _channel.Reader.ReadAllAsync(cancellationToken);
    }
}