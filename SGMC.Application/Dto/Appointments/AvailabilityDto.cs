namespace SGMC.Application.Dto.Appointments
{
    public class AvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly AvailableDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public short? AvailabilityModeId { get; set; }
        public string? AvailabilityModeName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAvailabilityDto
    {
        public int DoctorId { get; set; }
        public DateOnly AvailableDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public short? AvailabilityModeId { get; set; }
    }

    public class UpdateAvailabilityDto
    {
        public int AvailabilityId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly AvailableDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public short? AvailabilityModeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class AvailabilityModeDto
    {
        public short AvailabilityModeId { get; set; }
        public string AvailabilityMode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}