namespace SGMC.Application.Dto.System
{
    public class ReminderTemplateDto
    {
        public int TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
    }
}