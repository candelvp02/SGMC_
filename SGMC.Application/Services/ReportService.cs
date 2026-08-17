using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.Appointments;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;
using System.Text;

namespace SGMC.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IAppointmentRepository appointmentRepository,
            IUserRepository userRepository,
            ILogger<ReportService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<OperationResult<List<AppointmentResultDto>>> GetFilteredAppointmentsAsync(ReportFilterDto filter)
        {
            var result = new OperationResult<List<AppointmentResultDto>>();

            // validaciones de campo fuera de trycatch
            if (filter is null) return OperationResult<List<AppointmentResultDto>>.Fallo("El filtro es requerido.");
            var validation = filter.IsValidDto();
            if (!validation.Exitoso)
                return OperationResult<List<AppointmentResultDto>>.Fallo(validation.Mensaje);

            try
            {
                _logger.LogInformation("Obteniendo lista de citas filtradas...");

                // obtener datos filtrados
                var appointmentsList = (await GetFilteredAppointmentsInternalAsync(filter)).ToList();

                _logger.LogInformation($"Citas encontradas para la lista: {appointmentsList.Count()}");

                // map to dto
                var dtoList = appointmentsList.Select(apt => new AppointmentResultDto
                {
                    AppointmentId = apt.AppointmentId,
                    AppointmentDate = apt.AppointmentDate,
                    PatientName = apt.Patient?.PatientNavigation != null
                                    ? $"{apt.Patient.PatientNavigation.FirstName} {apt.Patient.PatientNavigation.LastName}"
                                    : "N/A",
                    DoctorName = apt.Doctor?.DoctorNavigation != null
                                    ? $"Dr. {apt.Doctor.DoctorNavigation.FirstName} {apt.Doctor.DoctorNavigation.LastName}"
                                    : "N/A",
                    StatusName = apt.Status?.StatusName ?? "N/A"
                }).ToList();

                result.Datos = dtoList;
                result.Exitoso = true;
                result.Mensaje = "Citas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo lista de citas filtradas");
                result.Exitoso = false;
                result.Mensaje = "Error al obtener lista de citas";
            }
            return result;
        }

        public async Task<OperationResult<byte[]>> GenerateAppointmentsReportAsync(ReportFilterDto filter)
        {
            var result = new OperationResult<byte[]>();

            // validaciones de campo fuera de trycatch
            if (filter is null) return OperationResult<byte[]>.Fallo("El filtro es requerido.");
            var validation = filter.IsValidDto();
            if (!validation.Exitoso)
                return OperationResult<byte[]>.Fallo(validation.Mensaje);

            try
            {
                _logger.LogInformation("Generando reporte PDF...");

                // obtener datos filtrados
                var appointmentsList = (await GetFilteredAppointmentsInternalAsync(filter)).ToList();

                _logger.LogInformation($"Citas despues de filtros: {appointmentsList.Count()}");

                // generar PDF
                var pdfBytes = GeneratePdfReport(appointmentsList, filter);

                result.Datos = pdfBytes;
                result.Exitoso = true;
                result.Mensaje = "Reporte generado correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte de citas");
                result.Exitoso = false;
                result.Mensaje = "Error al generar reporte";
            }
            return result;
        }

        public async Task<OperationResult<byte[]>> GenerateExcelAppointmentsReportAsync(ReportFilterDto filter)
        {
            var result = new OperationResult<byte[]>();

            // validaciones de campo fuera de trycatch
            if (filter is null) return OperationResult<byte[]>.Fallo("El filtro es requerido.");
            var validation = filter.IsValidDto();
            if (!validation.Exitoso)
                return OperationResult<byte[]>.Fallo(validation.Mensaje);

            try
            {
                _logger.LogInformation("Generando reporte Excel...");

                // obtener datos filtrados
                var appointmentsList = (await GetFilteredAppointmentsInternalAsync(filter)).ToList();

                _logger.LogInformation($"Citas despues de filtros: {appointmentsList.Count()}");

                //generar Excel
                var excelBytes = GenerateExcelReport(appointmentsList, filter);

                result.Datos = excelBytes;
                result.Exitoso = true;
                result.Mensaje = "Reporte Excel generado correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando reporte Excel");
                result.Exitoso = false;
                result.Mensaje = "Error al generar reporte Excel";
            }
            return result;
        }

        public async Task<OperationResult<AppointmentStatisticsDto>> GetAppointmentStatisticsAsync(ReportFilterDto filter)
        {
            var result = new OperationResult<AppointmentStatisticsDto>();

            // validaciones de campo fuera de trycatch
            if (filter is null) return OperationResult<AppointmentStatisticsDto>.Fallo("El filtro es requerido.");
            var validation = filter.IsValidDto();
            if (!validation.Exitoso)
                return OperationResult<AppointmentStatisticsDto>.Fallo(validation.Mensaje);

            try
            {
                // obtener datos filtrados
                var appointmentsList = (await GetFilteredAppointmentsInternalAsync(filter)).ToList();

                _logger.LogInformation($"Total de citas obtenidas para estadisticas: {appointmentsList.Count()}");

                // calcular estadisticas
                var stats = new AppointmentStatisticsDto
                {
                    TotalAppointments = appointmentsList.Count,
                    ConfirmedAppointments = appointmentsList.Count(a => a.StatusId == 2),
                    CancelledAppointments = appointmentsList.Count(a => a.StatusId == 3),
                    PendingAppointments = appointmentsList.Count(a => a.StatusId == 1),
                    CompletedAppointments = appointmentsList.Count(a => a.StatusId == 4),
                    RejectedAppointments = appointmentsList.Count(a => a.StatusId == 5),
                    StartDate = filter.StartDate ?? DateTime.Now.AddMonths(-1),
                    EndDate = filter.EndDate ?? DateTime.Now
                };

                _logger.LogInformation($"Estadisticas: Total={stats.TotalAppointments}, Confirmadas={stats.ConfirmedAppointments}, Pendientes={stats.PendingAppointments}, Canceladas={stats.CancelledAppointments}");

                // calcular ratios
                stats.CancellationRate = stats.TotalAppointments > 0 ?
                    (decimal)stats.CancelledAppointments / stats.TotalAppointments * 100 : 0;

                stats.ConfirmationRate = stats.TotalAppointments > 0 ?
                    (decimal)stats.ConfirmedAppointments / stats.TotalAppointments * 100 : 0;

                result.Datos = stats;
                result.Exitoso = true;
                result.Mensaje = "Estadisticas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadisticas");
                result.Exitoso = false;
                result.Mensaje = "Error al obtener estadisticas";
            }
            return result;
        }

        private async Task<IEnumerable<Appointment>> GetFilteredAppointmentsInternalAsync(ReportFilterDto filter)
        {
            _logger.LogInformation($"Obteniendo citas base desde {filter.StartDate} hasta {filter.EndDate}");

            var startDate = filter.StartDate ?? DateTime.Now.AddMonths(-1);
            var endDate = filter.EndDate ?? DateTime.Now;

            // obtener datos base del repositorio
            var appointments = await _appointmentRepository.GetByDateRangeAsync(startDate, endDate);

            _logger.LogInformation($"Citas base encontradas: {appointments.Count()}");

            // aplicar filtros en memoria
            if (filter.DoctorId.HasValue)
            {
                appointments = appointments.Where(a => a.DoctorId == filter.DoctorId.Value);
            }

            if (filter.StatusId.HasValue)
            {
                appointments = appointments.Where(a => a.StatusId == filter.StatusId.Value);
            }

            if (filter.PatientId.HasValue)
            {
                appointments = appointments.Where(a => a.PatientId == filter.PatientId.Value);
            }

            return appointments;
        }

        // metodos de generacion de Reportes
        private byte[] GeneratePdfReport(List<Appointment> appointments, ReportFilterDto filter)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='UTF-8'>");
            sb.AppendLine("<title>Reporte de Citas</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            sb.AppendLine("h1 { color: #333; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            sb.AppendLine("th { background-color: #4CAF50; color: white; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine(".header { margin-bottom: 30px; }");
            sb.AppendLine(".info { color: #666; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='header'>");
            sb.AppendLine("<h1>Reporte de Citas Medicas</h1>");
            sb.AppendLine($"<p class='info'>Fecha de generacion: {DateTime.Now:dd/MM/yyyy HH:mm}</p>");
            sb.AppendLine($"<p class='info'>Periodo: {filter.StartDate:dd/MM/yyyy} - {filter.EndDate:dd/MM/yyyy}</p>");
            sb.AppendLine($"<p class='info'>Total de citas: {appointments.Count}</p>");
            sb.AppendLine("</div>");

            if (appointments.Any())
            {
                sb.AppendLine("<table>");
                sb.AppendLine("<tr>");
                sb.AppendLine("<th>ID</th>");
                sb.AppendLine("<th>Paciente</th>");
                sb.AppendLine("<th>Doctor</th>");
                sb.AppendLine("<th>Fecha</th>");
                sb.AppendLine("<th>Estado</th>");
                sb.AppendLine("</tr>");

                foreach (var apt in appointments)
                {
                    string patientName = apt.Patient?.PatientNavigation != null
                        ? $"{apt.Patient.PatientNavigation.FirstName} {apt.Patient.PatientNavigation.LastName}"
                        : "N/A";

                    string doctorName = apt.Doctor?.DoctorNavigation != null
                        ? $"Dr. {apt.Doctor.DoctorNavigation.FirstName} {apt.Doctor.DoctorNavigation.LastName}"
                        : "N/A";

                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td>{apt.AppointmentId}</td>");
                    sb.AppendLine($"<td>{patientName}</td>");
                    sb.AppendLine($"<td>{doctorName}</td>");
                    sb.AppendLine($"<td>{apt.AppointmentDate:dd/MM/yyyy HH:mm}</td>");
                    sb.AppendLine($"<td>{apt.Status?.StatusName ?? "N/A"}</td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</table>");
            }
            else
            {
                sb.AppendLine("<p>No se encontraron citas en el periodo seleccionado.</p>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private byte[] GenerateExcelReport(List<Appointment> appointments, ReportFilterDto filter)
        {
            var sb = new StringBuilder();

            // Encabezados
            sb.AppendLine("ID,Paciente,Doctor,Fecha,Hora,Estado");

            // Datos
            foreach (var apt in appointments)
            {
                string patientName = apt.Patient?.PatientNavigation != null
                    ? $"{apt.Patient.PatientNavigation.FirstName} {apt.Patient.PatientNavigation.LastName}"
                    : "N/A";

                string doctorName = apt.Doctor?.DoctorNavigation != null
                    ? $"Dr. {apt.Doctor.DoctorNavigation.FirstName} {apt.Doctor.DoctorNavigation.LastName}"
                    : "N/A";

                sb.AppendLine($"{apt.AppointmentId}," +
                              $"\"{patientName}\"," +
                              $"\"{doctorName}\"," +
                              $"{apt.AppointmentDate:dd/MM/yyyy}," +
                              $"{apt.AppointmentDate:HH:mm}," +
                              $"{apt.Status?.StatusName ?? "N/A"}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}