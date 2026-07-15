using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Web.Models.Appointment;

namespace SGMC.Web.Controllers
{
    public class DoctorAppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;

        public DoctorAppointmentsController(
            IAppointmentService appointmentService,
            IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
        }

        // GET: /DoctorAppointments/Pending?doctorId=5
        public async Task<IActionResult> Pending(int? doctorId)
        {
            var viewModel = new DoctorPendingAppointmentsViewModel
            {
                SelectedDoctorId = doctorId,
                Doctors = await GetDoctorsList()
            };

            if (doctorId.HasValue && doctorId.Value > 0)
            {
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
            }

            return View(viewModel);
        }

        // POST: /DoctorAppointments/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, int doctorId)
        {
            var result = await _appointmentService.ConfirmAsync(id);

            TempData[result.Exitoso ? "Success" : "Error"] = result.Mensaje;

            return RedirectToAction(nameof(Pending), new { doctorId });
        }

        // POST: /DoctorAppointments/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, int doctorId)
        {
            var result = await _appointmentService.RejectAsync(id);

            TempData[result.Exitoso ? "Success" : "Error"] = result.Mensaje;

            return RedirectToAction(nameof(Pending), new { doctorId });
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
    }
}