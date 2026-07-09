using SGMC.Domain.Entities.Appointments;

namespace SGMC.Domain.Repositories.Appointments
{
    public interface IDoctorAvailabilityRepository
    {
        Task<DoctorAvailability?> GetByIdAsync(int id);
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DoctorAvailability>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<DoctorAvailability>> GetByDoctorAndDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate);

        Task<bool> IsAvailableAsync(int doctorId, DateOnly date, TimeOnly time);
        Task<bool> HasConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime);

        Task AddAsync(DoctorAvailability availability);
        Task UpdateAsync(DoctorAvailability availability);
        Task DeleteAsync(int availabilityId);
        Task<bool> ExistsAsync(int id);

        Task<bool> CheckForConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime);
        Task<bool> CheckForConflictExcludingCurrentAsync(int availabilityId, int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime);
    }
}