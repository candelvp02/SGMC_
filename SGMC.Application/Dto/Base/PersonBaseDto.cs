using SGMC.Application.Dto.Appointments;

namespace SGMC.Application.Dto.Base
{
    public abstract class PersonBaseDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string IdentificationNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public abstract class RegisterPersonBaseDto : PersonBaseDto
    {
        public string Password { get; set; } = string.Empty;
    }
}