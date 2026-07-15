namespace SGMC.Application.Dto.Appointments
{
    // Base con la info common de la cita
    public record AppointmentBaseDto
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int StatusId { get; set; }
    }

    // DTO de creacion
    public record CreateAppointmentDto : AppointmentBaseDto
    {
        public string? Notes { get; set; }
    }

    // DTO principal de lectura
    public record AppointmentDto : AppointmentBaseDto
    {
        public int AppointmentId { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // DTO de update
    public record UpdateAppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int StatusId { get; set; }
        public string? Notes { get; set; }
        public int Id { get; set; }
    }

    // Statistics dto
    public record AppointmentStatisticsDto
    {
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }

        public decimal CancellationRate { get; set; }
        public decimal ConfirmationRate { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Summary dto
    public record AppointmentSummaryDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;
        public int StatusId { get; set; }
    }

    // Report filter dto
    public record ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? StatusId { get; set; }
        public short? SpecialtyId { get; set; }
    }

    // Result dto
    public record AppointmentResultDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string? PatientName { get; set; } = string.Empty;
        public string? DoctorName { get; set; } = string.Empty;
        public string? StatusName { get; set; } = string.Empty;
    }

    // Filter dto
    public record AppointmentFilterDto
    {
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int? StatusId { get; set; }
    }

    // DTO de reprogramación — PBI-34
    public record RescheduleAppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime NewAppointmentDate { get; set; }
    }
}
