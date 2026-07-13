using SGMC.Application.Dto.Appointments;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAvailabilityService
    {
        Task<OperationResult<AvailabilityDto>> CreateAsync(CreateAvailabilityDto dto);
        Task<OperationResult<AvailabilityDto>> UpdateAsync(UpdateAvailabilityDto dto);
        Task<OperationResult> DeleteAsync(int id);

        Task<OperationResult<AvailabilityDto>> GetByIdAsync(int id);
        Task<OperationResult<List<AvailabilityDto>>> GetByDoctorIdAsync(int doctorId);
        Task<OperationResult<List<AvailabilityDto>>> GetByDoctorAndDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate);
    }
}