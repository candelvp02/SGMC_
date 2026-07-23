using SGMC.Domain.Entities.Medical;

namespace SGMC.Domain.Repositories.Medical
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord> AddAsync(MedicalRecord record);
        Task<MedicalRecord> UpdateAsync(MedicalRecord record);
        Task<MedicalRecord?> GetByIdAsync(int id);
        Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId);
        Task<MedicalRecord?> GetByIdWithDetailsAsync(int recordId);
        Task<bool> ExistsAsync(int recordId);
        Task DeleteAsync(int id);
        Task<IEnumerable<MedicalRecord>> GetAllWithDetailsAsync();
    }
}