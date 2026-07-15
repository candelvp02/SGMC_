using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Repositories.Medical;
using SGMC.Persistence.Base;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Repositories.Medical
{
    public sealed class SpecialtyRepository : BaseRepository<Specialty>, ISpecialtyRepository
    {
        public SpecialtyRepository(HealtSyncContext context) : base(context) { }

        public async Task<Specialty?> GetByIdAsync(short specialtyId)
            => await _dbSet.FirstOrDefaultAsync(s => s.SpecialtyId == specialtyId);

        public async Task<IEnumerable<Specialty>> GetActiveSpecialtiesAsync()
            => await _dbSet.Where(s => s.IsActive)
                            .OrderBy(s => s.SpecialtyName)
                            .ToListAsync();

        public async Task<IEnumerable<Specialty>> GetActiveAsync()
            => await GetActiveSpecialtiesAsync();

        public async Task<Specialty?> GetNameAsync(string specialtyName)
            => await _dbSet.FirstOrDefaultAsync(s => s.SpecialtyName == specialtyName);

        public async Task<bool> ExistsAsync(short specialtyId)
            => await _dbSet.AnyAsync(s => s.SpecialtyId == specialtyId);

        public async Task<bool> ExistsByNameAsync(string specialtyName)
            => await _dbSet.AnyAsync(s => s.SpecialtyName == specialtyName);

        public async Task DeleteAsync(short specialtyId)
        {
            var entity = await GetByIdAsync(specialtyId);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Specialty existing)
        {
            _dbSet.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}