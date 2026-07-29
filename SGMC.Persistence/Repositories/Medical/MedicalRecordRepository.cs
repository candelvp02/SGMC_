using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Repositories.Medical;
using SGMC.Persistence.Base;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Repositories.Medical
{
    public sealed class MedicalRecordRepository : BaseRepository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(HealtSyncContext context) : base(context) { }

        public override async Task<MedicalRecord?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
            => await _dbSet.Where(m => m.PatientId == patientId).ToListAsync();

        public async Task<IEnumerable<MedicalRecord>> GetByDoctorIdAsync(int doctorId)
            => await _dbSet.Where(m => m.DoctorId == doctorId).ToListAsync();

        public async Task<MedicalRecord?> GetByIdWithDetailsAsync(int recordId)
        {
            return await _dbSet
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .FirstOrDefaultAsync(m => m.RecordId == recordId);
        }

        public async Task<IEnumerable<MedicalRecord>> GetByPatientIdWithDetailsAsync(int patientId)
        {
            return await _dbSet
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.DateOfVisit)
                .ToListAsync();
        }

        public override async Task UpdateAsync(MedicalRecord record)
        {
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        async Task<MedicalRecord> IMedicalRecordRepository.UpdateAsync(MedicalRecord record)
        {
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> ExistsAsync(int recordId)
        {
            return await _dbSet.AnyAsync(m => m.RecordId == recordId);
        }

        public async Task<IEnumerable<MedicalRecord>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .ToListAsync();
        }
    }
}