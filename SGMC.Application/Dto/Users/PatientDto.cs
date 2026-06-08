using SGMC.Application.Dto.Base;
using System.ComponentModel.DataAnnotations;

namespace SGMC.Application.Dto.Users
{
    public class PatientDto : PersonBaseDto
    {
        public int PatientId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public int InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RegisterPatientDto : RegisterPersonBaseDto
    {
        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "El contacto de emergencia es requerido")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de emergencia es requerido")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de sangre es requerido")]
        public string BloodType { get; set; } = string.Empty;

        public string? Allergies { get; set; }

        [Required(ErrorMessage = "El proveedor de seguro es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor de seguro válido")]
        public int InsuranceProviderId { get; set; }
    }

    public class UpdatePatientDto
    {
        public int PatientId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public int InsuranceProviderId { get; set; }
    }
}