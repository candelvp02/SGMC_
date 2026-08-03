using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Repositories.System;

namespace SGMC.Infrastructure.BackgroundServices
{
    public class AppointmentNotificationBackgroundService : BackgroundService
    {
        private readonly IAppointmentNotificationEventQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentNotificationBackgroundService> _logger;

        public AppointmentNotificationBackgroundService(
            IAppointmentNotificationEventQueue queue,
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentNotificationBackgroundService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentNotificationBackgroundService iniciado. Escuchando eventos de citas...");

            await foreach (var evento in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessEventAsync(evento, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error procesando notificación de correo para la cita {AppointmentId} (evento {EventType}, destinatario {RecipientType}).",
                        evento.AppointmentId, evento.EventType, evento.RecipientType);
                }
            }
        }

        private async Task ProcessEventAsync(AppointmentNotificationEventDto evento, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var emailBuilder = scope.ServiceProvider.GetRequiredService<IAppointmentNotificationEmailBuilder>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
            var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            var email = emailBuilder.Build(evento);

            await emailSender.SendAsync(evento.RecipientEmail, email.Subject, email.HtmlBody, stoppingToken);

            // —————— Registrar fecha/hora de envío de cada notificación ——————
            var sentAt = DateTime.Now;
            await notificationRepository.AddAsync(new Notification
            {
                UserId = evento.RecipientUserId,
                RecipientId = evento.RecipientUserId,
                Title = email.Subject,
                IsRead = false,
                CreatedAt = sentAt,
                SentAt = sentAt
            });

            _logger.LogInformation(
                "Notificación de correo enviada a {RecipientType} {RecipientName} ({RecipientEmail}) — evento {EventType}, cita {AppointmentId}.",
                evento.RecipientType, evento.RecipientName, evento.RecipientEmail, evento.EventType, evento.AppointmentId);
        }
    }
}