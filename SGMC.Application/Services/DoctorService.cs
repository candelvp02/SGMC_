using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Users;
using SGMC.Application.Validators.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Medical;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public partial class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _repository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<DoctorService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IPersonRepository _personRepository;
        private readonly ISpecialtyRepository _specialtyRepository;

        public DoctorService(
            IDoctorRepository repository,
            IAppointmentRepository appointmentRepository,
            ILogger<DoctorService> logger,
            IUserRepository userRepository,
            IPersonRepository personRepository,
            ISpecialtyRepository specialtyRepository)
        {
            _repository = repository;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
            _userRepository = userRepository;
            _personRepository = personRepository;
            _specialtyRepository = specialtyRepository;
        }

        //        CRUD
        public async Task<OperationResult<DoctorDto>> CreateAsync(RegisterDoctorDto doctorDto)
        {
            if (doctorDto == null)
                return OperationResult<DoctorDto>.Fallo("Los datos del doctor son requeridos");

            // validaciones de campo fuera del try-catch
            var validationResult = doctorDto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<DoctorDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                // validaciones de negocio
                if (await _personRepository.ExistsByIdentificationNumberAsync(doctorDto.IdentificationNumber))
                    return OperationResult<DoctorDto>.Fallo("Ya existe una persona con esa cédula");

                if (await _userRepository.ExistsByEmailAsync(doctorDto.Email))
                    return OperationResult<DoctorDto>.Fallo("El email ya está en uso");

                var specialtyExists = await _specialtyRepository.ExistsAsync(doctorDto.SpecialtyId);
                if (!specialtyExists)
                    return OperationResult<DoctorDto>.Fallo("La especialidad seleccionada no existe");

                // 1. Crear User
                var user = new User
                {
                    Email = doctorDto.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(doctorDto.Password),
                    RoleId = 2, // Doctor
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var createdUser = await _userRepository.AddAsync(user);
                if (createdUser == null)
                    return OperationResult<DoctorDto>.Fallo("No se pudo crear el usuario del doctor");

                var newId = createdUser.UserId;

                // 2. Crear Person
                var person = new Person
                {
                    PersonId = newId,
                    FirstName = doctorDto.FirstName.Trim(),
                    LastName = doctorDto.LastName.Trim(),
                    DateOfBirth = doctorDto.DateOfBirth,
                    IdentificationNumber = doctorDto.IdentificationNumber.Trim(),
                    Gender = doctorDto.Gender
                };

                createdUser.UserNavigation = person;
                await _personRepository.AddAsync(person);

                // 3. Crear Doctor
                var doctor = new Doctor
                {
                    DoctorId = newId,
                    SpecialtyId = doctorDto.SpecialtyId,
                    LicenseNumber = doctorDto.LicenseNumber.Trim(),
                    PhoneNumber = doctorDto.PhoneNumber.Trim(),
                    YearsOfExperience = doctorDto.YearsOfExperience,
                    Education = doctorDto.Education.Trim(),
                    Bio = doctorDto.Bio?.Trim() ?? string.Empty,
                    ConsultationFee = doctorDto.ConsultationFee,
                    ClinicAddress = doctorDto.ClinicAddress?.Trim() ?? string.Empty,
                    LicenseExpirationDate = doctorDto.LicenseExpirationDate,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    DoctorNavigation = person
                };

                var createdDoctor = await _repository.AddAsync(doctor);
                if (createdDoctor == null)
                    return OperationResult<DoctorDto>.Fallo("No se pudo crear el doctor");

                // reconsultar con detalles para tener navs llenas
                var doctorWithDetails = await _repository.GetByIdWithDetailsAsync(createdDoctor.DoctorId)
                                        ?? createdDoctor;

                return OperationResult<DoctorDto>.Exito(
                    MapToDtoWithDetails(doctorWithDetails),
                    "Doctor creado correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear doctor");
                return OperationResult<DoctorDto>.Fallo($"Error al crear doctor: {ex.Message}");
            }
        }

        public async Task<OperationResult<DoctorDto>> UpdateAsync(UpdateDoctorDto doctorDto)
        {
            if (doctorDto == null)
                return OperationResult<DoctorDto>.Fallo("Los datos del doctor son requeridos");

            // validaciones de campo fuera del trycatch
            var validationResult = doctorDto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<DoctorDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var doctor = await _repository.GetByIdAsync(doctorDto.DoctorId);
                if (doctor == null)
                    return OperationResult<DoctorDto>.Fallo("Doctor no encontrado");

                // update de Entidades
                doctor.PhoneNumber = doctorDto.PhoneNumber.Trim();
                doctor.YearsOfExperience = doctorDto.YearsOfExperience;
                doctor.Education = doctorDto.Education.Trim();
                doctor.Bio = doctorDto.Bio?.Trim() ?? string.Empty;
                doctor.ConsultationFee = doctorDto.ConsultationFee;
                doctor.ClinicAddress = doctorDto.ClinicAddress?.Trim() ?? string.Empty;
                doctor.AvailabilityModeId = doctorDto.AvailabilityMode;
                doctor.LicenseExpirationDate = doctorDto.LicenseExpirationDate;
                doctor.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(doctor);

                var updatedDoctor = await _repository.GetByIdWithDetailsAsync(doctor.DoctorId);

                return OperationResult<DoctorDto>.Exito(
                    MapToDtoWithDetails(updatedDoctor!),
                    "Doctor actualizado correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar doctor {DoctorId}", doctorDto?.DoctorId);
                return OperationResult<DoctorDto>.Fallo($"Error al actualizar doctor: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult.Fallo("El ID del doctor es inválido");

                var doctor = await _repository.GetByIdAsync(id);
                if (doctor == null)
                    return OperationResult.Fallo("Doctor no encontrado");

                var futureAppointments = await _appointmentRepository.GetByDoctorIdAsync(id);
                if (futureAppointments.Any(a => a.AppointmentDate > DateTime.Now && a.StatusId != 3))
                    return OperationResult.Fallo("No se puede desactivar un doctor con citas futuras programadas");

                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    user.IsActive = false;
                    await _userRepository.UpdateAsync(user);
                }

                doctor.IsActive = false;
                doctor.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(doctor);

                return OperationResult.Exito("Doctor desactivado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar doctor {Id}", id);
                return OperationResult.Fallo($"Error al eliminar doctor: {ex.Message}");
            }
        }


        //   METODOS DE CONSULTA
        public async Task<OperationResult<List<DoctorDto>>> GetAllAsync()
        {
            try
            {
                var doctors = await _repository.GetAllWithDetailsAsync();

                return OperationResult<List<DoctorDto>>.Exito(
                    doctors.Select(MapToDtoWithDetails).ToList(),
                    "Doctores obtenidos correctamente"
                );
            }
            catch (Exception ex)
            {
                var innerMessage =
                    ex.InnerException?.InnerException?.Message
                    ?? ex.InnerException?.Message
                    ?? ex.Message;

                _logger.LogError(ex, "Error al obtener doctores: {Message}", innerMessage);

                return OperationResult<List<DoctorDto>>.Fallo(
                    $"Error al obtener doctores: {innerMessage}"
                );
            }
        }

        public async Task<OperationResult<List<DoctorDto>>> GetAllWithDetailsAsync()
        {
            try
            {
                var doctors = await _repository.GetAllWithDetailsAsync();
                return OperationResult<List<DoctorDto>>.Exito(
                    doctors.Select(MapToDtoWithDetails).ToList(),
                    "Doctores con detalles obtenidos correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctores con detalles");
                return OperationResult<List<DoctorDto>>.Fallo("Error al obtener doctores con detalles");
            }
        }

        public async Task<OperationResult<DoctorDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<DoctorDto>.Fallo("El ID del doctor es inválido");

                var doctor = await _repository.GetByIdWithDetailsAsync(id);
                if (doctor == null)
                    return OperationResult<DoctorDto>.Fallo("Doctor no encontrado");

                return OperationResult<DoctorDto>.Exito(
                    MapToDtoWithDetails(doctor),
                    "Doctor obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctor {Id}", id);
                return OperationResult<DoctorDto>.Fallo("Error al obtener doctor");
            }
        }

        public async Task<OperationResult<DoctorDto>> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<DoctorDto>.Fallo("El ID del doctor es inválido");

                var doctor = await _repository.GetByIdWithDetailsAsync(id);
                if (doctor == null)
                    return OperationResult<DoctorDto>.Fallo("Doctor no encontrado");

                return OperationResult<DoctorDto>.Exito(
                    MapToDtoWithDetails(doctor),
                    "Doctor con detalles obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctor con detalles {Id}", id);
                return OperationResult<DoctorDto>.Fallo("Error al obtener doctor con detalles");
            }
        }

        public async Task<OperationResult<List<AppointmentDto>>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            try
            {
                if (doctorId <= 0)
                    return OperationResult<List<AppointmentDto>>.Fallo("El ID del doctor es inválido");

                var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
                var appointmentDtos = appointments.Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    AppointmentDate = a.AppointmentDate,
                    StatusId = a.StatusId,
                    CreatedAt = a.CreatedAt
                }).ToList();

                return OperationResult<List<AppointmentDto>>.Exito(
                    appointmentDtos,
                    "Citas del doctor obtenidas correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener citas del doctor {DoctorId}", doctorId);
                return OperationResult<List<AppointmentDto>>.Fallo("Error al obtener citas del doctor");
            }
        }

        public async Task<OperationResult<List<DoctorDto>>> GetBySpecialtyIdAsync(short specialtyId)
        {
            try
            {
                if (specialtyId <= 0)
                    return OperationResult<List<DoctorDto>>.Fallo("El ID de la especialidad es inválido");

                var doctors = await _repository.GetBySpecialtyIdAsync(specialtyId);
                return OperationResult<List<DoctorDto>>.Exito(
                    doctors.Select(MapToDtoWithDetails).ToList(),
                    "Doctores obtenidos correctamente por especialidad"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctores por especialidad {Id}", specialtyId);
                return OperationResult<List<DoctorDto>>.Fallo("Error al obtener doctores por especialidad");
            }
        }

        public async Task<OperationResult<List<DoctorDto>>> GetActiveDoctorsAsync()
        {
            try
            {
                var doctors = await _repository.GetActiveDoctorsAsync();
                return OperationResult<List<DoctorDto>>.Exito(
                    doctors.Select(MapToDtoWithDetails).ToList(),
                    "Doctores activos obtenidos correctamente"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctores activos");
                return OperationResult<List<DoctorDto>>.Fallo("Error al obtener doctores activos");
            }
        }

        public async Task<OperationResult<DoctorDto>> GetByLicenseNumberAsync(string licenseNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseNumber))
                    return OperationResult<DoctorDto>.Fallo("El número de licencia es requerido");

                var doctor = await _repository.GetByLicenseNumberAsync(licenseNumber);
                if (doctor == null)
                    return OperationResult<DoctorDto>.Fallo("Doctor no encontrado");

                return OperationResult<DoctorDto>.Exito(
                    MapToDtoWithDetails(doctor),
                    "Doctor obtenido correctamente por número de licencia"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener doctor por número de licencia {License}", licenseNumber);
                return OperationResult<DoctorDto>.Fallo("Error al obtener doctor por número de licencia");
            }
        }

        public async Task<OperationResult<bool>> ExistsByLicenseNumberAsync(string licenseNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licenseNumber))
                    return OperationResult<bool>.Fallo("El número de licencia es requerido");

                var exists = await _repository.ExistsByLicenseNumberAsync(licenseNumber);
                return OperationResult<bool>.Exito(exists, "Verificación completada");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar licencia {License}", licenseNumber);
                return OperationResult<bool>.Fallo("Error al verificar doctor");
            }
        }

        // private mapping
        private static DoctorDto MapToDtoWithDetails(Doctor d) => new()
        {
            FirstName = d.DoctorNavigation?.FirstName ?? string.Empty,
            LastName = d.DoctorNavigation?.LastName ?? string.Empty,
            IdentificationNumber = d.DoctorNavigation?.IdentificationNumber ?? string.Empty,
            DateOfBirth = d.DoctorNavigation?.DateOfBirth,
            Gender = d.DoctorNavigation?.Gender ?? string.Empty,
            Email = d.DoctorNavigation?.User?.Email ?? string.Empty,
            DoctorId = d.DoctorId,
            SpecialtyId = d.SpecialtyId,
            SpecialtyName = d.Specialty?.SpecialtyName ?? string.Empty,
            LicenseNumber = d.LicenseNumber,
            PhoneNumber = d.PhoneNumber,
            YearsOfExperience = d.YearsOfExperience,
            Education = d.Education,
            Bio = d.Bio,
            ConsultationFee = d.ConsultationFee,
            ClinicAddress = d.ClinicAddress,
            AvailabilityMode = d.AvailabilityModeId?.ToString() ?? string.Empty,
            LicenseExpirationDate = d.LicenseExpirationDate,
            IsActive = d.IsActive
        };
    }
}

