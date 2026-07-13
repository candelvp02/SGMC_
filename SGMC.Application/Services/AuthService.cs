using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SGMC.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IPasswordResetTokenRepository tokenRepository,
            INotificationService notificationService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OperationResult<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            if (dto is null)
                return OperationResult<LoginResponseDto>.Fallo("Credenciales inválidas.");

            try
            {
                // 1. Buscar usuario por email
                var userResult = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
                if (userResult is null || !VerifyPassword(dto.Password, userResult.PasswordHash))
                {
                    // Mensaje genérico — no revelar qué campo falló (Task 4)
                    _logger.LogWarning("Intento de login fallido para email: {Email}", dto.Email);
                    return OperationResult<LoginResponseDto>.Fallo(
                        "Correo electrónico o contraseña incorrectos.");
                }

                // 2. Verificar cuenta activa
                if (!userResult.IsActive)
                {
                    return OperationResult<LoginResponseDto>.Fallo(
                        "Tu cuenta aún no está activa. Por favor confirma tu correo electrónico.");
                }

                // 3. Determinar nombre del rol
                var roleName = userResult.RoleId switch
                {
                    1 => "Administrador",
                    2 => "Médico",
                    3 => "Paciente",
                    _ => "Desconocido"
                };

                // 4. Generar JWT
                var (token, expiration) = GenerateJwtToken(userResult, roleName);

                _logger.LogInformation("Login exitoso para usuario {UserId} con rol {Role}",
                    userResult.UserId, roleName);

                return OperationResult<LoginResponseDto>.Exito(new LoginResponseDto
                {
                    UserId = userResult.UserId,
                    Email = userResult.Email,
                    RoleName = roleName,
                    RoleId = userResult.RoleId ?? 0,
                    AccessToken = token,
                    TokenExpiration = expiration,
                    RequiresTwoFactor = false   // extensión futura
                }, "Inicio de sesión exitoso.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno durante login para {Email}", dto.Email);
                return OperationResult<LoginResponseDto>.Fallo("Error interno. Intenta de nuevo.");
            }
        }

        public async Task<OperationResult> ForgotPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return OperationResult.Fallo("El correo electrónico es requerido.");

            try
            {
                var user = await _userRepository.GetByEmailAsync(email.Trim().ToLower());

                // Respuesta siempre exitosa — no revelar si el email existe (seguridad)
                if (user is null)
                {
                    _logger.LogWarning("Solicitud de reset para email no registrado: {Email}", email);
                    return OperationResult.Exito(
                        "Si el correo está registrado, recibirás las instrucciones en breve.");
                }

                // Invalidar tokens previos del usuario
                await _tokenRepository.InvalidatePreviousTokensAsync(user.UserId);

                // Generar token seguro
                var rawToken = GenerateSecureToken();
                var resetToken = new PasswordResetToken
                {
                    UserId = user.UserId,
                    Token = HashToken(rawToken),     // guardamos el hash, no el raw
                    ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _tokenRepository.AddAsync(resetToken);

                // Enviar email con el token RAW (el usuario lo recibe en el link)
                await _notificationService.SendPasswordResetEmailAsync(email, user.UserId);

                _logger.LogInformation("Token de reset generado para usuario {UserId}", user.UserId);

                return OperationResult.Exito(
                    "Si el correo está registrado, recibirás las instrucciones en breve.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de reset para {Email}", email);
                return OperationResult.Fallo("Error interno. Intenta de nuevo.");
            }
        }

        public async Task<OperationResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto is null)
                return OperationResult.Fallo("Datos inválidos.");

            try
            {
                var user = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
                if (user is null)
                    return OperationResult.Fallo("El enlace de recuperación no es válido o ha expirado.");

                var hashedToken = HashToken(dto.Token);
                var resetToken = await _tokenRepository.GetValidTokenAsync(user.UserId, hashedToken);

                if (resetToken is null || resetToken.ExpiresAt < DateTime.UtcNow || resetToken.IsUsed)
                    return OperationResult.Fallo("El enlace de recuperación no es válido o ha expirado.");

                // Actualizar contraseña
                user.PasswordHash = HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Marcar token como usado
                resetToken.IsUsed = true;
                await _tokenRepository.UpdateAsync(resetToken);

                _logger.LogInformation("Contraseña restablecida para usuario {UserId}", user.UserId);

                return OperationResult.Exito("Contraseña restablecida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                return OperationResult.Fallo("Error interno. Intenta de nuevo.");
            }
        }

        public Task<OperationResult> LogoutAsync(int userId)
        {
            // Con JWT stateless el logout se maneja en el cliente eliminando el token.
            // En una implementación con refresh tokens o blacklist, aquí se invalidaría.
            _logger.LogInformation("Logout registrado para usuario {UserId}", userId);
            return Task.FromResult(OperationResult.Exito("Sesión cerrada correctamente."));
        }

        // ─── Helpers privados ───

        private (string token, DateTime expiration) GenerateJwtToken(User user, string roleName)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key no configurada.");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SGMC";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "SGMC";
            var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("roleId", (user.RoleId ?? 0).ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(tokenDescriptor), expiration);
        }

        private static bool VerifyPassword(string plainPassword, string storedHash)
        {
            // BCrypt — consistente con el registro de pacientes/médicos
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }

        private static string HashPassword(string plainPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);
        }

        private static string GenerateSecureToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}