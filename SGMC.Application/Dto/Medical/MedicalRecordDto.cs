namespace SGMC.Application.Dto.Medical
{
    // Base con datos comunes de la historia clínica
    public record MedicalRecordBaseDto
    {
        public int PatientId { get; init; }
        public int DoctorId { get; init; }
        public string Diagnosis { get; init; } = string.Empty;
        public string Treatment { get; init; } = string.Empty;
        public string? Notes { get; init; }
    }

    // DTO principal de lectura
    public record MedicalRecordDto : MedicalRecordBaseDto
    {
        public int RecordId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string DoctorName { get; init; } = string.Empty;
        public DateTime DateOfVisit { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    // DTO de create
    public record CreateMedicalRecordDto : MedicalRecordBaseDto
    {
        public int? AppointmentId { get; init; }
        public DateTime RecordDate { get; init; }
    }

    // DTO de update
    public record UpdateMedicalRecordDto
    {
        public int RecordId { get; init; }
        public string Diagnosis { get; init; } = string.Empty;
        public string Treatment { get; init; } = string.Empty;
        public string? Notes { get; init; }
        public DateTime? RecordDate { get; init; }
    }
}