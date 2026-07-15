using SGMC.Application.Dto.Appointments;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IAppointmentService
    {
        // gestion de citas rf3.1.2/3.1.3/3.1.4/3.1.5
        Task<OperationResult<AppointmentDto>> CreateAsync(CreateAppointmentDto appointmentDto);
        Task<OperationResult> CancelAsync(int appointmentId);
        Task<OperationResult> ConfirmAsync(int appointmentId);
        Task<OperationResult> RejectAsync(int appointmentId);
        Task<OperationResult> RescheduleAsync(int appointmentId, DateTime newDate);
        Task<OperationResult<AppointmentDto>> UpdateAsync(UpdateAppointmentDto appointmentDto);
        Task<OperationResult> DeleteAsync(int id);


        //consultas rf3.1.6/3.1.8
        Task<OperationResult<List<AppointmentDto>>> GetAllAsync();
        Task<OperationResult<AppointmentDto>> GetByIdAsync(int id);
        Task<OperationResult<List<AppointmentDto>>> GetByPatientIdAsync(int patientId);
        Task<OperationResult<List<AppointmentDto>>> GetByDoctorIdAsync(int doctorId);
        Task<OperationResult<List<AppointmentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<OperationResult<List<AppointmentDto>>> GetUpcomingForPatientAsync(int patientId);
        Task<OperationResult<List<AppointmentDto>>> GetFilteredAppointmentsAsync(AppointmentFilterDto filter);
    }
}