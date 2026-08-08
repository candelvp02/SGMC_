using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models.Appointment;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    [Authorize(Roles = "Paciente")]
    public class PatientAppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;

        public PatientAppointmentsController(
            IAppointmentService appointmentService,
            IPatientService patientService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
        }

        // GET: /PatientAppointments
        public async Task<IActionResult> Index(int? statusId, DateTime? from, DateTime? to)
        {
            var patientId = GetCurrentPatientId();
            if (patientId == null)
                return RedirectToAction("Login", "Account");

            var result = await _appointmentService.GetByPatientIdAsync(patientId.Value);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["Error"] = result.Mensaje;
                ViewBag.Appointments = new List<AppointmentListViewModel>();
                ViewBag.StatusId = statusId;
                ViewBag.From = from?.ToString("yyyy-MM-dd");
                ViewBag.To = to?.ToString("yyyy-MM-dd");
                return View();
            }

            var appointments = result.Datos.AsEnumerable();

            if (statusId.HasValue && statusId.Value > 0)
                appointments = appointments.Where(a => a.StatusId == statusId.Value);

            if (from.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate >= from.Value);

            if (to.HasValue)
                appointments = appointments.Where(a => a.AppointmentDate <= to.Value.AddDays(1).AddSeconds(-1));

            ViewBag.Appointments = appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Select(AppointmentListViewModel.FromDto)
                .ToList();

            ViewBag.StatusId = statusId;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");

            return View();
        }

        // GET: /PatientAppointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index));

            var patientId = GetCurrentPatientId();
            if (patientId == null)
                return RedirectToAction("Login", "Account");

            var result = await _appointmentService.GetByIdAsync(id);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la cita pertenezca al paciente autenticado
            if (result.Datos.PatientId != patientId.Value)
            {
                TempData["Error"] = "No tienes permiso para ver esta cita.";
                return RedirectToAction(nameof(Index));
            }

            return View(AppointmentDetailsViewModel.FromDto(result.Datos));
        }

        // GET: /PatientAppointments/Reschedule/5
        public async Task<IActionResult> Reschedule(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index));

            var patientId = GetCurrentPatientId();
            if (patientId == null)
                return RedirectToAction("Login", "Account");

            var result = await _appointmentService.GetByIdAsync(id);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la cita pertenezca al paciente autenticado
            if (result.Datos.PatientId != patientId.Value)
            {
                TempData["Error"] = "No tienes permiso para reprogramar esta cita.";
                return RedirectToAction(nameof(Index));
            }

            // Solo se pueden reprogramar citas Pendiente (1) o Confirmada (2)
            if (result.Datos.StatusId != 1 && result.Datos.StatusId != 2)
            {
                TempData["Error"] = "Solo puedes reprogramar citas en estado Pendiente o Confirmada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(RescheduleAppointmentViewModel.FromDto(result.Datos));
        }
        // POST: /PatientAppointments/CancelConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var patientId = GetCurrentPatientId();
            if (patientId == null)
                return RedirectToAction("Login", "Account");

            var appointmentResult = await _appointmentService.GetByIdAsync(id);

            if (!appointmentResult.Exitoso || appointmentResult.Datos == null)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (appointmentResult.Datos.PatientId != patientId.Value)
            {
                TempData["Error"] = "No tienes permiso para cancelar esta cita.";
                return RedirectToAction(nameof(Index));
            }

            if (appointmentResult.Datos.StatusId != 1 && appointmentResult.Datos.StatusId != 2)
            {
                TempData["Error"] = "Solo puedes cancelar citas en estado Pendiente o Confirmada.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var result = await _appointmentService.CancelAsync(id);

            if (!result.Exitoso)
            {
                TempData["Error"] = result.Mensaje;
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Success"] = "Tu cita fue cancelada correctamente. El médico ha sido notificado por correo.";
            return RedirectToAction(nameof(Index));
        }
        private int? GetCurrentPatientId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}