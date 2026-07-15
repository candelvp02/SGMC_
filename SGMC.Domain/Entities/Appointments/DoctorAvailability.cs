using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.Users;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMC.Domain.Entities.Appointments
{
    [Table("DoctorAvailability", Schema = "appointments")]
    public partial class DoctorAvailability
    {
        public int AvailabilityId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly AvailableDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public short? AvailabilityModeId { get; set; }
        public virtual Doctor? Doctor { get; set; }
        public virtual AvailabilityMode? AvailabilityMode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}