using System.ComponentModel.DataAnnotations;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;

namespace SGMC.Web.Models.Doctor
{
    // ViewModel para la vista de creación de doctores
    public class CreateDoctorViewModel
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

        [Required(ErrorMessage = "La especialidad es requerida")]
        [Range(1, short.MaxValue, ErrorMessage = "Debe seleccionar una especialidad válida")]
        [Display(Name = "Especialidad")]
        public short SpecialtyId { get; set; }

        [Required(ErrorMessage = "El número de licencia es requerido")]
        [StringLength(50, ErrorMessage = "El número de licencia no puede exceder 50 caracteres")]
        [Display(Name = "Número de Licencia")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [Display(Name = "Vencimiento de Licencia")]
        [DataType(DataType.Date)]
        public DateOnly LicenseExpirationDate { get; set; }

        [Required(ErrorMessage = "Los años de experiencia son requeridos")]
        [Range(0, 60, ErrorMessage = "Los años de experiencia deben estar entre 0 y 60")]
        [Display(Name = "Años de Experiencia")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "La educación es requerida")]
        [StringLength(500, ErrorMessage = "La educación no puede exceder 500 caracteres")]
        [Display(Name = "Educación")]
        public string Education { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La biografía no puede exceder 1000 caracteres")]
        [Display(Name = "Biografía")]
        public string? Bio { get; set; }

        [Display(Name = "Tarifa de Consulta")]
        [Range(0, 999999.99, ErrorMessage = "La tarifa debe ser un valor válido")]
        [DataType(DataType.Currency)]
        public decimal? ConsultationFee { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
        [Display(Name = "Dirección de Clínica")]
        public string? ClinicAddress { get; set; }

        // Lista para el dropdown de especialidades
        public List<SpecialtySelectViewModel>? Specialties { get; set; }

        // Convierte el ViewModel a DTO
        public RegisterDoctorDto ToDto()
        {
            return new RegisterDoctorDto
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                IdentificationNumber = this.IdentificationNumber,
                DateOfBirth = this.DateOfBirth,
                Gender = this.Gender,
                Email = this.Email,
                Password = this.Password,
                PhoneNumber = this.PhoneNumber,
                SpecialtyId = this.SpecialtyId,
                LicenseNumber = this.LicenseNumber,
                LicenseExpirationDate = this.LicenseExpirationDate,
                YearsOfExperience = this.YearsOfExperience,
                Education = this.Education,
                Bio = this.Bio,
                ConsultationFee = this.ConsultationFee,
                ClinicAddress = this.ClinicAddress
            };
        }
    }

