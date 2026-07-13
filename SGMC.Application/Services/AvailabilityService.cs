using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Validators.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IDoctorAvailabilityRepository _repository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(
            IDoctorAvailabilityRepository repository,
            IDoctorRepository doctorRepository,
            ILogger<AvailabilityService> logger)
        {
            _repository = repository;
            _doctorRepository = doctorRepository;
            _logger = logger;
        }

        public async Task<OperationResult<AvailabilityDto>> CreateAsync(CreateAvailabilityDto dto)
        {
            if (dto is null) return OperationResult<AvailabilityDto>.Fallo("Datos de disponibilidad requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<AvailabilityDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                if (!await _doctorRepository.ExistsAsync(d => d.DoctorId == dto.DoctorId))
                    return OperationResult<AvailabilityDto>.Fallo("El doctor no existe.");

                var conflictExists = await _repository.CheckForConflictAsync(
                    dto.DoctorId, dto.AvailableDate, dto.StartTime, dto.EndTime);

                if (conflictExists)
                    return OperationResult<AvailabilityDto>.Fallo("El horario entra en conflicto con una disponibilidad existente.");

                var availability = new DoctorAvailability
                {
                    DoctorId = dto.DoctorId,
                    AvailableDate = dto.AvailableDate,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    AvailabilityModeId = dto.AvailabilityModeId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _repository.AddAsync(availability);
                var created = await _repository.GetByIdAsync(availability.AvailabilityId);

                return OperationResult<AvailabilityDto>.Exito(MapToDto(created)!, "Disponibilidad creada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear disponibilidad.");
                return OperationResult<AvailabilityDto>.Fallo($"Error interno al crear disponibilidad: {ex.Message}");
            }
        }

        public async Task<OperationResult<AvailabilityDto>> UpdateAsync(UpdateAvailabilityDto dto)
        {
            if (dto is null) return OperationResult<AvailabilityDto>.Fallo("Datos de actualización requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<AvailabilityDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var existing = await _repository.GetByIdAsync(dto.AvailabilityId);
                if (existing is null)
                    return OperationResult<AvailabilityDto>.Fallo("Disponibilidad no encontrada.");

                var conflictExists = await _repository.CheckForConflictExcludingCurrentAsync(
                    dto.AvailabilityId, dto.DoctorId, dto.AvailableDate, dto.StartTime, dto.EndTime);

                if (conflictExists)
                    return OperationResult<AvailabilityDto>.Fallo("El nuevo horario entra en conflicto con otra disponibilidad.");

                existing.AvailableDate = dto.AvailableDate;
                existing.StartTime = dto.StartTime;
                existing.EndTime = dto.EndTime;
                existing.AvailabilityModeId = dto.AvailabilityModeId;
                existing.IsActive = dto.IsActive;
                existing.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(existing);
                var updated = await _repository.GetByIdAsync(existing.AvailabilityId);

                return OperationResult<AvailabilityDto>.Exito(MapToDto(updated)!, "Disponibilidad actualizada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar disponibilidad {Id}", dto.AvailabilityId);
                return OperationResult<AvailabilityDto>.Fallo($"Error interno al actualizar disponibilidad: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            if (id <= 0) return OperationResult.Fallo("ID de disponibilidad inválido.");

            try
            {
                var exists = await _repository.ExistsAsync(id);
                if (!exists) return OperationResult.Fallo("Disponibilidad no encontrada.");

                await _repository.DeleteAsync(id);
                return OperationResult.Exito("Disponibilidad eliminada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar disponibilidad {Id}", id);
                return OperationResult.Fallo($"Error al eliminar disponibilidad: {ex.Message}");
            }
        }

        public async Task<OperationResult<AvailabilityDto>> GetByIdAsync(int id)
        {
            if (id <= 0) return OperationResult<AvailabilityDto>.Fallo("ID de disponibilidad inválido.");

            try
            {
                var availability = await _repository.GetByIdAsync(id);
                if (availability is null)
                    return OperationResult<AvailabilityDto>.Fallo("Disponibilidad no encontrada.");

                return OperationResult<AvailabilityDto>.Exito(MapToDto(availability)!, "Disponibilidad obtenida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener disponibilidad {Id}", id);
                return OperationResult<AvailabilityDto>.Fallo($"Error al obtener disponibilidad: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<AvailabilityDto>>> GetByDoctorIdAsync(int doctorId)
        {
            if (doctorId <= 0) return OperationResult<List<AvailabilityDto>>.Fallo("ID de doctor inválido.");

            try
            {
                var availability = await _repository.GetByDoctorIdAsync(doctorId);
                var dtoList = availability.Select(MapToDto).ToList();
                return OperationResult<List<AvailabilityDto>>.Exito(dtoList!, "Disponibilidad del doctor obtenida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener disponibilidad del doctor {Id}", doctorId);
                return OperationResult<List<AvailabilityDto>>.Fallo($"Error al obtener disponibilidad: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<AvailabilityDto>>> GetByDoctorAndDateRangeAsync(int doctorId, DateOnly startDate, DateOnly endDate)
        {
            if (doctorId <= 0) return OperationResult<List<AvailabilityDto>>.Fallo("ID de doctor inválido.");
            if (startDate > endDate) return OperationResult<List<AvailabilityDto>>.Fallo("El rango de fechas es inválido.");

            try
            {
                var availability = await _repository.GetByDoctorAndDateRangeAsync(doctorId, startDate, endDate);
                var dtoList = availability.Select(MapToDto).ToList();
                return OperationResult<List<AvailabilityDto>>.Exito(dtoList!, "Disponibilidad del doctor obtenida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener disponibilidad del doctor {Id} en el rango de fechas", doctorId);
                return OperationResult<List<AvailabilityDto>>.Fallo($"Error al obtener disponibilidad: {ex.Message}");
            }
        }

        private static AvailabilityDto? MapToDto(DoctorAvailability? a)
        {
            if (a == null) return null;

            return new AvailabilityDto
            {
                AvailabilityId = a.AvailabilityId,
                DoctorId = a.DoctorId,
                AvailableDate = a.AvailableDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                AvailabilityModeId = a.AvailabilityModeId,
                AvailabilityModeName = a.AvailabilityMode?.AvailabilityMode1,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            };
        }
    }
}