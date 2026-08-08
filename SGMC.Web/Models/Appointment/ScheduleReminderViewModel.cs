using System.ComponentModel.DataAnnotations;
using SGMC.Application.Dto.System;

namespace SGMC.Web.Models.Appointment
{
    public class ScheduleReminderViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string AppointmentDateFormatted { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Selecciona cuándo enviar el recordatorio.")]
        public DateTime ScheduledAt { get; set; }

        public int? SelectedTemplateId { get; set; }
        public string? CustomMessage { get; set; }

        public List<ReminderTemplateDto> Templates { get; set; } = new();
        public List<ReminderDto> ExistingReminders { get; set; } = new();
    }
}