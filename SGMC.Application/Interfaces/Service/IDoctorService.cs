using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Users;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IDoctorService
    {
        // CRUD
        Task<OperationResult<DoctorDto>> CreateAsync(RegisterDoctorDto doctorDto);
        Task<OperationResult<DoctorDto>> UpdateAsync(UpdateDoctorDto doctorDto);
        Task<OperationResult> DeleteAsync(int id);

        // Queries
        Task<OperationResult<List<DoctorDto>>> GetAllAsync();
        Task<OperationResult<List<DoctorDto>>> GetAllWithDetailsAsync();
        Task<OperationResult<DoctorDto>> GetByIdAsync(int id);
        Task<OperationResult<DoctorDto>> GetByIdWithDetailsAsync(int id);
        Task<OperationResult<List<AppointmentDto>>> GetAppointmentsByDoctorIdAsync(int doctorId);
        Task<OperationResult<List<DoctorDto>>> GetBySpecialtyIdAsync(short specialtyId);
        Task<OperationResult<List<DoctorDto>>> GetActiveDoctorsAsync();
        Task<OperationResult<DoctorDto>> GetByLicenseNumberAsync(string licenseNumber);
        Task<OperationResult<bool>> ExistsByLicenseNumberAsync(string licenseNumber);
    }
}