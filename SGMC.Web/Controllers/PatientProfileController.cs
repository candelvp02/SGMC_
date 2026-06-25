using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class PatientProfileController : Controller
    {
        private readonly IPatientService _patientService;

        public PatientProfileController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // GET: /PatientProfile
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var result = await _patientService.GetByUserIdAsync(userId.Value);
            if (!result.Exitoso)
            {
                TempData["Error"] = result.Mensaje;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Datos);
        }

        // POST: /PatientProfile/UpdateContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(PatchPatientContactDto dto, int patientId)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor, corrige los errores del formulario.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _patientService.PatchContactInfoAsync(patientId, dto);

            if (!result.Exitoso)
                TempData["Error"] = result.Mensaje;
            else
                TempData["Success"] = result.Mensaje;

            return RedirectToAction(nameof(Index));
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}