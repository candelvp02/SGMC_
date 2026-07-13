using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.LoginAsync(dto);

            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return View(dto);
            }

            // Crear cookie de autenticación con los claims del JWT
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Datos!.UserId.ToString()),
                new(ClaimTypes.Email, result.Datos.Email),
                new(ClaimTypes.Role, result.Datos.RoleName),
                new("AccessToken", result.Datos.AccessToken)
            };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false });

            // Redirección por rol
            return result.Datos.RoleName switch
            {
                "Administrador" => RedirectToAction("Index", "Admin"),
                "Médico" => RedirectToAction("Index", "Doctor"),
                _ => RedirectToAction("Index", "Patient")
            };
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _authService.ForgotPasswordAsync(dto.Email);

            // Mensaje siempre positivo — no revelar si el email existe
            TempData["SuccessMessage"] =
                "Si tu correo está registrado, recibirás las instrucciones en breve.";
            return View(dto);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordDto { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return View(dto);
            }

            TempData["InfoMessage"] = "Contraseña restablecida. Puedes iniciar sesión.";
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim?.Value, out var userId))
                await _authService.LogoutAsync(userId);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}