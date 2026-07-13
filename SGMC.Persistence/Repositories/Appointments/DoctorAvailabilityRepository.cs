using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Persistence.Base;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Repositories.Appointments
{
    public sealed class DoctorAvailabilityRepository : BaseRepository<DoctorAvailability>, IDoctorAvailabilityRepository
    {
        public DoctorAvailabilityRepository(HealtSyncContext context) : base(context) { }

        public override async Task<DoctorAvailability?> GetByIdAsync(int id)
            => await _dbSet.Include(d => d.AvailabilityMode)
                            .FirstOrDefaultAsync(d => d.AvailabilityId == id);

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId)
            => await _dbSet.Where(d => d.DoctorId == doctorId)
                            .Include(d => d.AvailabilityMode)
                            .ToListAsync();

        public async Task<IEnumerable<DoctorAvailability>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
            => await _dbSet.Where(d => d.AvailableDate >= startDate && d.AvailableDate <= endDate)
                            .Include(d => d.AvailabilityMode)
                            .ToListAsync();

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorAndDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate)
            => await _dbSet.Where(d => d.DoctorId == doctorId && d.AvailableDate >= startDate && d.AvailableDate <= endDate)
                            .Include(d => d.AvailabilityMode)
                            .ToListAsync();

        public async Task<bool> IsAvailableAsync(int doctorId, DateOnly date, TimeOnly time)
        {
            return await _dbSet.AnyAsync(d =>
                d.DoctorId == doctorId &&
                d.AvailableDate == date &&
                d.StartTime <= time &&
                time < d.EndTime &&
                d.IsActive);
        }

        public async Task<bool> HasConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
            => await CheckForConflictAsync(doctorId, date, startTime, endTime);

        public new async Task AddAsync(DoctorAvailability availability)
        {
            await _dbSet.AddAsync(availability);
            await _context.SaveChangesAsync();
        }

        public new async Task UpdateAsync(DoctorAvailability availability)
        {
            _dbSet.Update(availability);
            await _context.SaveChangesAsync();
        }

        public override async Task DeleteAsync(int availabilityId)
        {
            var entity = await _dbSet.FindAsync(availabilityId);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
            => await _dbSet.AnyAsync(d => d.AvailabilityId == id);

        // Dos turnos se solapan si uno empieza antes de que el otro termine, en ambos sentidos
        public async Task<bool> CheckForConflictAsync(int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            return await _dbSet.AnyAsync(d =>
                d.DoctorId == doctorId &&
                d.AvailableDate == date &&
                d.IsActive &&
                d.StartTime < endTime &&
                startTime < d.EndTime);
        }

        public async Task<bool> CheckForConflictExcludingCurrentAsync(int availabilityId, int doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            return await _dbSet.AnyAsync(d =>
                d.AvailabilityId != availabilityId &&
                d.DoctorId == doctorId &&
                d.AvailableDate == date &&
                d.IsActive &&
                d.StartTime < endTime &&
                startTime < d.EndTime);
        }
    }
}