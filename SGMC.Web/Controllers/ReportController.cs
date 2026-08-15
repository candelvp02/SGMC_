using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models;
using SGMC.Web.Models.Appointment;
using SGMC.Web.Models.Report;

namespace SGMC.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public ReportController(
            IReportService reportService,
            IPatientService patientService,
            IDoctorService doctorService)
        {
            _reportService = reportService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        // GET: Report
        // Acepta los filtros como parámetros
        [HttpGet]
        public async Task<ActionResult> Index([FromQuery] ReportFilterDto filter)
        {
            if (filter.StartDate == null)
                filter.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            if (filter.EndDate == null)
                filter.EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            // 1. Obtener las estadísticas
            var statsResult = await _reportService.GetAppointmentStatisticsAsync(filter);

            // Obtener la lista de citas filtradas
            var appointmentsResult = await _reportService.GetFilteredAppointmentsAsync(filter);

            var appointmentList = new List<AppointmentResultViewModel>();
            if (appointmentsResult.Exitoso && appointmentsResult.Datos != null)
            {
                // Mapeamos del DTO del servicio a nuestro ViewModel
                appointmentList = appointmentsResult.Datos.Select(dto => new AppointmentResultViewModel
                {
                    AppointmentId = dto.AppointmentId,
                    AppointmentDate = dto.AppointmentDate,
                    PatientName = dto.PatientName,
                    DoctorName = dto.DoctorName,
                    StatusName = dto.StatusName,
                    StatusColor = GetStatusColor(dto.StatusName)
                }).ToList();
            }

            // 3. Cargar el ViewModel
            var viewModel = new ReportViewModel
            {
                Filter = filter,
                Statistics = statsResult.Exitoso ? statsResult.Datos : new AppointmentStatisticsDto(),
                Patients = await GetPatientsList(),
                Doctors = await GetDoctorsList(),
                Statuses = GetStatusesList(),
                FilteredAppointments = appointmentList
            };

            if (!statsResult.Exitoso || !appointmentsResult.Exitoso)
            {
                ViewBag.ErrorMessage = statsResult.Mensaje + " " + appointmentsResult.Mensaje;
            }

            return View(viewModel);
        }

        // POST: Report/GeneratePdfReport
        [HttpPost]
        public async Task<IActionResult> GeneratePdfReport(ReportFilterDto filter)
        {
            var result = await _reportService.GenerateAppointmentsReportAsync(filter);
            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return RedirectToAction(nameof(Index), filter);
            }

            return File(result.Datos!, "text/html", $"Reporte_Citas_{DateTime.Now:yyyyMMdd}.html");
        }

        // POST: Report/GenerateExcelReport
        [HttpPost]
        public async Task<IActionResult> GenerateExcelReport(ReportFilterDto filter)
        {
            var result = await _reportService.GenerateExcelAppointmentsReportAsync(filter);
            if (!result.Exitoso)
            {
                TempData["ErrorMessage"] = result.Mensaje;
                return RedirectToAction(nameof(Index), filter);
            }

            return File(result.Datos!, "text/csv", $"Reporte_Citas_{DateTime.Now:yyyyMMdd}.csv");
        }


        // METODOS PRIVADOS

        private async Task<List<PatientSelectViewModel>> GetPatientsList()
        {
            var result = await _patientService.GetActiveAsync();
            if (!result.Exitoso || result.Datos == null)
                return new List<PatientSelectViewModel>();
            return result.Datos.Select(p => new PatientSelectViewModel
            {
                PatientId = p.PatientId,
                FullName = p.FullName,
                IdentificationNumber = p.IdentificationNumber
            }).ToList();
        }

        private async Task<List<DoctorSelectViewModel>> GetDoctorsList()
        {
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

        private List<StatusSelectViewModel> GetStatusesList()
        {
            return new List<StatusSelectViewModel>
            {
                new StatusSelectViewModel { StatusId = 1, StatusName = "Pendiente" },
                new StatusSelectViewModel { StatusId = 2, StatusName = "Confirmada" },
                new StatusSelectViewModel { StatusId = 3, StatusName = "Cancelada" },
                new StatusSelectViewModel { StatusId = 4, StatusName = "Completada" },
                new StatusSelectViewModel { StatusId = 5, StatusName = "Rechazada" }
            };
        }
        private string GetStatusColor(string? statusName)
        {
            // Misma paleta usada en el resto de la app (ver _StatusBadge.cshtml
            // y AppointmentViewModel.StatusBadgeClass) para que los colores
            // signifiquen lo mismo en todas las pantallas.
            return statusName switch
            {
                "Pendiente" => "warning",   // amarillo
                "Confirmada" => "success",  // verde
                "Completada" => "info",     // celeste
                "Cancelada" => "danger",    // rojo
                "Rechazada" => "secondary", // gris
                _ => "secondary"
            };
        }
    }
}