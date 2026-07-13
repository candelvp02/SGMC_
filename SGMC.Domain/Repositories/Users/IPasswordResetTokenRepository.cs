using SGMC.Domain.Entities.Users;

namespace SGMC.Domain.Repositories.Users
{
    public interface IPasswordResetTokenRepository
    {
        Task AddAsync(PasswordResetToken token);
        Task<PasswordResetToken?> GetValidTokenAsync(int userId, string hashedToken);
        Task InvalidatePreviousTokensAsync(int userId);
        Task UpdateAsync(PasswordResetToken token);
    }
}