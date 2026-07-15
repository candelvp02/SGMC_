using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Insurance;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.Controllers
{
    // TASK 128: Solo pacientes autenticados acceden a este controller.
    // Jamás se expone GetAllAsync() aquí; únicamente GetActiveAsync() filtra en origen.
    [Authorize(Roles = "Paciente")]
    public class InsuranceProviderController : Controller
    {
        private readonly IInsuranceProviderService _insuranceService;

        public InsuranceProviderController(IInsuranceProviderService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        // GET: InsuranceProvider
        // TASK 127 / TASK 128: Llama a GetActiveAsync() — solo devuelve proveedores con IsActive = true.
        // Los preferenciales se ordenan primero para resaltarlos visualmente en la vista.
        public async Task<ActionResult> Index()
        {
            var result = await _insuranceService.GetActiveAsync();

            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View(new List<InsuranceProviderDto>());
            }

            // Preferenciales primero, luego alfabético
            var sorted = result.Datos!
                .OrderByDescending(p => p.IsPreferred)
                .ThenBy(p => p.Name)
                .ToList();

            return View(sorted);
        }

        // GET: InsuranceProvider/Details/5
        // TASK 128: Aunque el paciente conozca el ID de un proveedor inactivo,
        // esta validación lo bloquea en el servidor y redirige con mensaje.
        public async Task<ActionResult> Details(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _insuranceService.GetByIdAsync(id);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["ErrorMessage"] = "El proveedor solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            // TASK 128: Segunda línea de defensa — un proveedor inactivo
            // no puede ser consultado por un paciente, aunque conozca su ID.
            if (!result.Datos.IsActive)
            {
                TempData["ErrorMessage"] = "Este proveedor no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            return View(result.Datos);
        }
    }
}