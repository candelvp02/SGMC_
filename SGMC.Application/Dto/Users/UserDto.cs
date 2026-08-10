using SGMC.Application.Dto.Base;
using SGMC.Domain.Entities.Users;

namespace SGMC.Application.Dto.Users
{
    public class UserDto : PersonBaseDto
    {
        public string? PhoneNumber;

        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; internal set; }
    }
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public bool IsActive { get; set; }

        private static UserDto? MapToDto(User? u)
        {
            if (u == null)
                return null;

            return new UserDto
            {
                UserId = u.UserId,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.RoleName ?? string.Empty,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            };
        }
    }
}