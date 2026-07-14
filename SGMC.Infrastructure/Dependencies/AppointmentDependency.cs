using Microsoft.Extensions.DependencyInjection;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Persistence.Repositories.Appointments;

namespace SGMC.Infrastructure.Dependencies
{
    public static class AppointmentDependency
    {
        public static void AddAppointmentDependencies(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IDoctorAvailabilityRepository, DoctorAvailabilityRepository>();

            // Services
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAppointmentNotificationService, AppointmentNotificationService>();

        }
    }
}