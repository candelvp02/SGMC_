using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Web.Services;

namespace SGMC.Web.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorApiClient _doctorApiClient;

        public DoctorController(IDoctorApiClient doctorApiClient)
        {
            _doctorApiClient = doctorApiClient;
        }

        // GET: Doctor
        public async Task<ActionResult> Index()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            var apiResult = await _doctorApiClient.GetAllAsync();

            if (!apiResult.Success || apiResult.Data == null)
            {
                ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "Error al obtener los doctores desde la API.";
                return View(new List<DoctorDto>());
            }

            return View(apiResult.Data);
        }

        // GET: Doctor/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var apiResult = await _doctorApiClient.GetByIdAsync(id);

            if (!apiResult.Success || apiResult.Data == null)
            {
                ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "No se pudo obtener el doctor desde la API.";
                return View();
            }

            return View(apiResult.Data);
        }

        // GET: Doctor/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RegisterDoctorDto registerDoctorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(registerDoctorDto);
                }

                var apiResult = await _doctorApiClient.CreateAsync(registerDoctorDto);

                if (!apiResult.Success)
                {
                    ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "Error al crear el doctor en la API.";
                    return View(registerDoctorDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al crear doctor: {ex.Message}";
                return View(registerDoctorDto);
            }
        }

        // GET: Doctor/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var apiResult = await _doctorApiClient.GetByIdAsync(id);

            if (!apiResult.Success || apiResult.Data == null)
            {
                ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "No se pudo obtener el doctor desde la API.";
                return View();
            }

            var dto = apiResult.Data;

            var updateDto = new UpdateDoctorDto
            {
                DoctorId = dto.DoctorId,
                SpecialtyId = dto.SpecialtyId,
                PhoneNumber = dto.PhoneNumber,
                YearsOfExperience = dto.YearsOfExperience,
                Education = dto.Education,
                Bio = dto.Bio,
                ConsultationFee = dto.ConsultationFee,
                ClinicAddress = dto.ClinicAddress,
                LicenseExpirationDate = dto.LicenseExpirationDate,
                IsActive = dto.IsActive,
            };

            return View(updateDto);
        }

        // POST: Doctor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateDoctorDto updateDoctorDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(updateDoctorDto);
                }

                var apiResult = await _doctorApiClient.UpdateAsync(updateDoctorDto);

                if (!apiResult.Success)
                {
                    ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "Error al actualizar el doctor en la API.";
                    return View(updateDoctorDto);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error al actualizar doctor: {ex.Message}";
                return View(updateDoctorDto);
            }
        }

        // GET: Doctor/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var apiResult = await _doctorApiClient.GetByIdAsync(id);

            if (!apiResult.Success || apiResult.Data == null)
            {
                ViewBag.ErrorMessage = apiResult.ErrorMessage ?? "No se pudo obtener el doctor desde la API.";
                return RedirectToAction(nameof(Index));
            }

            return View(apiResult.Data);
        }

        // POST: Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var apiResult = await _doctorApiClient.DeleteAsync(id);

                if (!apiResult.Success)
                {
                    TempData["ErrorMessage"] = apiResult.ErrorMessage ?? "No se pudo eliminar el doctor en la API.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al eliminar doctor: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
