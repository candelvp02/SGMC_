using SGMC.Application.Dto.Appointments;
using SGMC.Domain.Base;

namespace SGMC.Application.Validators.Appointments
{
    public static class AvailabilityValidator
    {
        public static OperationResult IsValidDto(this CreateAvailabilityDto dto)
        {
            var errores = new List<string>();
            if (dto.DoctorId <= 0)
                errores.Add("El ID del doctor es requerido.");
            if (dto.AvailableDate < DateOnly.FromDateTime(DateTime.Now))
                errores.Add("La fecha disponible no puede ser en el pasado.");
            if (dto.StartTime >= dto.EndTime)
                errores.Add("La hora de inicio debe ser menor que la hora de fin.");
            if (dto.StartTime.Hour < 7 || dto.EndTime.Hour > 19)
                errores.Add("El horario de disponibilidad debe estar entre 07:00 y 19:00.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de disponibilidad.", errores)
                : OperationResult.Exito();
        }

        public static OperationResult IsValidDto(this UpdateAvailabilityDto dto)
        {
            var errores = new List<string>();
            if (dto.AvailabilityId <= 0)
                errores.Add("El ID de disponibilidad es inválido.");
            if (dto.DoctorId <= 0)
                errores.Add("El ID del doctor es requerido.");
            if (dto.AvailableDate < DateOnly.FromDateTime(DateTime.Now))
                errores.Add("La fecha disponible no puede ser en el pasado.");
            if (dto.StartTime >= dto.EndTime)
                errores.Add("La hora de inicio debe ser menor que la hora de fin.");
            if (dto.StartTime.Hour < 7 || dto.EndTime.Hour > 19)
                errores.Add("El horario de disponibilidad debe estar entre 07:00 y 19:00.");

            return errores.Count > 0
                ? OperationResult.Fallo("Errores de validación de actualización de disponibilidad.", errores)
                : OperationResult.Exito();
        }
    }
}