using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Services;

namespace SGMC.Web.Controllers
{
    public class SpecialtyController : Controller
    {
        private readonly ISpecialtyService _specialtyService;
        private readonly IDoctorApiClient _doctorApiClient;

        public SpecialtyController(ISpecialtyService specialtyService, IDoctorApiClient doctorApiClient)
        {
            _specialtyService = specialtyService;
            _doctorApiClient = doctorApiClient;
        }

        // GET: Specialty
        // Lista de especialidades activas, ordenadas alfabéticamente, para que
        // el paciente identifique qué tipo de médico necesita.
        public async Task<ActionResult> Index()
        {
            var result = await _specialtyService.GetActiveAsync();

            if (!result.Exitoso || result.Datos == null)
            {
                ViewBag.ErrorMessage = result.Mensaje ?? "No se pudieron obtener las especialidades.";
                return View(new List<SGMC.Application.Dto.Medical.SpecialtyDto>());
            }

            // El repositorio ya entrega el listado ordenado alfabéticamente,
            // pero se refuerza aquí por si la fuente de datos cambia.
            var specialties = result.Datos
                .OrderBy(s => s.SpecialtyName)
                .ToList();

            return View(specialties);
        }

        // GET: Specialty/Doctors/5
        // Al seleccionar una especialidad, se muestran los médicos disponibles
        // (activos) para esa especialidad.
        public async Task<ActionResult> Doctors(short id)
        {
            var specialtyResult = await _specialtyService.GetByIdAsync(id);

            if (!specialtyResult.Exitoso || specialtyResult.Datos == null || !specialtyResult.Datos.IsActive)
            {
                TempData["ErrorMessage"] = "La especialidad seleccionada no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SpecialtyName = specialtyResult.Datos.SpecialtyName;
            ViewBag.SpecialtyId = specialtyResult.Datos.SpecialtyId;

            var doctorsResult = await _doctorApiClient.GetBySpecialtyAsync(id);

            if (!doctorsResult.Success || doctorsResult.Data == null)
            {
                ViewBag.ErrorMessage = doctorsResult.ErrorMessage ?? "No se pudieron obtener los médicos de esta especialidad.";
                return View(new List<SGMC.Application.Dto.Users.DoctorDto>());
            }

            var activeDoctors = doctorsResult.Data
                .Where(d => d.IsActive)
                .OrderBy(d => d.FullName)
                .ToList();

            return View(activeDoctors);
        }
    }
}