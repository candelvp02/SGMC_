using SGMC.Application.Dto.Users;
using SGMC.Application.Validators.Common;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.Users
{
    // Validador para DTOs de Patient
    public static class PatientValidator
    {
        // Valida RegisterPatientDto
        public static OperationResult IsValidDto(this RegisterPatientDto dto)
        {
            var errores = new List<string>();

            // Validaciones de nombre
            if (!ValidationHelper.IsValidLength(dto.FirstName, 2, 40))
                errores.Add("El nombre debe tener entre 2 y 40 caracteres.");

            if (!ValidationHelper.IsValidLength(dto.LastName, 2, 40))
                errores.Add("El apellido debe tener entre 2 y 40 caracteres.");

            // Valida cedula
            if (!ValidationHelper.IsValidCedula(dto.IdentificationNumber))
                errores.Add("Formato de cédula inválido. Debe ser XXX-XXXXXXX-X.");

            // Valida gender
            if (!ValidationHelper.IsValidGender(dto.Gender))
                errores.Add("El género debe ser 'M' o 'F'.");

            // Valida de fecha de nacimiento
            if (!ValidationHelper.IsValidAge(dto.DateOfBirth))
                errores.Add("La fecha de nacimiento no es válida.");

            // Valida de email
            if (!ValidationHelper.IsValidEmail(dto.Email))
                errores.Add("Formato de email inválido.");

            // Valida password
            if (!ValidationHelper.IsValidPassword(dto.Password))
                errores.Add("La contraseña debe tener al menos 8 caracteres, incluir mayúsculas, minúsculas y números.");

            // Validac de proveedor de seguro
            if (dto.InsuranceProviderId <= 0)
                errores.Add("El proveedor de seguro es requerido.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de paciente.", errores)
                : OperationResult.Exito();
        }

        // Valida PatchPatientInsuranceDto
        public static OperationResult IsValidDto(this PatchPatientInsuranceDto dto)
        {
            var errores = new List<string>();

            if (dto.InsuranceProviderId <= 0)
                errores.Add("Debe seleccionar un proveedor de seguro v\u00e1lido.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validaci\u00f3n de seguro m\u00e9dico.", errores)
                : OperationResult.Exito();
        }

        // Valida UpdatePatientDto
        public static OperationResult IsValidDto(this UpdatePatientDto dto)
        {
            var errores = new List<string>();

            if (dto.PatientId <= 0)
                errores.Add("El ID del paciente es inválido.");

            if (dto.InsuranceProviderId <= 0)
                errores.Add("El proveedor de seguro es requerido.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de actualización de paciente.", errores)
                : OperationResult.Exito();
        }
    }
}