namespace SGMC.Web.Models.Appointment
{
    public class DoctorPendingAppointmentsViewModel
    {
        public int? SelectedDoctorId { get; set; }
        public List<DoctorSelectViewModel> Doctors { get; set; } = new();
        public List<PendingAppointmentViewModel> PendingAppointments { get; set; } = new();
    }
}