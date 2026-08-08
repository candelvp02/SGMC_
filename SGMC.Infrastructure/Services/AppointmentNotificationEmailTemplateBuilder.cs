using System.Globalization;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Infrastructure.Services
{
    public class AppointmentNotificationEmailTemplateBuilder : IAppointmentNotificationEmailBuilder
    {
        private static readonly CultureInfo EsDo = new("es-DO");

        public AppointmentNotificationEmailContent Build(AppointmentNotificationEventDto data)
        {
            var presentation = GetEventPresentation(data);

            var appointmentDateText = FormatDate(data.AppointmentDate);
            var sentAtText = data.QueuedAt.ToString("dd/MM/yyyy HH:mm", EsDo);
            var counterpartLabel = data.RecipientType == NotificationRecipientType.Doctor ? "Paciente" : "Médico";

            var oldDateRow = data.PreviousAppointmentDate is not null
                ? $@"
                <tr>
                    <td style=""padding:8px 0;color:#6b7280;font-size:14px;"">Fecha anterior</td>
                    <td style=""padding:8px 0;color:#111827;font-size:14px;text-decoration:line-through;"">{FormatDate(data.PreviousAppointmentDate.Value)}</td>
                </tr>"
                : string.Empty;

            var greeting = data.RecipientType == NotificationRecipientType.Doctor
                ? $"{data.RecipientName},"
                : $"Hola {data.RecipientName},";

            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
<meta charset=""UTF-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>{presentation.Subject}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f3f4f6;font-family:Segoe UI, Arial, sans-serif;"">
  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f3f4f6;padding:24px 0;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 1px 3px rgba(0,0,0,0.1);"">
          <tr>
            <td style=""background-color:{presentation.AccentColor};padding:20px 32px;"">
              <span style=""display:inline-block;background-color:rgba(255,255,255,0.2);color:#ffffff;font-size:12px;font-weight:600;letter-spacing:0.5px;padding:4px 10px;border-radius:12px;text-transform:uppercase;"">{presentation.BadgeText}</span>
              <h1 style=""margin:12px 0 0;color:#ffffff;font-size:20px;font-weight:600;"">SGMC — Sistema de Gestión Médica</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px;"">
              <p style=""margin:0 0 8px;color:#111827;font-size:16px;"">{greeting}</p>
              <p style=""margin:0 0 24px;color:#374151;font-size:15px;line-height:1.5;"">{presentation.IntroText}</p>

              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-top:1px solid #e5e7eb;border-bottom:1px solid #e5e7eb;"">
                <tr>
                  <td style=""padding:8px 0;color:#6b7280;font-size:14px;width:40%;"">{counterpartLabel}</td>
                  <td style=""padding:8px 0;color:#111827;font-size:14px;font-weight:600;"">{data.CounterpartName}</td>
                </tr>
                <tr>
                  <td style=""padding:8px 0;color:#6b7280;font-size:14px;"">Fecha y hora de la cita</td>
                  <td style=""padding:8px 0;color:#111827;font-size:14px;font-weight:600;"">{appointmentDateText}</td>
                </tr>{oldDateRow}
              </table>

              <p style=""margin:24px 0 0;color:#9ca3af;font-size:12px;line-height:1.5;"">
                Notificación generada automáticamente por SGMC el {sentAtText}. Este es un mensaje informativo, por favor no respondas a este correo.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            return new AppointmentNotificationEmailContent
            {
                Subject = presentation.Subject,
                HtmlBody = html
            };
        }

        private static string FormatDate(DateTime date)
        {
            var text = date.ToString("dddd dd 'de' MMMM 'de' yyyy, hh:mm tt", EsDo);
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        private static (string AccentColor, string BadgeText, string Subject, string IntroText) GetEventPresentation(AppointmentNotificationEventDto data)
        {
            var isDoctor = data.RecipientType == NotificationRecipientType.Doctor;

            return data.EventType switch
            {
                AppointmentNotificationEventType.NuevaCita => (
                    "#16a34a", "Nueva cita", $"Nueva cita agendada — {data.CounterpartName}",
                    "Un paciente agendó una nueva cita contigo. A continuación los detalles:"),

                AppointmentNotificationEventType.CitaConfirmada => isDoctor
                    ? ("#16a34a", "Cita confirmada", $"Cita confirmada — {data.CounterpartName}",
                       "Confirmaste la siguiente cita. Quedó registrada en tu agenda:")
                    : ("#16a34a", "Cita confirmada", $"Tu cita fue confirmada — {data.CounterpartName}",
                       "El médico confirmó tu cita. Estos son los detalles:"),

                AppointmentNotificationEventType.CitaCancelada => isDoctor
                    ? ("#dc2626", "Cita cancelada", $"Cita cancelada — {data.CounterpartName}",
                       "La siguiente cita fue cancelada y el horario ya quedó liberado en tu agenda:")
                    : ("#dc2626", "Cita cancelada", $"Tu cita fue cancelada — {data.CounterpartName}",
                       "La siguiente cita fue cancelada. Podés agendar un nuevo horario cuando quieras:"),

                AppointmentNotificationEventType.CitaReprogramada => isDoctor
                    ? ("#d97706", "Cita reprogramada", $"Cita reprogramada — {data.CounterpartName}",
                       "La siguiente cita fue reprogramada. Estos son los nuevos detalles:")
                    : ("#d97706", "Cita reprogramada", $"Tu cita fue reprogramada — {data.CounterpartName}",
                       "Tu cita fue reprogramada. Estos son los nuevos detalles:"),

                _ => ("#2563eb", "Actualización", "Actualización de tu agenda", "Hubo una actualización relacionada a una cita.")
            };
        }
    }
}