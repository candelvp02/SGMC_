using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models.Appointment;
using System.Security.Claims;

namespace SGMC.Web.Controllers
{
    // Vistas de gestión y visualización de citas del médico autenticado.
    [Authorize(Roles = "Médico")]
    public class DoctorAppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService? _doctorService;
        private readonly IReminderService? _reminderService;

        // Constructor completo: usado por ASP.NET Core DI en producción,
        // habilita Pending/Confirm/Reject/Reminder (necesitan la lista de
        // doctores y el servicio de recordatorios).
        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public DoctorAppointmentsController(
                    IAppointmentService appointmentService,
                    IDoctorService doctorService,
                    IReminderService reminderService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _reminderService = reminderService;
        }

        // Constructor reducido: usado por Index/Calendar/Details (y por las
        // pruebas unitarias), que no necesitan la lista de doctores ni recordatorios.
        public DoctorAppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
            _doctorService = null;
            _reminderService = null;
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

        // GET: /DoctorAppointments/Pending
        public async Task<IActionResult> Pending()
        {
            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var viewModel = new DoctorPendingAppointmentsViewModel
            {
                SelectedDoctorId = doctorId
            };

            var filter = new AppointmentFilterDto { DoctorId = doctorId.Value, StatusId = 1 };
            var result = await _appointmentService.GetFilteredAppointmentsAsync(filter);

            if (!result.Exitoso || result.Datos == null)
            {
                TempData["Error"] = result.Mensaje;
            }
            else
            {
                viewModel.PendingAppointments = result.Datos
                    .OrderBy(a => a.AppointmentDate)
                    .Select(PendingAppointmentViewModel.FromDto)
                    .ToList();
            }

            return View(viewModel);
        }

        // POST: /DoctorAppointments/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _appointmentService.ConfirmAsync(id);

            TempData[result.Exitoso ? "Success" : "Error"] = result.Mensaje;

            return RedirectToAction(nameof(Pending));
        }

        // POST: /DoctorAppointments/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _appointmentService.RejectAsync(id);

            TempData[result.Exitoso ? "Success" : "Error"] = result.Mensaje;

            return RedirectToAction(nameof(Pending));
        }

        // GET: /DoctorAppointments/Reminder/5
        public async Task<IActionResult> Reminder(int id)
        {
            if (_reminderService is null)
            {
                TempData["Error"] = "El servicio de recordatorios no está disponible.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var appointmentResult = await _appointmentService.GetByIdAsync(id);
            if (!appointmentResult.Exitoso || appointmentResult.Datos == null || appointmentResult.Datos.DoctorId != doctorId.Value)
            {
                TempData["Error"] = "Cita no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var detailsVm = AppointmentDetailsViewModel.FromDto(appointmentResult.Datos);
            var existingReminders = await _reminderService.GetByAppointmentIdAsync(id);

            var viewModel = new ScheduleReminderViewModel
            {
                AppointmentId = id,
                PatientName = detailsVm.PatientName,
                AppointmentDateFormatted = detailsVm.AppointmentDateFormatted,
                AppointmentDate = appointmentResult.Datos.AppointmentDate,
                Templates = _reminderService.GetTemplates(),
                ExistingReminders = existingReminders.Datos ?? new()
            };

            return View(viewModel);
        }

        // POST: /DoctorAppointments/Reminder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reminder(ScheduleReminderViewModel model)
        {
            if (_reminderService is null)
            {
                TempData["Error"] = "El servicio de recordatorios no está disponible.";
                return RedirectToAction(nameof(Details), new { id = model.AppointmentId });
            }

            var doctorId = GetCurrentDoctorId();
            if (doctorId == null)
                return RedirectToAction("Login", "Account");

            var dto = new ScheduleReminderDto
            {
                AppointmentId = model.AppointmentId,
                TemplateId = model.SelectedTemplateId,
                CustomMessage = model.CustomMessage,
                ScheduledAt = model.ScheduledAt
            };

            var result = await _reminderService.ScheduleAsync(dto, doctorId.Value);

            TempData[result.Exitoso ? "Success" : "Error"] = result.Mensaje;

            return RedirectToAction(nameof(Details), new { id = model.AppointmentId });
        }

        private int? GetCurrentDoctorId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<List<DoctorSelectViewModel>> GetDoctorsList()
        {
            if (_doctorService == null)
                return new List<DoctorSelectViewModel>();

            var result = await _doctorService.GetActiveDoctorsAsync();

            if (!result.Exitoso || result.Datos == null)
                return new List<DoctorSelectViewModel>();

            return result.Datos.Select(d => new DoctorSelectViewModel
            {
                DoctorId = d.DoctorId,
                FullName = d.FullName,
                SpecialtyName = d.SpecialtyName
            }).ToList();
        }
    }
}
