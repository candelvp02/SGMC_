using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Base;

namespace SGMC.Application.Dto.System
{
    public class DoctorDto : PersonBaseDto
    {
        public int DoctorId { get; set; }
        public short SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? ClinicAddress { get; set; }
        public short? AvailabilityModeId { get; set; }
        public string? AvailabilityMode { get; set; }
        public DateOnly LicenseExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public List<AvailabilityDto> UpcomingAvailability { get; set; } = new();
    }
}