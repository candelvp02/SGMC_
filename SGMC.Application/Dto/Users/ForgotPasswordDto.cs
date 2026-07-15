using System.ComponentModel.DataAnnotations;

namespace SGMC.Application.Dto.Users
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = string.Empty;
    }
}