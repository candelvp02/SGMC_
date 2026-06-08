using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;
        private readonly ILogger<PatientService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IInsuranceProviderRepository _insuranceProviderRepository;

        public PatientService(
            IPatientRepository repository,
            ILogger<PatientService> logger,
            IUserRepository userRepository,
            IPersonRepository personRepository,
            IInsuranceProviderRepository insuranceProviderRepository)
        {
            _repository = repository;
            _logger = logger;
            _userRepository = userRepository;
            _personRepository = personRepository;
            _insuranceProviderRepository = insuranceProviderRepository;
        }

        // create

        public async Task<OperationResult<PatientDto>> CreateAsync(RegisterPatientDto dto)
        {
            // Validaciones de campo
            if (dto == null)
                return OperationResult<PatientDto>.Fallo("Los datos del paciente son requeridos");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<PatientDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                // Validaciones de negocio (BD)
                if (await _personRepository.ExistsByIdentificationNumberAsync(dto.IdentificationNumber))
                    return OperationResult<PatientDto>.Fallo("Ya existe una persona con esa cédula");

                if (await _userRepository.ExistsByEmailAsync(dto.Email))
                    return OperationResult<PatientDto>.Fallo("El email ya está en uso");

                var insuranceExists = await _insuranceProviderRepository.ExistsAsync(dto.InsuranceProviderId);
                if (!insuranceExists)
                    return OperationResult<PatientDto>.Fallo("El proveedor de seguro seleccionado no existe");

                // create

                // 1) Crear User
                var user = new User
                {
                    Email = dto.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = 3, // Paciente
                    IsActive = false, //En espera a autenticación.
                    CreatedAt = DateTime.Now
                };

                var createdUser = await _userRepository.AddAsync(user);
                if (createdUser == null || createdUser.UserId <= 0)
                    return OperationResult<PatientDto>.Fallo("No se pudo crear el usuario del paciente");

                int newId = createdUser.UserId;

                // 2) Crear Person enlazada al mismo ID
                var person = new Person
                {
                    PersonId = newId,
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    IdentificationNumber = dto.IdentificationNumber.Trim(),
                    Gender = dto.Gender
                };

                await _personRepository.AddAsync(person);

                // vincular navegación en memoria
                createdUser.UserNavigation = person;

                // 3) Crear Patient con el mismo ID
                var patient = new Patient
                {
                    PatientId = newId,
                    Gender = dto.Gender,
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Address = dto.Address.Trim(),
                    EmergencyContactName = dto.EmergencyContactName.Trim(),
                    EmergencyContactPhone = dto.EmergencyContactPhone.Trim(),
                    BloodType = dto.BloodType,
                    Allergies = dto.Allergies?.Trim() ?? string.Empty,
                    InsuranceProviderId = dto.InsuranceProviderId,
                    IsActive = false, //En espera a autenticación.
                    CreatedAt = DateTime.Now,
                    PatientNavigation = person
                };

                var createdPatient = await _repository.AddAsync(patient);
                if (createdPatient == null)
                    return OperationResult<PatientDto>.Fallo("No se pudo crear el paciente");

                var dtoResult = MapToDto(createdPatient);
                dtoResult.Email = createdUser.Email;

                return OperationResult<PatientDto>.Exito(dtoResult, "Paciente creado correctamente");
            }
            catch (Exception ex)
            {
                var innerMessage =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                _logger.LogError(ex, "Error al crear paciente: {Message}", innerMessage);
                return OperationResult<PatientDto>.Fallo($"Error al crear paciente: {innerMessage}");
            }
        }

        // update

        public async Task<OperationResult<PatientDto>> UpdateAsync(UpdatePatientDto dto)
        {
            if (dto == null)
                return OperationResult<PatientDto>.Fallo("Los datos del paciente son requeridos");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<PatientDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var patient = await _repository.GetByIdWithDetailsAsync(dto.PatientId);
                if (patient == null)
                    return OperationResult<PatientDto>.Fallo("Paciente no encontrado");

                var insuranceExists = await _insuranceProviderRepository.ExistsAsync(dto.InsuranceProviderId);
                if (!insuranceExists)
                    return OperationResult<PatientDto>.Fallo("El proveedor de seguro seleccionado no existe");

                patient.PhoneNumber = dto.PhoneNumber.Trim();
                patient.Address = dto.Address.Trim();
                patient.EmergencyContactName = dto.EmergencyContactName.Trim();
                patient.EmergencyContactPhone = dto.EmergencyContactPhone.Trim();
                patient.Allergies = dto.Allergies?.Trim() ?? string.Empty;
                patient.InsuranceProviderId = dto.InsuranceProviderId;
                patient.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(patient);

                return OperationResult<PatientDto>.Exito(
                    MapToDto(patient),
                    "Paciente actualizado correctamente"
                );
            }
            catch (Exception ex)
            {
                var innerMessage =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                _logger.LogError(ex, "Error al actualizar paciente {Id}: {Message}", dto?.PatientId, innerMessage);
                return OperationResult<PatientDto>.Fallo($"Error al actualizar paciente: {innerMessage}");
            }
        }

        // delete

        public async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult.Fallo("El ID del paciente es inválido");

                var patient = await _repository.GetByIdAsync(id);
                if (patient == null)
                    return OperationResult.Fallo("Paciente no encontrado");

                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userRepository.UpdateAsync(user);
                }

                patient.IsActive = false;
                patient.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(patient);

                return OperationResult.Exito("Paciente desactivado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar paciente {Id}", id);
                return OperationResult.Fallo($"Error al eliminar paciente: {ex.Message}");
            }
        }

        // queries
        public async Task<OperationResult<List<PatientDto>>> GetAllAsync()
        {
            try
            {
                var patients = await _repository.GetAllWithDetailsAsync();

                return OperationResult<List<PatientDto>>.Exito(
                    patients.Select(MapToDto).ToList(),
                    "Pacientes obtenidos correctamente"
                );
            }
            catch (Exception ex)
            {
                var innerMessage =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                _logger.LogError(ex, "Error al obtener pacientes: {Message}", innerMessage);

                return OperationResult<List<PatientDto>>.Fallo(
                    $"Error al obtener pacientes: {innerMessage}"
                );
            }
        }

        public async Task<OperationResult<List<PatientDto>>> GetActiveAsync()
        {
            try
            {
                var patients = await _repository.GetActivePatientsAsync();
                return OperationResult<List<PatientDto>>.Exito(
                    patients.Select(MapToDto).ToList(),
                    "Pacientes activos obtenidos correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pacientes activos");
                return OperationResult<List<PatientDto>>.Fallo("Error al obtener pacientes activos");
            }
        }

        public async Task<OperationResult<List<PatientDto>>> GetByInsuranceProviderAsync(int insuranceProviderId)
        {
            try
            {
                if (insuranceProviderId <= 0)
                    return OperationResult<List<PatientDto>>.Fallo("El ID del proveedor de seguro es inválido");

                var patients = await _repository.GetByInsuranceProviderIdAsync(insuranceProviderId);
                return OperationResult<List<PatientDto>>.Exito(
                    patients.Select(MapToDto).ToList(),
                    "Pacientes obtenidos por proveedor de seguro"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener pacientes por seguro {Id}", insuranceProviderId);
                return OperationResult<List<PatientDto>>.Fallo("Error al obtener pacientes");
            }
        }

        public async Task<OperationResult<PatientDto>> GetByPhoneNumberAsync(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return OperationResult<PatientDto>.Fallo("El número de teléfono es requerido");

                var patient = await _repository.GetByPhoneNumberAsync(phoneNumber);
                if (patient == null)
                    return OperationResult<PatientDto>.Fallo("Paciente no encontrado");

                return OperationResult<PatientDto>.Exito(
                    MapToDto(patient),
                    "Paciente obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente por teléfono {Phone}", phoneNumber);
                return OperationResult<PatientDto>.Fallo("Error al obtener paciente");
            }
        }

        public async Task<OperationResult<bool>> ExistsAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    return OperationResult<bool>.Fallo("El ID del paciente es inválido");

                var exists = await _repository.ExistsAsync(patientId);
                return OperationResult<bool>.Exito(exists, "Verificación completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar paciente {Id}", patientId);
                return OperationResult<bool>.Fallo("Error al verificar paciente");
            }
        }

        public async Task<OperationResult<PatientDto>> GetByIdWithDetailsAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    return OperationResult<PatientDto>.Fallo("El ID del paciente es inválido");

                var patient = await _repository.GetByIdWithDetailsAsync(patientId);
                if (patient == null)
                    return OperationResult<PatientDto>.Fallo("Paciente no encontrado");

                return OperationResult<PatientDto>.Exito(
                    MapToDto(patient),
                    "Paciente con detalles obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente con detalles {Id}", patientId);
                return OperationResult<PatientDto>.Fallo("Error al obtener paciente con detalles");
            }
        }

        public async Task<OperationResult<List<PatientDto>>> GetWithAppointmentsAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    return OperationResult<List<PatientDto>>.Fallo("El ID del paciente es inválido");

                var patient = await _repository.GetByIdWithAppointmentsAsync(patientId);
                if (patient == null)
                    return OperationResult<List<PatientDto>>.Fallo("Paciente no encontrado");

                return OperationResult<List<PatientDto>>.Exito(
                    new List<PatientDto> { MapToDto(patient) },
                    "Paciente con citas obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente con citas {Id}", patientId);
                return OperationResult<List<PatientDto>>.Fallo("Error al obtener paciente con citas");
            }
        }

        public async Task<OperationResult<List<PatientDto>>> GetWithMedicalRecordsAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    return OperationResult<List<PatientDto>>.Fallo("El ID del paciente es inválido");

                var patient = await _repository.GetByIdWithMedicalRecordsAsync(patientId);
                if (patient == null)
                    return OperationResult<List<PatientDto>>.Fallo("Paciente no encontrado");

                return OperationResult<List<PatientDto>>.Exito(
                    new List<PatientDto> { MapToDto(patient) },
                    "Paciente con registros médicos obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente con registros {Id}", patientId);
                return OperationResult<List<PatientDto>>.Fallo("Error al obtener paciente con registros médicos");
            }
        }

        public async Task<OperationResult<PatientDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PatientDto>.Fallo("El ID del paciente es inválido");

                var patient = await _repository.GetByIdWithDetailsAsync(id);
                if (patient == null)
                    return OperationResult<PatientDto>.Fallo("Paciente no encontrado");

                return OperationResult<PatientDto>.Exito(
                    MapToDto(patient),
                    "Paciente obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener paciente {Id}", id);
                return OperationResult<PatientDto>.Fallo("Error al obtener paciente");
            }
        }

        // private mapping

        private static PatientDto MapToDto(Patient p) => new()
        {
            PatientId = p.PatientId,
            FirstName = p.PatientNavigation?.FirstName ?? string.Empty,
            LastName = p.PatientNavigation?.LastName ?? string.Empty,
            DateOfBirth = p.PatientNavigation?.DateOfBirth,
            IdentificationNumber = p.PatientNavigation?.IdentificationNumber ?? string.Empty,
            Email = p.PatientNavigation?.User?.Email ?? string.Empty,
            Gender = p.Gender,
            PhoneNumber = p.PhoneNumber,
            Address = p.Address,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            BloodType = p.BloodType,
            Allergies = p.Allergies,
            InsuranceProviderId = p.InsuranceProviderId,
            InsuranceProviderName = p.InsuranceProvider?.Name ?? string.Empty,
            IsActive = p.IsActive
        };
    }
}

