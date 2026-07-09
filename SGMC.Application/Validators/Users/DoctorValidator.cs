using SGMC.Application.Dto.System;
using SGMC.Application.Validators.Common;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.Users
{
    // Validador para DTOs de Doctor
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

            // Valida gender
            if (!ValidationHelper.IsValidGender(dto.Gender))
                errores.Add("El género debe ser 'M' o 'F'.");

            // valida email
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

            // valida specialty
            if (dto.SpecialtyId <= 0)
                errores.Add("La especialidad es requerida.");

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
            if (dto.SpecialtyId <= 0)
                errores.Add("La especialidad es requerida.");
            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de actualización de doctor.", errores)
                : OperationResult.Exito();
        }
    }
}