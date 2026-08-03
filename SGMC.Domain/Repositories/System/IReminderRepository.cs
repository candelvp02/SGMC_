using SGMC.Domain.Entities.System;

namespace SGMC.Domain.Repositories.System
{
    public interface IReminderRepository
    {
        Task<Reminder> AddAsync(Reminder reminder);
        Task<Reminder?> GetByIdAsync(int reminderId);
        Task<List<Reminder>> GetByAppointmentIdAsync(int appointmentId);
        Task<List<Reminder>> GetPendingByAppointmentIdAsync(int appointmentId);
        Task<List<Reminder>> GetDueRemindersAsync(DateTime asOf);
        Task UpdateAsync(Reminder reminder);
    }
}