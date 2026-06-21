namespace SGMC.Application.Dto.Users
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public DateTime TokenExpiration { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }
}