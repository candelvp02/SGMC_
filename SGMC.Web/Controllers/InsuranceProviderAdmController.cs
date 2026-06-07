using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Insurance;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.Controllers
{
    public class InsuranceProviderAdmController : Controller
    {
        private readonly IInsuranceProviderService _insuranceService;

        public InsuranceProviderAdmController(IInsuranceProviderService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _insuranceService.GetAllAsync();
            if (!result.Exitoso)
            {
                ViewBag.ErrorMessage = result.Mensaje;
                return View(new List<InsuranceProviderDto>());
            }
            return View(result.Datos);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateInsuranceProviderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Log de errores de validación
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                    }
                    return View(dto);
                }

                var result = await _insuranceService.CreateAsync(dto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Proveedor de Seguro creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error creando el proveedor: " + ex.Message;
                return View(dto);
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            var result = await _insuranceService.GetByIdAsync(id);
            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Datos);
        }

        public async Task<ActionResult> Edit(int id)
        {
            var result = await _insuranceService.GetByIdAsync(id);
            if (!result.Exitoso || result.Datos == null)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            var updateDto = new UpdateInsuranceProviderDto
            {
                InsuranceProviderId = result.Datos.InsuranceProviderId,
                Name = result.Datos.Name,
                PhoneNumber = result.Datos.PhoneNumber,
                Email = result.Datos.Email,
                Website = result.Datos.Website,
                Address = result.Datos.Address,
                IsActive = result.Datos.IsActive,
                NetworkTypeId = result.Datos.NetworkTypeId
            };

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, UpdateInsuranceProviderDto dto)
        {
            if (id != dto.InsuranceProviderId)
            {
                ViewBag.ErrorMessage = "El ID no coincide.";
                return View(dto);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(dto);
                }

                var result = await _insuranceService.UpdateAsync(dto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Proveedor actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error actualizando el proveedor: " + ex.Message;
                return View(dto);
            }
        }

        public async Task<ActionResult> Delete(int id)
        {
            var result = await _insuranceService.GetByIdAsync(id);
            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Datos);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _insuranceService.DeleteAsync(id);

                TempData["SuccessMessage"] = "Proveedor eliminado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error eliminando el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
