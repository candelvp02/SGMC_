using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Médico")]
    public class DoctorInboxController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public DoctorInboxController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: DoctorInbox/Index
        public async Task<IActionResult> Index()
        {
            // Obtener el DoctorId del claim del usuario autenticado
            var doctorIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(doctorIdClaim, out int doctorId))
            {
                ViewBag.ErrorMessage = "No se pudo identificar al médico autenticado.";
                return View(new List<SGMC.Application.Dto.Appointments.AppointmentDto>());
            }

            var result = await _appointmentService.GetByDoctorIdAsync(doctorId);

            if (!result.Exitoso || result.Datos == null)
            {
                ViewBag.ErrorMessage = result.Mensaje ?? "Error al obtener las citas.";
                return View(new List<SGMC.Application.Dto.Appointments.AppointmentDto>());
            }

            // Solo mostrar las pendientes (StatusId = 1)
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
    }
}