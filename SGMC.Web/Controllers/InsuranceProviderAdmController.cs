using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Insurance;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class InsuranceProviderAdmController : Controller
    {
        private readonly IInsuranceProviderService _insuranceService;

        public InsuranceProviderAdmController(IInsuranceProviderService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        private List<NetworkTypeDto> GetNetworkTypes() => new()
        {
            new NetworkTypeDto { NetworkTypeId = 1, Name = "HMO" },
            new NetworkTypeDto { NetworkTypeId = 2, Name = "PPO" },
            new NetworkTypeDto { NetworkTypeId = 3, Name = "EPO" },
            new NetworkTypeDto { NetworkTypeId = 4, Name = "POS" },
            new NetworkTypeDto { NetworkTypeId = 5, Name = "HDHP" }
        };

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
            ViewBag.NetworkTypes = GetNetworkTypes();
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
                    ViewBag.NetworkTypes = GetNetworkTypes();
                    return View(dto);
                }

                var result = await _insuranceService.CreateAsync(dto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    ViewBag.NetworkTypes = GetNetworkTypes();
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Proveedor de seguro creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error creando el proveedor: " + ex.Message;
                ViewBag.NetworkTypes = GetNetworkTypes();
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
                City = result.Datos.City,
                State = result.Datos.State,
                Country = result.Datos.Country,
                ZipCode = result.Datos.ZipCode,
                CoverageDetails = result.Datos.CoverageDetails,
                LogoUrl = result.Datos.LogoUrl,
                IsPreferred = result.Datos.IsPreferred,
                NetworkTypeId = result.Datos.NetworkTypeId,
                CustomerSupportContact = result.Datos.CustomerSupportContact,
                AcceptedRegions = result.Datos.AcceptedRegions,
                MaxCoverageAmount = result.Datos.MaxCoverageAmount,
                IsActive = result.Datos.IsActive
            };

            ViewBag.NetworkTypes = GetNetworkTypes();
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, UpdateInsuranceProviderDto dto)
        {
            if (id != dto.InsuranceProviderId)
            {
                ViewBag.ErrorMessage = "El ID no coincide.";
                ViewBag.NetworkTypes = GetNetworkTypes();
                return View(dto);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.NetworkTypes = GetNetworkTypes();
                    return View(dto);
                }

                var result = await _insuranceService.UpdateAsync(dto);

                if (!result.Exitoso)
                {
                    ViewBag.ErrorMessage = result.Mensaje;
                    ViewBag.NetworkTypes = GetNetworkTypes();
                    return View(dto);
                }

                TempData["SuccessMessage"] = "Proveedor actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error actualizando el proveedor: " + ex.Message;
                ViewBag.NetworkTypes = GetNetworkTypes();
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Deactivate(int id)
        {
            try
            {
                var providerResult = await _insuranceService.GetByIdAsync(id);
                if (!providerResult.Exitoso || providerResult.Datos == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new UpdateInsuranceProviderDto
                {
                    InsuranceProviderId = providerResult.Datos.InsuranceProviderId,
                    Name = providerResult.Datos.Name,
                    PhoneNumber = providerResult.Datos.PhoneNumber,
                    Email = providerResult.Datos.Email,
                    Website = providerResult.Datos.Website,
                    Address = providerResult.Datos.Address,
                    City = providerResult.Datos.City,
                    State = providerResult.Datos.State,
                    Country = providerResult.Datos.Country,
                    ZipCode = providerResult.Datos.ZipCode,
                    CoverageDetails = providerResult.Datos.CoverageDetails,
                    LogoUrl = providerResult.Datos.LogoUrl,
                    IsPreferred = providerResult.Datos.IsPreferred,
                    NetworkTypeId = providerResult.Datos.NetworkTypeId,
                    CustomerSupportContact = providerResult.Datos.CustomerSupportContact,
                    AcceptedRegions = providerResult.Datos.AcceptedRegions,
                    MaxCoverageAmount = providerResult.Datos.MaxCoverageAmount,
                    IsActive = false
                };

                var result = await _insuranceService.UpdateAsync(dto);

                if (!result.Exitoso)
                    TempData["ErrorMessage"] = result.Mensaje;
                else
                    TempData["SuccessMessage"] = "Proveedor desactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error desactivando el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Activate(int id)
        {
            try
            {
                var providerResult = await _insuranceService.GetByIdAsync(id);
                if (!providerResult.Exitoso || providerResult.Datos == null)
                {
                    TempData["ErrorMessage"] = "Proveedor no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new UpdateInsuranceProviderDto
                {
                    InsuranceProviderId = providerResult.Datos.InsuranceProviderId,
                    Name = providerResult.Datos.Name,
                    PhoneNumber = providerResult.Datos.PhoneNumber,
                    Email = providerResult.Datos.Email,
                    Website = providerResult.Datos.Website,
                    Address = providerResult.Datos.Address,
                    City = providerResult.Datos.City,
                    State = providerResult.Datos.State,
                    Country = providerResult.Datos.Country,
                    ZipCode = providerResult.Datos.ZipCode,
                    CoverageDetails = providerResult.Datos.CoverageDetails,
                    LogoUrl = providerResult.Datos.LogoUrl,
                    IsPreferred = providerResult.Datos.IsPreferred,
                    NetworkTypeId = providerResult.Datos.NetworkTypeId,
                    CustomerSupportContact = providerResult.Datos.CustomerSupportContact,
                    AcceptedRegions = providerResult.Datos.AcceptedRegions,
                    MaxCoverageAmount = providerResult.Datos.MaxCoverageAmount,
                    IsActive = true
                };

                var result = await _insuranceService.UpdateAsync(dto);

                if (!result.Exitoso)
                    TempData["ErrorMessage"] = result.Mensaje;
                else
                    TempData["SuccessMessage"] = "Proveedor reactivado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error reactivando el proveedor: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}