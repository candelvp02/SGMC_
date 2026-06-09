using SGMC.Application.Dto.Users;
using SGMC.Domain.Base;


namespace SGMC.Application.Interfaces.Service
{
    public interface IAuthService
    {
        // Valida credenciales y genera JWT. Maneja 2FA si está activo.
        Task<OperationResult<LoginResponseDto>> LoginAsync(LoginDto dto);

        // Genera token de recuperación (30 min) y envía email.
        Task<OperationResult> ForgotPasswordAsync(string email);

        // Valida el token y actualiza el hash de contraseña.
        Task<OperationResult> ResetPasswordAsync(ResetPasswordDto dto);

        // Invalida el token JWT activo (logout).
        Task<OperationResult> LogoutAsync(int userId);
    }
}