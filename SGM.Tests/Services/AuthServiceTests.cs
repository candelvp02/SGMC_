using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPasswordResetTokenRepository> _tokenRepoMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly IAuthService _service;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _tokenRepoMock = new Mock<IPasswordResetTokenRepository>();
            _notificationMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<AuthService>>();
            _configMock = new Mock<IConfiguration>();

            // Configurar JWT mínimo para los tests
            _configMock.Setup(c => c["Jwt:Key"])
                .Returns("SGMC_TEST_SECRET_KEY_MINIMO_32_CHARS!!");
            _configMock.Setup(c => c["Jwt:Issuer"]).Returns("SGMC");
            _configMock.Setup(c => c["Jwt:Audience"]).Returns("SGMC");
            _configMock.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

            _service = new AuthService(
                _userRepoMock.Object,
                _tokenRepoMock.Object,
                _notificationMock.Object,
                _configMock.Object,
                _loggerMock.Object);
        }

        // ─── Login — Mensajes genéricos (TASK 4 core) ───────────────────

        // PRUEBA 1: Email incorrecto debe retornar mensaje genérico
        [Fact]
        public async Task Login_WrongEmail_ReturnsGenericMessage()
        {
            _userRepoMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "noexiste@test.com",
                Password = "Password1!"
            });

            result.Exitoso.Should().BeFalse();
            // El mensaje NO debe indicar si fue el email o la contraseña
            result.Mensaje.ToLower().Should().NotContain("correo no");
            result.Mensaje.ToLower().Should().NotContain("email no");
            result.Mensaje.ToLower().Should().NotContain("contraseña incorrecta");
            result.Mensaje.Should().Contain("Correo electrónico o contraseña incorrectos");
        }

        // PRUEBA 2: Contraseña incorrecta debe retornar el MISMO mensaje genérico
        [Fact]
        public async Task Login_WrongPassword_ReturnsSameGenericMessage()
        {
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("juan@test.com"))
                .ReturnsAsync(new User
                {
                    UserId = 1,
                    Email = "juan@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass1!"),
                    IsActive = true,
                    RoleId = 3
                });

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "juan@test.com",
                Password = "WrongPass999!"
            });

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("Correo electrónico o contraseña incorrectos.");
        }

        // PRUEBA 3: Cuenta inactiva debe informar confirmar correo
        [Fact]
        public async Task Login_InactiveAccount_ReturnsActivationMessage()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Pass1234!");
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("inactive@test.com"))
                .ReturnsAsync(new User
                {
                    UserId = 2,
                    Email = "inactive@test.com",
                    PasswordHash = passwordHash,
                    IsActive = false,
                    RoleId = 3
                });

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "inactive@test.com",
                Password = "Pass1234!"
            });

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("activa");
            result.Mensaje.Should().Contain("confirma tu correo");
        }

        // PRUEBA 4: Credenciales correctas deben generar JWT válido
        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("ValidPass1!");
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("medico@test.com"))
                .ReturnsAsync(new User
                {
                    UserId = 5,
                    Email = "medico@test.com",
                    PasswordHash = passwordHash,
                    IsActive = true,
                    RoleId = 2  // Médico
                });

            var result = await _service.LoginAsync(new LoginDto
            {
                Email = "medico@test.com",
                Password = "ValidPass1!"
            });

            result.Exitoso.Should().BeTrue();
            result.Datos.Should().NotBeNull();
            result.Datos!.AccessToken.Should().NotBeNullOrWhiteSpace();
            result.Datos.RoleName.Should().Be("Médico");
            result.Datos.TokenExpiration.Should().BeAfter(DateTime.UtcNow);
        }

        // PRUEBA 5: Login con dto null debe fallar
        [Fact]
        public async Task Login_NullDto_ReturnsFailure()
        {
            var result = await _service.LoginAsync(null!);

            result.Exitoso.Should().BeFalse();
        }

        // ─── ForgotPassword — Seguridad ─────────────────────────────────

        // PRUEBA 6: Email no registrado responde igual que uno registrado (no revelar)
        [Fact]
        public async Task ForgotPassword_UnknownEmail_ReturnsSameMessageAsKnownEmail()
        {
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("noexiste@test.com"))
                .ReturnsAsync((User?)null);

            var resultUnknown = await _service.ForgotPasswordAsync("noexiste@test.com");

            resultUnknown.Exitoso.Should().BeTrue();
            resultUnknown.Mensaje.Should().Contain("Si el correo está registrado");
        }

        // PRUEBA 7: Email registrado también responde con mensaje genérico
        [Fact]
        public async Task ForgotPassword_KnownEmail_ReturnsSameGenericMessage()
        {
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("juan@test.com"))
                .ReturnsAsync(new User { UserId = 1, Email = "juan@test.com" });

            _tokenRepoMock
                .Setup(r => r.InvalidatePreviousTokensAsync(1))
                .Returns(Task.CompletedTask);
            _tokenRepoMock
                .Setup(r => r.AddAsync(It.IsAny<PasswordResetToken>()))
                .Returns(Task.CompletedTask);
            _notificationMock
                .Setup(n => n.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(OperationResult.Exito("Enviado"));

            var resultKnown = await _service.ForgotPasswordAsync("juan@test.com");

            resultKnown.Exitoso.Should().BeTrue();
            resultKnown.Mensaje.Should().Contain("Si el correo está registrado");
        }

        // PRUEBA 8: Email vacío en forgot password debe fallar
        [Fact]
        public async Task ForgotPassword_EmptyEmail_ReturnsFailure()
        {
            var result = await _service.ForgotPasswordAsync(string.Empty);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("requerido");
        }

        // ─── ResetPassword ───────────────────────────────────────────────

        // PRUEBA 9: Token expirado debe rechazar el reset
        [Fact]
        public async Task ResetPassword_ExpiredToken_ReturnsFailure()
        {
            _userRepoMock
                .Setup(r => r.GetByEmailAsync("juan@test.com"))
                .ReturnsAsync(new User { UserId = 1, Email = "juan@test.com" });

            // Simular que no existe token válido (expirado/usado)
            _tokenRepoMock
                .Setup(r => r.GetValidTokenAsync(1, It.IsAny<string>()))
                .ReturnsAsync((PasswordResetToken?)null);

            var result = await _service.ResetPasswordAsync(new ResetPasswordDto
            {
                Email = "juan@test.com",
                Token = "token-expirado",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            });

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("expirado");
        }

        // PRUEBA 10: Token válido debe actualizar contraseña correctamente
        [Fact]
        public async Task ResetPassword_ValidToken_UpdatesPasswordSuccessfully()
        {
            var user = new User
            {
                UserId = 1,
                Email = "juan@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123!")
            };

            _userRepoMock
                .Setup(r => r.GetByEmailAsync("juan@test.com"))
                .ReturnsAsync(user);

            _tokenRepoMock
                .Setup(r => r.GetValidTokenAsync(1, It.IsAny<string>()))
                .ReturnsAsync(new PasswordResetToken
                {
                    Id = 1,
                    UserId = 1,
                    Token = "hashed-token",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    IsUsed = false
                });

            _userRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);
            _tokenRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<PasswordResetToken>()))
                .Returns(Task.CompletedTask);

            var result = await _service.ResetPasswordAsync(new ResetPasswordDto
            {
                Email = "juan@test.com",
                Token = "raw-valid-token",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            });

            result.Exitoso.Should().BeTrue();
            result.Mensaje.Should().Contain("restablecida");
        }
    }
}