using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models.Patient;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class PatientProfileController : Controller
    {
        private readonly IPatientService _patientService;
        private readonly IInsuranceProviderService _insuranceProviderService;

        public PatientProfileController(
            IPatientService patientService,
            IInsuranceProviderService insuranceProviderService)
        {
            _patientService = patientService;
            _insuranceProviderService = insuranceProviderService;
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

        // POST: /PatientProfile/UpdateInsurance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInsurance(PatchPatientInsuranceDto dto, int patientId)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debe seleccionar un proveedor de seguro v\u00e1lido.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _patientService.PatchInsuranceProviderAsync(patientId, dto);

            if (!result.Exitoso)
                TempData["Error"] = result.Mensaje;
            else
                TempData["Success"] = result.Mensaje;

            return RedirectToAction(nameof(Index));
        }

        // GET: /PatientProfile/GetInsuranceProviders (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetInsuranceProviders()
        {
            var result = await _insuranceProviderService.GetActiveAsync();
            if (!result.Exitoso || result.Datos == null)
                return Json(new List<object>());

            var providers = result.Datos.Select(p => new
            {
                insuranceProviderId = p.InsuranceProviderId,
                name = p.Name,
                coverageDetails = p.CoverageDetails ?? string.Empty,
                networkTypeName = p.NetworkTypeName ?? string.Empty
            });

            return Json(providers);
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}