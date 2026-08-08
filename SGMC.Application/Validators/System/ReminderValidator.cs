using SGMC.Application.Dto.System;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.System
{
    public static class ReminderValidator
    {
        public static OperationResult IsValidDto(this ScheduleReminderDto dto)
        {
            var errores = new List<string>();

            if (dto.AppointmentId <= 0)
                errores.Add("La cita es requerida.");

            if (dto.ScheduledAt == default)
                errores.Add("La fecha y hora de envío del recordatorio es requerida.");
            else if (dto.ScheduledAt <= DateTime.Now)
                errores.Add("La fecha y hora del recordatorio debe estar en el futuro.");

            var tienePlantilla = dto.TemplateId.HasValue && dto.TemplateId.Value > 0;
            var tieneMensajePersonalizado = !string.IsNullOrWhiteSpace(dto.CustomMessage);

            if (!tienePlantilla && !tieneMensajePersonalizado)
                errores.Add("Debes redactar un mensaje o seleccionar una plantilla predefinida.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación del recordatorio.", errores)
                : OperationResult.Exito();
        }
    }
}
