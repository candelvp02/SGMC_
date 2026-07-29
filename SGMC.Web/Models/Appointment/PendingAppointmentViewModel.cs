using SGMC.Application.Dto.Appointments;

namespace SGMC.Web.Models.Appointment
{
    public class PendingAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public static PendingAppointmentViewModel FromDto(AppointmentDto dto) => new()
        {
            AppointmentId = dto.AppointmentId,
            PatientName = dto.PatientName,
            AppointmentDate = dto.AppointmentDate,
            CreatedAt = dto.CreatedAt
        };
    }
}