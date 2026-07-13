using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using LoginDto = SGMC.Application.Dto.Users.LoginDto;
using ResetPasswordDto = SGMC.Application.Dto.Users.ResetPasswordDto;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/login — Autentica credenciales y retorna JWT.
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<OperationResult<LoginResponseDto>>> Login(
            [FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Credenciales inválidas."));

            var result = await _authService.LoginAsync(dto);

            if (!result.Exitoso)
                return Unauthorized(result);

            return Ok(result);
        }

        // POST /api/auth/forgot-password — Envía link de recuperación (expira 30 min).
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<OperationResult>> ForgotPassword(
            [FromBody] ForgotPasswordRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Email))
                return BadRequest(OperationResult.Fallo("El correo electrónico es requerido."));

            // Siempre 200 para no revelar si el email existe
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(result);
        }

        // POST /api/auth/reset-password — Valida token y actualiza contraseña.
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<OperationResult>> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(OperationResult.Fallo("Datos inválidos."));

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Exitoso)
                return BadRequest(result);

            return Ok(result);
        }

        // POST /api/auth/logout — Invalida sesión.
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<OperationResult>> Logout()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
                return BadRequest(OperationResult.Fallo("Sesión no identificada."));

            var result = await _authService.LogoutAsync(userId);
            return Ok(result);
        }
    }

    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = string.Empty;
    }
}