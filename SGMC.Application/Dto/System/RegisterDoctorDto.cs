using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Base;


namespace SGMC.Application.Dto.System 
{ 
    public class RegisterDoctorDto : RegisterPersonBaseDto
{
    public short SpecialtyId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Education { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal? ConsultationFee { get; set; }
    public string? ClinicAddress { get; set; }
    public DateOnly LicenseExpirationDate { get; set; }
} 
}