    // ViewModel para la vista de edición de doctores
    public class EditDoctorViewModel
    {
        [Required]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        [RegularExpression(@"^\d{3}-\d{3}-\d{4}$", ErrorMessage = "Formato de teléfono inválido")]
        [Display(Name = "Teléfono")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los años de experiencia son requeridos")]
        [Range(0, 60, ErrorMessage = "Los años de experiencia deben estar entre 0 y 60")]
        [Display(Name = "Años de Experiencia")]
        public int YearsOfExperience { get; set; }

        [Required(ErrorMessage = "La educación es requerida")]
        [StringLength(500, ErrorMessage = "La educación no puede exceder 500 caracteres")]
        [Display(Name = "Educación")]
        public string Education { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La biografía no puede exceder 1000 caracteres")]
        [Display(Name = "Biografía")]
        public string? Bio { get; set; }

        [Display(Name = "Tarifa de Consulta")]
        [Range(0, 999999.99, ErrorMessage = "La tarifa debe ser un valor válido")]
        [DataType(DataType.Currency)]
        public decimal? ConsultationFee { get; set; }

        [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
        [Display(Name = "Dirección de Clínica")]
        public string? ClinicAddress { get; set; }

        [Display(Name = "Modo de Disponibilidad")]
        public short? AvailabilityMode { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es requerida")]
        [Display(Name = "Vencimiento de Licencia")]
        [DataType(DataType.Date)]
        public DateOnly LicenseExpirationDate { get; set; }

        // Propiedades de solo lectura
        [Display(Name = "Nombre Completo")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Número de Licencia")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "Especialidad")]
        public string SpecialtyName { get; set; } = string.Empty;

        // Listas para dropdowns
        public List<AvailabilityModeSelectViewModel>? AvailabilityModes { get; set; }

        // Crea el ViewModel desde un DTO
        public static EditDoctorViewModel FromDto(DoctorDto dto)
        {
            return new EditDoctorViewModel
            {
                DoctorId = dto.DoctorId,
                PhoneNumber = dto.PhoneNumber,
                YearsOfExperience = dto.YearsOfExperience,
                Education = dto.Education,
                Bio = dto.Bio,
                ConsultationFee = dto.ConsultationFee,
                ClinicAddress = dto.ClinicAddress,
                LicenseExpirationDate = dto.LicenseExpirationDate,
                FullName = dto.FullName,
                LicenseNumber = dto.LicenseNumber,
                SpecialtyName = dto.SpecialtyName
            };
        }

        // Convierte el ViewModel a DTO
        public UpdateDoctorDto ToDto()
        {
            return new UpdateDoctorDto
            {
                DoctorId = this.DoctorId,
                PhoneNumber = this.PhoneNumber,
                YearsOfExperience = this.YearsOfExperience,
                Education = this.Education,
                Bio = this.Bio,
                ConsultationFee = this.ConsultationFee,
                ClinicAddress = this.ClinicAddress,
                AvailabilityMode = this.AvailabilityMode,
                LicenseExpirationDate = this.LicenseExpirationDate
            };
        }
    }

    // ViewModel para la lista de doctores
    public class DoctorListViewModel
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public decimal? ConsultationFee { get; set; }
        public bool IsActive { get; set; }

        public string ConsultationFeeFormatted => ConsultationFee.HasValue
            ? ConsultationFee.Value.ToString("C")
            : "No especificada";

        public static DoctorListViewModel FromDto(DoctorDto dto)
        {
            return new DoctorListViewModel
            {
                DoctorId = dto.DoctorId,
                FullName = dto.FullName,
                SpecialtyName = dto.SpecialtyName,
                LicenseNumber = dto.LicenseNumber,
                PhoneNumber = dto.PhoneNumber,
                YearsOfExperience = dto.YearsOfExperience,
                ConsultationFee = dto.ConsultationFee,
                IsActive = dto.IsActive
            };
        }
    }

    // ViewModel para detalles del doctor
    public class DoctorDetailsViewModel
    {
        public int DoctorId { get; set; }
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
        public string SpecialtyName { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateOnly LicenseExpirationDate { get; set; }
        public bool IsLicenseValid => LicenseExpirationDate > DateOnly.FromDateTime(DateTime.Now);
        public int YearsOfExperience { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string ConsultationFeeFormatted => ConsultationFee.HasValue
            ? ConsultationFee.Value.ToString("C")
            : "No especificada";
        public string? ClinicAddress { get; set; }
        public string? AvailabilityMode { get; set; }
        public bool IsActive { get; set; }

        public static DoctorDetailsViewModel FromDto(DoctorDto dto)
        {
            return new DoctorDetailsViewModel
            {
                DoctorId = dto.DoctorId,
                FullName = dto.FullName,
                IdentificationNumber = dto.IdentificationNumber,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                SpecialtyName = dto.SpecialtyName,
                LicenseNumber = dto.LicenseNumber,
                LicenseExpirationDate = dto.LicenseExpirationDate,
                YearsOfExperience = dto.YearsOfExperience,
                Education = dto.Education,
                Bio = dto.Bio,
                ConsultationFee = dto.ConsultationFee,
                ClinicAddress = dto.ClinicAddress,
                AvailabilityMode = dto.AvailabilityMode,
                IsActive = dto.IsActive
            };
        }
    }

    // ViewModel auxiliar para selección de especialidades
    public class SpecialtySelectViewModel
    {
        public short SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    // ViewModel auxiliar para modos de disponibilidad
    public class AvailabilityModeSelectViewModel
    {
        public short AvailabilityModeId { get; set; }
        public string AvailabilityModeName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}