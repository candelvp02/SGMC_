using SGMC.Application.Dto.Appointments;

namespace SGMC.Web.Models.Appointment
{
    public class RescheduleAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime CurrentAppointmentDate { get; set; }
        public string CurrentStatusName { get; set; } = string.Empty;

        public static RescheduleAppointmentViewModel FromDto(AppointmentDto dto) => new()
        {
            AppointmentId = dto.AppointmentId,
            DoctorId = dto.DoctorId,
            DoctorName = dto.DoctorName,
            CurrentAppointmentDate = dto.AppointmentDate,
            CurrentStatusName = dto.StatusName
        };
    }
}