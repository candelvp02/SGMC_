using SGMC.Domain.Entities.Users;

namespace SGMC.Domain.Repositories.Users
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(int patientId);
        Task<IEnumerable<Patient>> GetAllAsync();
        Task<Patient> AddAsync(Patient patient);
        Task UpdateAsync(Patient patient);
        Task DeleteAsync(int patientId);
        Task<IEnumerable<Patient>> GetActivePatientsAsync();
        Task<IEnumerable<Patient>> GetByInsuranceProviderIdAsync(int insuranceProviderId);
        Task<Patient?> GetByPhoneNumberAsync(string phoneNumber);
        Task<bool> ExistsAsync(int patientId);
        Task<Patient?> GetByIdWithDetailsAsync(int patientId);
        Task<IEnumerable<Patient>> GetAllWithDetailsAsync();
        Task<Patient?> GetByIdWithAppointmentsAsync(int patientId);
        Task<Patient?> GetByIdWithMedicalRecordsAsync(int patientId);
        Task<Patient?> GetByIdentificationNumberAsync(string identificationNumber);
        Task GetByIdWithDetailsAsync(object id);
        Task<Patient?> GetByUserIdAsync(int userId);
    }
}