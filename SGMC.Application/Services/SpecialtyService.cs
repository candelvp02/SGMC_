using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Medical;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.Medical;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Repositories.Medical;

namespace SGMC.Application.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ISpecialtyRepository _repository;
        private readonly ILogger<SpecialtyService> _logger;

        public SpecialtyService(ISpecialtyRepository repository, ILogger<SpecialtyService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<OperationResult<SpecialtyDto>> CreateAsync(CreateSpecialtyDto dto)
        {
            if (dto == null) return OperationResult<SpecialtyDto>.Fallo("Datos requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<SpecialtyDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                if (await _repository.ExistsByNameAsync(dto.SpecialtyName))
                    return OperationResult<SpecialtyDto>.Fallo("Ya existe una especialidad con ese nombre.");

                var specialty = new Specialty
                {
                    SpecialtyName = dto.SpecialtyName,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var created = await _repository.AddAsync(specialty);
                if (created == null)
                    return OperationResult<SpecialtyDto>.Fallo("No se pudo crear la especialidad.");

                return OperationResult<SpecialtyDto>.Exito(
                    MapToDto(created),
                    "Especialidad creada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear especialidad");
                return OperationResult<SpecialtyDto>.Fallo("Error interno al crear especialidad");
            }
        }

        public async Task<OperationResult<SpecialtyDto>> UpdateAsync(UpdateSpecialtyDto dto)
        {
            if (dto == null) return OperationResult<SpecialtyDto>.Fallo("Datos requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<SpecialtyDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var existing = await _repository.GetByIdAsync(dto.SpecialtyId);
                if (existing == null)
                    return OperationResult<SpecialtyDto>.Fallo("Especialidad no encontrada");

                if (existing.SpecialtyName != dto.SpecialtyName &&
                    await _repository.ExistsByNameAsync(dto.SpecialtyName))
                    return OperationResult<SpecialtyDto>.Fallo("Ya existe otra especialidad con ese nombre.");

                existing.SpecialtyName = dto.SpecialtyName;
                existing.IsActive = dto.IsActive;
                existing.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(existing);

                return OperationResult<SpecialtyDto>.Exito(
                    MapToDto(existing),
                    "Especialidad actualizada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar especialidad {Id}", dto?.SpecialtyId);
                return OperationResult<SpecialtyDto>.Fallo("Error interno al actualizar especialidad");
            }
        }

        public async Task<OperationResult> DeleteAsync(short id)
        {
            if (id <= 0) return OperationResult.Fallo("El ID de la especialidad es inválido");

            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return OperationResult.Fallo("Especialidad no encontrada");

                await _repository.DeleteAsync(existing);

                return OperationResult.Exito("Especialidad eliminada correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar especialidad {Id}", id);
                return OperationResult.Fallo("Error al eliminar especialidad");
            }
        }

        public async Task<OperationResult<List<SpecialtyDto>>> GetAllAsync()
        {
            try
            {
                var specialties = await _repository.GetAllAsync();
                return OperationResult<List<SpecialtyDto>>.Exito(
                    specialties.Select(MapToDto).ToList(),
                    "Especialidades obtenidas correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especialidades");
                return OperationResult<List<SpecialtyDto>>.Fallo("Error al obtener especialidades");
            }
        }

        public async Task<OperationResult<SpecialtyDto>> GetByIdAsync(short id)
        {
            try
            {
                if (id <= 0) return OperationResult<SpecialtyDto>.Fallo("El ID es inválido");

                var specialty = await _repository.GetByIdAsync(id);
                if (specialty == null)
                    return OperationResult<SpecialtyDto>.Fallo("Especialidad no encontrada");

                return OperationResult<SpecialtyDto>.Exito(
                    MapToDto(specialty),
                    "Especialidad obtenida correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especialidad {Id}", id);
                return OperationResult<SpecialtyDto>.Fallo("Error al obtener especialidad");
            }
        }

        public async Task<OperationResult<List<SpecialtyDto>>> GetActiveAsync()
        {
            try
            {
                var specialties = await _repository.GetActiveAsync();
                return OperationResult<List<SpecialtyDto>>.Exito(
                    specialties.Select(MapToDto).ToList(),
                    "Especialidades activas obtenidas correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener especialidades activas");
                return OperationResult<List<SpecialtyDto>>.Fallo("Error al obtener especialidades activas");
            }
        }

        public async Task<OperationResult<bool>> ExistsAsync(short id)
        {
            try
            {
                if (id <= 0) return OperationResult<bool>.Exito(false, "ID inválido");

                var exists = await _repository.ExistsAsync(id);
                return OperationResult<bool>.Exito(
                    exists,
                    exists ? "La especialidad existe" : "La especialidad no existe");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de especialidad {Id}", id);
                return OperationResult<bool>.Fallo("Error al verificar existencia de especialidad");
            }
        }

        public async Task<OperationResult<bool>> ExistsByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return OperationResult<bool>.Fallo("El nombre es requerido");

                var exists = await _repository.ExistsByNameAsync(name);
                return OperationResult<bool>.Exito(
                    exists,
                    exists ? "Ya existe una especialidad con ese nombre" : "El nombre está disponible");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar nombre de especialidad {Name}", name);
                return OperationResult<bool>.Fallo("Error al verificar nombre de especialidad");
            }
        }

        private static SpecialtyDto MapToDto(Specialty s) => new()
        {
            SpecialtyId = s.SpecialtyId,
            SpecialtyName = s.SpecialtyName,
            IsActive = s.IsActive
        };
    }
}
