using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Repositories.System;

namespace SGMC.Infrastructure.BackgroundServices
{
    // Task 105 — Scheduler que revisa periódicamente los recordatoriospendientes y despacha los que ya llegaron a su fecha/hora programada.
    public class ReminderDispatchBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderDispatchBackgroundService> _logger;

        public ReminderDispatchBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderDispatchBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "ReminderDispatchBackgroundService iniciado. Revisando cada {Interval}s.",
                PollInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DispatchDueRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error revisando recordatorios pendientes de despacho.");
                }

                try
                {
                    await Task.Delay(PollInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // el servicio se está deteniendo
                }
            }
        }

        private async Task DispatchDueRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var due = await reminderRepository.GetDueRemindersAsync(DateTime.Now);
            if (due.Count == 0)
                return;

            foreach (var reminder in due)
            {
                try
                {
                    var html = BuildReminderHtml(reminder.PatientName, reminder.Message);
                    await emailSender.SendAsync(reminder.PatientEmail, "Recordatorio de tu cita médica", html, stoppingToken);

                    reminder.Status = "Enviado";
                    reminder.SentAt = DateTime.Now;
                    await reminderRepository.UpdateAsync(reminder);

                    _logger.LogInformation(
                        "Recordatorio {ReminderId} enviado a {Email} (cita {AppointmentId}).",
                        reminder.ReminderId, reminder.PatientEmail, reminder.AppointmentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error enviando el recordatorio {ReminderId} de la cita {AppointmentId}.",
                        reminder.ReminderId, reminder.AppointmentId);
                }
            }
        }

        private static string BuildReminderHtml(string patientName, string message)
        {
            return $@"<!DOCTYPE html>
<html lang=""es"">
<head><meta charset=""UTF-8"" /><title>Recordatorio de tu cita</title></head>
<body style=""margin:0;padding:0;background-color:#f3f4f6;font-family:Segoe UI, Arial, sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f3f4f6;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
          <tr>
            <td style=""background-color:#2563eb;padding:20px 32px;"">
              <span style=""display:inline-block;background-color:rgba(255,255,255,0.2);color:#ffffff;font-size:12px;font-weight:600;letter-spacing:0.5px;padding:4px 10px;border-radius:12px;text-transform:uppercase;"">Recordatorio</span>
              <h1 style=""margin:12px 0 0;color:#ffffff;font-size:20px;font-weight:600;"">SGMC — Sistema de Gestión Médica</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 8px;color:#111827;font-size:16px;"">Hola {patientName},</p>
              <p style=""margin:0;color:#374151;font-size:15px;line-height:1.6;white-space:pre-line;"">{message}</p>
              <p style=""margin:24px 0 0;color:#9ca3af;font-size:12px;line-height:1.5;"">
                Recordatorio enviado automáticamente por SGMC. Este es un mensaje informativo, por favor no respondas a este correo.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}