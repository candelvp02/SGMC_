using Microsoft.Extensions.DependencyInjection;
using SGMC.Application.Interfaces.Service;
using SGMC.Infrastructure.BackgroundServices;
using SGMC.Infrastructure.Services;

namespace SGMC.Infrastructure.Dependencies
{
    public static class AppointmentNotificationDependency
    {
        public static void AddAppointmentNotificationDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IAppointmentNotificationEventQueue, AppointmentNotificationEventQueue>();
            services.AddSingleton<IAppointmentNotificationEmailBuilder, AppointmentNotificationEmailTemplateBuilder>();

            // SIMULADO por ahora.
            services.AddTransient<IEmailSender, LoggingEmailSender>();

            services.AddHostedService<AppointmentNotificationBackgroundService>();
        }
    }
}