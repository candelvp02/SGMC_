namespace SGMC.Application.Dto.System
{
    public class ScheduleReminderDto
    {
        public int AppointmentId { get; set; }
        public int? TemplateId { get; set; }
        public string? CustomMessage { get; set; }
        public DateTime ScheduledAt { get; set; }
    }
}