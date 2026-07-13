using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGMC.Application.Dto.System
{

    public class UpdateDoctorDto
    {
        public int DoctorId { get; set; }
        public short SpecialtyId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? ClinicAddress { get; set; }
        public short? AvailabilityMode { get; set; }
        public DateOnly LicenseExpirationDate { get; set; }
        public bool IsActive { get; set; }
    }
}