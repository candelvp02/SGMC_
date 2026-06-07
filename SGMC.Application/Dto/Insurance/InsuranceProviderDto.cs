using System.ComponentModel.DataAnnotations;

namespace SGMC.Application.Dto.Insurance
{
    public class InsuranceProviderDto
    {
        public int InsuranceProviderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Website { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string CoverageDetails { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsPreferred { get; set; }
        public int NetworkTypeId { get; set; }
        public string NetworkTypeName { get; set; } = string.Empty;
        public string? CustomerSupportContact { get; set; }
        public string? AcceptedRegions { get; set; }
        public decimal? MaxCoverageAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateInsuranceProviderDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        public string? Website { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Address { get; set; } = string.Empty;

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }

        [Required(ErrorMessage = "Los detalles de cobertura son requeridos")]
        public string CoverageDetails { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }
        public bool IsPreferred { get; set; } = false;

        [Required(ErrorMessage = "El tipo de red es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de red")]
        public int NetworkTypeId { get; set; }

        public string? CustomerSupportContact { get; set; }
        public string? AcceptedRegions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser un valor positivo")]
        public decimal? MaxCoverageAmount { get; set; }
    }

    public class UpdateInsuranceProviderDto
    {
        public int InsuranceProviderId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        public string? Website { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Address { get; set; } = string.Empty;

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }

        [Required(ErrorMessage = "Los detalles de cobertura son requeridos")]
        public string CoverageDetails { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }
        public bool IsPreferred { get; set; }

        [Required(ErrorMessage = "El tipo de red es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo de red")]
        public int NetworkTypeId { get; set; }

        public string? CustomerSupportContact { get; set; }
        public string? AcceptedRegions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser un valor positivo")]
        public decimal? MaxCoverageAmount { get; set; }

        public bool IsActive { get; set; }
    }
}