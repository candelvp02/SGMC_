using System.ComponentModel.DataAnnotations;

namespace SGMC.Application.Dto.Users
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;

        public string? TwoFactorCode { get; set; }
    }
}