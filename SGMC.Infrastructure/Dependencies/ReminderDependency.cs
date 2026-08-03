using Microsoft.Extensions.DependencyInjection;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Repositories.System;
using SGMC.Infrastructure.BackgroundServices;
using SGMC.Persistence.Repositories.System;

namespace SGMC.Infrastructure.Dependencies
{
    public static class ReminderDependency
    {
        public static void AddReminderDependencies(this IServiceCollection services)
        {
            services.AddScoped<IReminderRepository, ReminderRepository>();
            services.AddScoped<IReminderService, ReminderService>();
            services.AddHostedService<ReminderDispatchBackgroundService>();
        }
    }
}