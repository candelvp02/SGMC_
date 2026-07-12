using SGMC.Application.Dto.Users;

namespace SGMC.Web.Services
{
    public interface IDoctorApiClient
    {
        Task<ApiResponse<List<DoctorDto>>> GetAllAsync();
        Task<ApiResponse<DoctorDto>> GetByIdAsync(int id);
        Task<ApiResponse<DoctorDto>> CreateAsync(RegisterDoctorDto dto);
        Task<ApiResponse<DoctorDto>> UpdateAsync(UpdateDoctorDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<List<DoctorDto>>> GetBySpecialtyAsync(short specialtyId);
    }
}