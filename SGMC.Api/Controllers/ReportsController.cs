using Microsoft.AspNetCore.Mvc;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;

namespace SGMC.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("appointments")]
        public async Task<IActionResult> GetFilteredAppointments([FromQuery] ReportFilterDto filter)
        {
            var result = await _reportService.GetFilteredAppointmentsAsync(filter);
            if (!result.Exitoso) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("appointments/statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] ReportFilterDto filter)
        {
            var result = await _reportService.GetAppointmentStatisticsAsync(filter);
            if (!result.Exitoso) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("appointments/export/html")]
        public async Task<IActionResult> ExportHtml([FromQuery] ReportFilterDto filter)
        {
            var result = await _reportService.GenerateAppointmentsReportAsync(filter);
            if (!result.Exitoso || result.Datos is null) return BadRequest(result);
            return File(result.Datos, "text/html", "reporte-citas.html");
        }

        [HttpGet("appointments/export/excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] ReportFilterDto filter)
        {
            var result = await _reportService.GenerateExcelAppointmentsReportAsync(filter);
            if (!result.Exitoso || result.Datos is null) return BadRequest(result);
            return File(result.Datos, "text/csv", "reporte-citas.csv");
        }
    }
}