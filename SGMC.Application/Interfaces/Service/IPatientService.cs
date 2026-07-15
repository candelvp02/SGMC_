using SGMC.Application.Dto.Users;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IPatientService
    {
        //crud rf3.1.12 gestion de pacientes
        Task<OperationResult<PatientDto>> CreateAsync(RegisterPatientDto dto);
        Task<OperationResult<PatientDto>> UpdateAsync(UpdatePatientDto dto);
        Task<OperationResult<PatientDto>> PatchContactInfoAsync(int patientId, PatchPatientContactDto dto);
        Task<OperationResult<PatientDto>> PatchInsuranceProviderAsync(int patientId, PatchPatientInsuranceDto dto);
        Task<OperationResult> DeleteAsync(int id);

        //consultas
        Task<OperationResult<List<PatientDto>>> GetAllAsync();
        Task<OperationResult<PatientDto>> GetByIdAsync(int id);
        Task<OperationResult<List<PatientDto>>> GetActiveAsync();
        Task<OperationResult<List<PatientDto>>> GetByInsuranceProviderAsync(int insuranceProviderId);
        Task<OperationResult<PatientDto>> GetByPhoneNumberAsync(string phoneNumber);
        Task<OperationResult<bool>> ExistsAsync(int patientId);

        //con relaciones
        Task<OperationResult<PatientDto>> GetByIdWithDetailsAsync(int patientId);
        Task<OperationResult<List<PatientDto>>> GetWithAppointmentsAsync(int patientId);
        Task<OperationResult<List<PatientDto>>> GetWithMedicalRecordsAsync(int patientId);
        Task<OperationResult<PatientDto>> GetByUserIdAsync(int userId);
    }
}