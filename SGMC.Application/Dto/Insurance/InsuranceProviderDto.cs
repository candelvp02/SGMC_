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
        public decimal? MaxCoverageAmount { get; set; }
        public bool IsActive { get; set; }
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

        [Required(ErrorMessage = "La dirección es requerida")]
        public string Address { get; set; } = string.Empty;

        public string? Website { get; set; }
        public string CoverageDetails { get; set; } = "Cobertura estándar";
        public int NetworkTypeId { get; set; } = 1;
    }
    public class UpdateInsuranceProviderDto
    {
        public int InsuranceProviderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public int NetworkTypeId { get; set; } = 1;
    }
}