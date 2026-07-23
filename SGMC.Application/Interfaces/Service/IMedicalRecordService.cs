using SGMC.Application.Dto.Medical;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IMedicalRecordService
    {
        // CRUD
        Task<OperationResult<MedicalRecordDto>> CreateAsync(CreateMedicalRecordDto dto);
        Task<OperationResult<MedicalRecordDto>> UpdateAsync(UpdateMedicalRecordDto dto);
        Task<OperationResult> DeleteAsync(int id);

        // Queries
        Task<OperationResult<MedicalRecordDto>> GetByIdAsync(int id);
        Task<OperationResult<List<MedicalRecordDto>>> GetAllAsync();
        Task<OperationResult<List<MedicalRecordDto>>> GetByPatientIdAsync(int patientId);
        Task<OperationResult<List<MedicalRecordDto>>> GetByDoctorIdAsync(int doctorId);

        // Acceso desde la Agenda del Médico
        Task<OperationResult<List<MedicalRecordDto>>> GetPatientHistoryForDoctorAsync(int doctorId, int patientId);
    }
}