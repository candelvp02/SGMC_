using System.ComponentModel.DataAnnotations;
using SGMC.Application.Dto.Users;

namespace SGMC.Web.Models.Patient
{
    // ViewModel para la vista de creación de pacientes
    public class CreatePatientViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 40 caracteres")]
        [Display(Name = "Nombre")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 40 caracteres")]
        [Display(Name = "Apellido")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cédula es requerida")]
        [RegularExpression(@"^\d{3}-\d{7}-\d{1}$", ErrorMessage = "Formato de cédula inválido (XXX-XXXXXXX-X)")]
        [Display(Name = "Cédula")]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Display(Name = "Fecha de Nacimiento")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Required(ErrorMessage = "El género es requerido")]
        [RegularExpression("^[MF]$", ErrorMessage = "El género debe ser M o F")]
        [Display(Name = "Género")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "La contraseña debe contener al menos una mayúscula, una minúscula y un número")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Formato de teléfono inválido (XXX-XXX-XXXX)")]
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Display(Name = "Dirección")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del contacto de emergencia es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Contacto de Emergencia")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de emergencia es requerido")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono de Emergencia")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de sangre es requerido")]
        [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Tipo de sangre inválido")]
        [Display(Name = "Tipo de Sangre")]
        public string BloodType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Las alergias no pueden exceder 500 caracteres")]
        [Display(Name = "Alergias")]
        public string? Allergies { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor de seguro")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor de seguro válido")]
        [Display(Name = "Proveedor de Seguro")]
        public int InsuranceProviderId { get; set; }

        // Propiedad de navegación para el dropdown
        public List<InsuranceProviderViewModel>? InsuranceProviders { get; set; }

        // Convierte el ViewModel a DTO para la capa de aplicación
        public RegisterPatientDto ToDto()
        {
            return new RegisterPatientDto
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                IdentificationNumber = this.IdentificationNumber,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                Email = this.Email,
                Password = this.Password,
                PhoneNumber = this.PhoneNumber,
                Address = this.Address,
                EmergencyContactName = this.EmergencyContactName,
                EmergencyContactPhone = this.EmergencyContactPhone,
                BloodType = this.BloodType,
                Allergies = this.Allergies,
                InsuranceProviderId = this.InsuranceProviderId
            };
        }
    }

    // ViewModel para la vista de edición de pacientes
    public class EditPatientViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "La dirección debe tener al menos 10 caracteres")]
        [Display(Name = "Dirección")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del contacto es requerido")]
        [Display(Name = "Contacto de Emergencia")]
        public string EmergencyContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de emergencia es requerido")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono de Emergencia")]
        public string EmergencyContactPhone { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Las alergias no pueden exceder 500 caracteres")]
        [Display(Name = "Alergias")]
        public string? Allergies { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor de seguro")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un proveedor válido")]
        [Display(Name = "Proveedor de Seguro")]
        public int InsuranceProviderId { get; set; }

        // Propiedades de solo lectura para mostrar en la vista
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Cédula")]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Display(Name = "Género")]
        public string Gender { get; set; } = string.Empty;

        [Display(Name = "Tipo de Sangre")]
        public string BloodType { get; set; } = string.Empty;

        // Lista de proveedores para el dropdown
        public List<InsuranceProviderViewModel>? InsuranceProviders { get; set; }

        // Crea el ViewModel desde un DTO
        public static EditPatientViewModel FromDto(PatientDto dto)
        {
            return new EditPatientViewModel
            {
                PatientId = dto.PatientId,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                Allergies = dto.Allergies,
                InsuranceProviderId = dto.InsuranceProviderId,
                FullName = dto.FullName,
                IdentificationNumber = dto.IdentificationNumber,
                Gender = dto.Gender,
                BloodType = dto.BloodType
            };
        }

        // Convierte el ViewModel a DTO
        public UpdatePatientDto ToDto()
        {
            return new UpdatePatientDto
            {
                PatientId = this.PatientId,
                PhoneNumber = this.PhoneNumber,
                Address = this.Address,
                EmergencyContactName = this.EmergencyContactName,
                EmergencyContactPhone = this.EmergencyContactPhone,
                Allergies = this.Allergies,
                InsuranceProviderId = this.InsuranceProviderId
            };
        }
    }

    // ViewModel para mostrar lista de pacientes
    public class PatientListViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static PatientListViewModel FromDto(PatientDto dto)
        {
            return new PatientListViewModel
            {
                PatientId = dto.PatientId,
                FullName = dto.FullName,
                IdentificationNumber = dto.IdentificationNumber,
                PhoneNumber = dto.PhoneNumber,
                BloodType = dto.BloodType,
                InsuranceProviderName = dto.InsuranceProviderName,
                IsActive = dto.IsActive
            };
        }
    }

    // ViewModel para detalles del paciente
    public class PatientDetailsViewModel
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public int? Age => DateOfBirth.HasValue
            ? DateTime.Now.Year - DateOfBirth.Value.Year
            : null;
        public string Gender { get; set; } = string.Empty;
        public string GenderDisplay => Gender == "M" ? "Masculino" : "Femenino";
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public string? Allergies { get; set; }
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool HasAllergies => !string.IsNullOrWhiteSpace(Allergies);

        public static PatientDetailsViewModel FromDto(PatientDto dto)
        {
            return new PatientDetailsViewModel
            {
                PatientId = dto.PatientId,
                FullName = dto.FullName,
                IdentificationNumber = dto.IdentificationNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                BloodType = dto.BloodType,
                Allergies = dto.Allergies,
                InsuranceProviderName = dto.InsuranceProviderName,
                IsActive = dto.IsActive
            };
        }
    }

    // ViewModel auxiliar para proveedores de seguro
    public class InsuranceProviderViewModel
    {
        public int InsuranceProviderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CoverageDetails { get; set; } = string.Empty;
        public string NetworkTypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public static InsuranceProviderViewModel FromDto(
            SGMC.Application.Dto.Insurance.InsuranceProviderDto dto)
        {
            return new InsuranceProviderViewModel
            {
                InsuranceProviderId = dto.InsuranceProviderId,
                Name = dto.Name,
                CoverageDetails = dto.CoverageDetails ?? string.Empty,
                NetworkTypeName = dto.NetworkTypeName ?? string.Empty,
                IsActive = dto.IsActive
            };
        }
    }
}