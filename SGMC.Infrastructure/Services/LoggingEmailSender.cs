using Microsoft.Extensions.Logging;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Infrastructure.Services
{
    // IMPORTANTE!!!! SIMULACIÓN de envío de correo.
    public class LoggingEmailSender : IEmailSender
    {
        private readonly ILogger<LoggingEmailSender> _logger;

        public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "SIMULACIÓN: Correo enviado a {ToEmail} — Asunto: \"{Subject}\" ({Length} caracteres de HTML).",
                toEmail, subject, htmlBody?.Length ?? 0);

            return Task.CompletedTask;
        }
    }
}