using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Users;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Repositories.Users
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly HealtSyncContext _context;

        public PasswordResetTokenRepository(HealtSyncContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetValidTokenAsync(int userId, string hashedToken)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.UserId == userId
                         && t.Token == hashedToken
                         && !t.IsUsed
                         && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidatePreviousTokensAsync(int userId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
                token.IsUsed = true;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}