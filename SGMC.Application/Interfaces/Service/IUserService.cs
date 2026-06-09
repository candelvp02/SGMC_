using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface IUserService
    {
        Task<OperationResult<UserDto>> AuthenticateAsync(UserLoginDto dto);
        Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto dto);
        Task<OperationResult> RequestPasswordResetAsync(string email);
        Task<OperationResult> ActivateAccountAsync(int userId);

        Task<OperationResult<UserDto>> UpdateProfileAsync(UpdateUserDto dto);
        Task<OperationResult> ChangePasswordAsync(ChangePasswordDto dto);

        Task<OperationResult<UserDto>> GetByIdAsync(int id);
        Task<OperationResult<UserDto>> GetByEmailAsync(string email);
        Task<OperationResult<List<UserDto>>> GetByRoleAsync(short roleId);
        Task<OperationResult<List<UserDto>>> GetActiveUsersAsync();
        Task<OperationResult<List<UserDto>>> GetAllAsync();
        Task<OperationResult<List<UserDto>>> SearchAsync(string query);

        Task<OperationResult> DeactivateAsync(int id);
        Task<OperationResult> ChangeRoleAsync(int userId, int roleId);
    }
}