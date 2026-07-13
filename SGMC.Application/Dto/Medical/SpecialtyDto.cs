namespace SGMC.Application.Dto.Medical
{
    public class SpecialtyDto
    {
        public short SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
    public class CreateSpecialtyDto
    {
        public string SpecialtyName { get; set; } = string.Empty;
    }
    public class UpdateSpecialtyDto
    {
        public short SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}