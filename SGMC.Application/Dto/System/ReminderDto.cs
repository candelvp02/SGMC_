namespace SGMC.Application.Dto.System
{
    public class ReminderDto
    {
        public int ReminderId { get; set; }
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}