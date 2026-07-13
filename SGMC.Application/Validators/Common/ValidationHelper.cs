using System.Text.RegularExpressions;

namespace SGMC.Application.Validators.Common
{
    //clase con helpers de validacion comunes
    public static class ValidationHelper
    {
        // regex compiladas para mejor performance
        private static readonly Regex CedulaRegex = new(@"^\d{3}-\d{7}-\d{1}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex UpperRegex = new(@"[A-Z]", RegexOptions.Compiled);
        private static readonly Regex LowerRegex = new(@"[a-z]", RegexOptions.Compiled);
        private static readonly Regex DigitRegex = new(@"\d", RegexOptions.Compiled);

        // Valida formato de cedula dominicana (XXX-XXXXXXX-X)
        public static bool IsValidCedula(string? cedula)
        {
            return !string.IsNullOrWhiteSpace(cedula) && CedulaRegex.IsMatch(cedula);
        }

        // Valida formato de email
        public static bool IsValidEmail(string? email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }

        // Valida que la password cumpla con requisitos de seguridad
        public static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            return UpperRegex.IsMatch(password) &&
                   LowerRegex.IsMatch(password) &&
                   DigitRegex.IsMatch(password);
        }

        // Valida genero (M o F)
        public static bool IsValidGender(string? gender)
        {
            return gender == "M" || gender == "F";
        }
        public static bool IsValidGenderFull(string? gender)
        {
            return gender == "Masculino" || gender == "Femenino";
        }


        // Valida rango de edad valida
        public static bool IsValidAge(DateOnly? dateOfBirth)
        {
            if (!dateOfBirth.HasValue)
                return true;

            var age = DateTime.Now.Year - dateOfBirth.Value.Year;
            return age >= 0 && age <= 120;
        }

        // Valida longitud de texto entre min y max
        public static bool IsValidLength(string? text, int min, int max)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var length = text.Trim().Length;
            return length >= min && length <= max;
        }
    }
}
