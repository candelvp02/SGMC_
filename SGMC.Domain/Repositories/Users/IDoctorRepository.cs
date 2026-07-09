using SGMC.Domain.Entities.Users;

namespace SGMC.Domain.Repositories.Users
{
    public interface IDoctorRepository : IBaseRepository<Doctor>
    {
        new Task UpdateAsync(Doctor doctor);
        Task<Doctor?> GetByEmailAsync(string email);
        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<Doctor>> GetAllWithDetailsAsync();
        Task<Doctor?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Doctor>> GetBySpecialtyIdAsync(short specialtyId);
        Task<IEnumerable<Doctor>> GetActiveDoctorsAsync();
        Task<Doctor?> GetByLicenseNumberAsync(string licenseNumber);
        Task<IEnumerable<Doctor>> SearchAsync(string? name, short? specialtyId);
    }
}