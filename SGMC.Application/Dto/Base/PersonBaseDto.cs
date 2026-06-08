using System.ComponentModel.DataAnnotations;

namespace SGMC.Application.Dto.Base
{
    public abstract class PersonBaseDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
    }
    public abstract class RegisterPersonBaseDto : PersonBaseDto
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}