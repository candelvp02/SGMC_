using SGMC.Application.Dto.System;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IReminderService
    {
        List<ReminderTemplateDto> GetTemplates();
        Task<OperationResult<ReminderDto>> ScheduleAsync(ScheduleReminderDto dto, int doctorId);
        Task<OperationResult> CancelAsync(int reminderId, int doctorId);
        Task<OperationResult<List<ReminderDto>>> GetByAppointmentIdAsync(int appointmentId);
        Task CancelPendingRemindersForAppointmentAsync(int appointmentId);
    }
}