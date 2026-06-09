using Microsoft.Extensions.DependencyInjection;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Repositories.Users;
using SGMC.Persistence.Repositories.Users;

namespace SGMC.Infrastructure.Dependencies
{
    public static class AuthDependency
    {
        public static void AddAuthDependencies(this IServiceCollection services)
        {
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<IAuthService, AuthService>();
        }
    }
}