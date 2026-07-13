using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models.Appointment;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    // Vista de visualización de citas del médico autenticado.
    [Authorize(Roles = "Médico")]
    public class DoctorAppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public DoctorAppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: /DoctorAppointments
        public async Task<IActionResult> Index(int? statusId, DateTime? from, DateTime? to)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var result = await _appointmentService.GetByDoctorIdAsync(doctorId.Value);

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

        // GET: /DoctorAppointments/Calendar?month=2026-07
        public async Task<IActionResult> Calendar(int? year, int? month)
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var displayedMonth = new DateTime(year ?? today.Year, month ?? today.Month, 1);

            var result = await _appointmentService.GetByDoctorIdAsync(doctorId.Value);

            var appointmentsInMonth = new List<AppointmentListViewModel>();
            if (result.Exitoso && result.Datos != null)
            {
                appointmentsInMonth = result.Datos
                    .Where(a => a.AppointmentDate.Year == displayedMonth.Year
                             && a.AppointmentDate.Month == displayedMonth.Month)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(AppointmentListViewModel.FromDto)
                    .ToList();
            }
            else
            {
                TempData["Error"] = result.Mensaje;
            }

            var viewModel = new DoctorCalendarViewModel
            {
                DisplayedMonth = displayedMonth,
                AppointmentsByDay = appointmentsInMonth
                    .GroupBy(a => a.AppointmentDate.Date)
                    .ToDictionary(g => g.Key, g => g.ToList())
            };

            return View(viewModel);
        }

        // GET: /DoctorAppointments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return RedirectToAction(nameof(Index));

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var result = await _appointmentService.GetByIdAsync(id);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar que la cita pertenezca al médico autenticado
            if (result.Datos.DoctorId != doctorId.Value)
            {
                TempData["Error"] = "No tienes permiso para ver esta cita.";
                return RedirectToAction(nameof(Index));
            }

            return View(AppointmentDetailsViewModel.FromDto(result.Datos));
        }

        private int? GetCurrentDoctorId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
