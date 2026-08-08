using System.ComponentModel.DataAnnotations.Schema;
using SGMC.Domain.Entities.Appointments;

namespace SGMC.Domain.Entities.System
{
    [Table("Reminders", Schema = "system")]
    public partial class Reminder
    {
        public int ReminderId { get; set; }

        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        // Pendiente / Enviado / Cancelado
        public string Status { get; set; } = "Pendiente";

        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public virtual Appointment? Appointment { get; set; }
    }
}