using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Validators.Common;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.Users
{
    // Validador para DTOs de User
    public static class UserValidator
    {
        // Valida RegisterUserDto
        public static OperationResult IsValidDto(this RegisterUserDto dto)
        {
            var errores = new List<string>();

            // Valida email
            if (!ValidationHelper.IsValidEmail(dto.Email))
                errores.Add("Formato de email inválido.");

            // Valida password
            if (!ValidationHelper.IsValidPassword(dto.Password))
                errores.Add("La contraseña debe tener al menos 8 caracteres, e incluir mayúsculas, minúsculas y números.");

            // Valida RoleId
            if (dto.RoleId <= 0)
                errores.Add("El ID del rol es requerido.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de registro de usuario.", errores)
                : OperationResult.Exito();
        }

        // Valida UpdateUserDto
        public static OperationResult IsValidDto(this UpdateUserDto dto)
        {
            var errores = new List<string>();

            if (dto.UserId <= 0)
                errores.Add("El ID del usuario es inválido.");

            // Valida email
            if (!ValidationHelper.IsValidEmail(dto.Email))
                errores.Add("Formato de email inválido.");

            // Valida RoleId
            if (dto.RoleId is null or <= 0)
                errores.Add("El ID del rol es requerido.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de actualización de usuario.", errores)
                : OperationResult.Exito();
        }
    }
}