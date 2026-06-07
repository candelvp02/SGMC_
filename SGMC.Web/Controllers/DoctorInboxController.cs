using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Médico")]
    public class DoctorInboxController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly ISpecialtyService _specialtyService;

        public DoctorInboxController(
            IAppointmentService appointmentService,
            IDoctorService doctorService,
            ISpecialtyService specialtyService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _specialtyService = specialtyService;
        }

        // GET: DoctorInbox/Index
        public async Task<IActionResult> Index()
        {
            var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                ViewBag.ErrorMessage = "No se pudo identificar al médico autenticado.";
                return View(new List<SGMC.Application.Dto.Appointments.AppointmentDto>());
            }

            // Verificar si la especialidad está activa
            var doctorResult = await _doctorService.GetByIdAsync(doctorId);
            if (doctorResult.Exitoso && doctorResult.Datos != null)
            {
                var specialtyResult = await _specialtyService.GetByIdAsync(doctorResult.Datos.SpecialtyId);
                if (specialtyResult.Exitoso && specialtyResult.Datos != null && !specialtyResult.Datos.IsActive)
                {
                    TempData["WarningMessage"] = "Tu especialidad fue desactivada por el administrador. Por favor selecciona una nueva.";
                }
            }

            var result = await _appointmentService.GetByDoctorIdAsync(doctorId);

            if (!result.Exitoso || result.Datos == null)
            {
                ViewBag.ErrorMessage = result.Mensaje ?? "Error al obtener las citas.";
                return View(new List<SGMC.Application.Dto.Appointments.AppointmentDto>());
            }

            var pendientes = result.Datos
                .Where(a => a.StatusId == 1)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            return View(pendientes);
        }

        // POST: DoctorInbox/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _appointmentService.ConfirmAsync(id);

            if (!result.Exitoso)
                TempData["ErrorMessage"] = result.Mensaje ?? "No se pudo confirmar la cita.";
            else
                TempData["SuccessMessage"] = "Cita confirmada correctamente. El paciente será notificado.";

            return RedirectToAction(nameof(Index));
        }

        // POST: DoctorInbox/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _appointmentService.CancelAsync(id);

            if (!result.Exitoso)
                TempData["ErrorMessage"] = result.Mensaje ?? "No se pudo rechazar la cita.";
            else
                TempData["SuccessMessage"] = "Cita rechazada. El horario quedó liberado y el paciente será notificado.";

            return RedirectToAction(nameof(Index));
        }

        // GET: DoctorInbox/Specialty
        public async Task<IActionResult> Specialty()
        {
            var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al médico autenticado.";
                return RedirectToAction(nameof(Index));
            }

            var doctorResult = await _doctorService.GetByIdAsync(doctorId);
            if (!doctorResult.Exitoso || doctorResult.Datos == null)
            {
                TempData["ErrorMessage"] = "No se pudo obtener el perfil del médico.";
                return RedirectToAction(nameof(Index));
            }

            var specialtiesResult = await _specialtyService.GetActiveAsync();
            ViewBag.Specialties = specialtiesResult.Exitoso ? specialtiesResult.Datos : new List<SGMC.Application.Dto.Medical.SpecialtyDto>();
            ViewBag.CurrentSpecialtyId = doctorResult.Datos.SpecialtyId;
            ViewBag.CurrentSpecialtyName = doctorResult.Datos.SpecialtyName;

            return View();
        }

        // POST: DoctorInbox/Specialty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Specialty(short specialtyId)
        {
            var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al médico autenticado.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _doctorService.AssignSpecialtyAsync(doctorId, specialtyId);

            if (!result.Exitoso)
                TempData["ErrorMessage"] = result.Mensaje;
            else
                TempData["SuccessMessage"] = "Especialidad actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}