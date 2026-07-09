using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;

namespace SGMC.Application.Interfaces.Persistence
{
    public interface IDoctorAdoRepository
    {
        Task<List<DoctorDto>> ListActiveAsync();
        Task<List<DoctorDto>> SearchByNameAsync(string name);
        Task<int> GetTotalAppointmentsAsync(int doctorId);
        Task<List<DoctorDto>> ListBySpecialtyAsync(short specialtyId);
        Task<DoctorDto?> GetByIdWithDetailsAsync(int doctorId);
        Task<bool> UpdateAvailabilityModeAsync(int doctorId, short availabilityModeId);
    }
}