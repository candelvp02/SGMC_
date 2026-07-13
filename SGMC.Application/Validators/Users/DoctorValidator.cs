using SGMC.Application.Dto.System;
using SGMC.Application.Validators.Common;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.Users
{
    public static class DoctorValidator
    {
        // Valida RegisterDoctorDto
        public static OperationResult IsValidDto(this RegisterDoctorDto dto)
        {
            var errores = new List<string>();

            // Validaciones de nombre
            if (!ValidationHelper.IsValidLength(dto.FirstName, 2, 40))
                errores.Add("El nombre debe tener entre 2 y 40 caracteres.");
            if (!ValidationHelper.IsValidLength(dto.LastName, 2, 40))
                errores.Add("El apellido debe tener entre 2 y 40 caracteres.");

            // Valida cedula
            if (!ValidationHelper.IsValidCedula(dto.IdentificationNumber))
                errores.Add("Formato de cédula inválido.");

            // género debe ser "Masculino" o "Femenino" (no "M"/"F")
            if (!ValidationHelper.IsValidGenderFull(dto.Gender))
                errores.Add("El género debe ser 'Masculino' o 'Femenino'.");

            // Valida email
            if (!ValidationHelper.IsValidEmail(dto.Email))
                errores.Add("Formato de email inválido.");

            // Valida password
            if (string.IsNullOrWhiteSpace(dto.Password) ||
                dto.Password.Length < 8 ||
                dto.Password.Length > 100 ||
                !ValidationHelper.IsValidPassword(dto.Password))
            {
                errores.Add("La contraseña debe ser segura (Mayúsculas, minúsculas, números).");
            }

            // Valida specialty
            if (dto.SpecialtyId <= 0)
                errores.Add("La especialidad es requerida.");

            // licencia no puede estar vencida
            if (dto.LicenseExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
                errores.Add("La licencia médica está vencida. Por favor, renuévela antes de registrarse.");

            // años de experiencia no pueden ser negativos
            if (dto.YearsOfExperience < 0)
                errores.Add("Los años de experiencia no pueden ser negativos.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de doctor.", errores)
                : OperationResult.Exito();
        }

        // Valida UpdateDoctorDto
        public static OperationResult IsValidDto(this UpdateDoctorDto dto)
        {
            var errores = new List<string>();
            if (dto.DoctorId <= 0)
                errores.Add("El ID del doctor es inválido.");

            // también validar en updates
            if (dto.LicenseExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
                errores.Add("La licencia médica está vencida. Por favor, renuévela antes de actualizar.");

            if (dto.YearsOfExperience < 0)
                errores.Add("Los años de experiencia no pueden ser negativos.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de actualización de doctor.", errores)
                : OperationResult.Exito();
        }
    }
}