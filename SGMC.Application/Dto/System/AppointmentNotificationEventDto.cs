namespace SGMC.Application.Dto.System
{
    public enum AppointmentNotificationEventType
    {
        NuevaCita,
        CitaConfirmada,
        CitaCancelada,
        CitaReprogramada
    }

    public enum NotificationRecipientType
    {
        Doctor,
        Patient
    }

    public class AppointmentNotificationEventDto
    {
        public AppointmentNotificationEventType EventType { get; set; }
        public NotificationRecipientType RecipientType { get; set; }
        public int AppointmentId { get; set; }

        public int RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;

        public string CounterpartName { get; set; } = string.Empty;

        public DateTime AppointmentDate { get; set; }
        public DateTime? PreviousAppointmentDate { get; set; }

        public DateTime QueuedAt { get; set; }
    }
}