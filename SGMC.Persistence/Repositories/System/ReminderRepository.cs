using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Repositories.System;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Repositories.System
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly HealtSyncContext _context;

        public ReminderRepository(HealtSyncContext context)
        {
            _context = context;
        }

        public async Task<Reminder> AddAsync(Reminder reminder)
        {
            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<Reminder?> GetByIdAsync(int reminderId)
            => await _context.Reminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId);

        public async Task<List<Reminder>> GetByAppointmentIdAsync(int appointmentId)
            => await _context.Reminders
                .Where(r => r.AppointmentId == appointmentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<List<Reminder>> GetPendingByAppointmentIdAsync(int appointmentId)
            => await _context.Reminders
                .Where(r => r.AppointmentId == appointmentId && r.Status == "Pendiente")
                .ToListAsync();

        public async Task<List<Reminder>> GetDueRemindersAsync(DateTime asOf)
            => await _context.Reminders
                .Where(r => r.Status == "Pendiente" && r.ScheduledAt <= asOf)
                .ToListAsync();

        public async Task UpdateAsync(Reminder reminder)
        {
            _context.Reminders.Update(reminder);
            await _context.SaveChangesAsync();
        }
    }
}