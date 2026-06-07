using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.System;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            INotificationService notificationService,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<OperationResult<UserDto>> AuthenticateAsync(LoginDto dto)
        {
            if (dto == null) return OperationResult<UserDto>.Fallo("Credenciales requeridas.");

            try
            {
                var user = await _userRepository.GetByEmailWithRoleAsync(dto.Email.ToLower().Trim());

                if (user == null || !user.IsActive)
                    return OperationResult<UserDto>.Fallo("Usuario o contraseña inválidos.");

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return OperationResult<UserDto>.Fallo("Usuario o contraseña inválidos.");

                var userDto = MapToDto(user);
                return OperationResult<UserDto>.Exito(userDto, "Autenticación exitosa.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación: {Email}", dto.Email);
                return OperationResult<UserDto>.Fallo("Error del servidor al autenticar.");
            }
        }

        public async Task<OperationResult<UserDto>> RegisterAsync(RegisterUserDto dto)
        {
            if (dto == null) return OperationResult<UserDto>.Fallo("Datos de registro requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<UserDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                if (await _userRepository.ExistsByEmailAsync(dto.Email))
                    return OperationResult<UserDto>.Fallo("El email ya está en uso.");

                if (await _roleRepository.GetByIdAsync(dto.RoleId) == null)
                    return OperationResult<UserDto>.Fallo("El rol especificado no existe.");

                var user = new User
                {
                    Email = dto.Email.ToLower().Trim(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    RoleId = dto.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var createdUser = await _userRepository.AddAsync(user);
                if (createdUser == null)
                    return OperationResult<UserDto>.Fallo("Error al crear usuario.");

                var userDto = MapToDto(createdUser);
                return OperationResult<UserDto>.Exito(userDto, "Usuario registrado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el registro del usuario.");
                return OperationResult<UserDto>.Fallo($"Error del servidor al registrar: {ex.Message}");
            }
        }

        public async Task<OperationResult> RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return OperationResult.Fallo("Email requerido.");

            try
            {
                var user = await _userRepository.GetByEmailAsync(email.ToLower().Trim());
                if (user == null)
                {
                    _logger.LogWarning("Intento de reseteo de contraseña para email no encontrado.");
                    return OperationResult.Exito("Si la cuenta existe, recibirá un correo para restablecer la contraseña.");
                }

                await _notificationService.SendPasswordResetEmailAsync(user.Email, user.UserId);

                return OperationResult.Exito("Si la cuenta existe, recibirá un correo para restablecer la contraseña.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar reseteo de contraseña.");
                return OperationResult.Fallo($"Error al procesar la solicitud: {ex.Message}");
            }
        }

        public async Task<OperationResult> ActivateAccountAsync(int userId)
        {
            if (userId <= 0) return OperationResult.Fallo("ID de usuario inválido.");

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return OperationResult.Fallo("Usuario no encontrado.");

                if (user.IsActive)
                    return OperationResult.Fallo("La cuenta ya está activa.");

                user.IsActive = true;
                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                return OperationResult.Exito("Cuenta activada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al activar cuenta para usuario ID: {Id}", userId);
                return OperationResult.Fallo($"Error al activar la cuenta: {ex.Message}");
            }
        }

        public async Task<OperationResult<UserDto>> UpdateProfileAsync(UpdateUserDto dto)
        {
            if (dto == null) return OperationResult<UserDto>.Fallo("Datos de actualización requeridos.");

            var validationResult = dto.IsValidDto();
            if (!validationResult.Exitoso)
                return OperationResult<UserDto>.Fallo(validationResult.Mensaje, validationResult.Errores);

            try
            {
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                    return OperationResult<UserDto>.Fallo("Usuario no encontrado.");

                if (user.Email.ToLower() != dto.Email.ToLower().Trim())
                {
                    if (await _userRepository.ExistsByEmailAsync(dto.Email))
                        return OperationResult<UserDto>.Fallo("El nuevo email ya está en uso.");

                    user.Email = dto.Email.ToLower().Trim();
                }

                user.RoleId = dto.RoleId;
                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateAsync(user);

                var updatedUser = await _userRepository.GetByIdWithRoleAsync(user.UserId);

                return OperationResult<UserDto>.Exito(
                    MapToDto(updatedUser!),
                    "Perfil actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar perfil de usuario ID: {Id}", dto.UserId);
                return OperationResult<UserDto>.Fallo($"Error al actualizar perfil: {ex.Message}");
            }
        }

        public async Task<OperationResult> ChangePasswordAsync(ChangePasswordDto dto)
        {
            if (dto == null) return OperationResult.Fallo("Datos de cambio de contraseña requeridos.");

            try
            {
                var user = await _userRepository.GetByIdAsync(dto.UserId);
                if (user == null)
                    return OperationResult.Fallo("Usuario no encontrado.");

                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                    return OperationResult.Fallo("La contraseña actual es incorrecta.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateAsync(user);

                return OperationResult.Exito("Contraseña cambiada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña del usuario ID: {Id}", dto.UserId);
                return OperationResult.Fallo($"Error al cambiar la contraseña: {ex.Message}");
            }
        }

        public async Task<OperationResult<UserDto>> GetByIdAsync(int id)
        {
            if (id <= 0) return OperationResult<UserDto>.Fallo("ID de usuario inválido.");

            try
            {
                var user = await _userRepository.GetByIdWithRoleAsync(id);
                if (user == null)
                    return OperationResult<UserDto>.Fallo("Usuario no encontrado.");

                return OperationResult<UserDto>.Exito(
                    MapToDto(user),
                    "Usuario obtenido correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {Id}", id);
                return OperationResult<UserDto>.Fallo($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<OperationResult<UserDto>> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return OperationResult<UserDto>.Fallo("Email requerido.");

            try
            {
                var user = await _userRepository.GetByEmailWithRoleAsync(email.ToLower().Trim());
                if (user == null)
                    return OperationResult<UserDto>.Fallo("Usuario no encontrado.");

                return OperationResult<UserDto>.Exito(
                    MapToDto(user),
                    "Usuario obtenido correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por email: {Email}", email);
                return OperationResult<UserDto>.Fallo($"Error al obtener usuario: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<UserDto>>> GetByRoleAsync(short roleId)
        {
            if (roleId <= 0)
                return OperationResult<List<UserDto>>.Fallo("ID de rol inválido.");

            try
            {
                var users = await _userRepository.GetByRoleIdAsync(roleId);
                var userDtos = users.Select(MapToDto).ToList();
                return OperationResult<List<UserDto>>.Exito(
                    userDtos,
                    $"Usuarios con Rol ID {roleId} obtenidos correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios por rol ID: {Id}", roleId);
                return OperationResult<List<UserDto>>.Fallo($"Error al obtener usuarios por rol: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<UserDto>>> GetActiveUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetActiveAsync();
                var userDtos = users.Select(MapToDto!).ToList();
                return OperationResult<List<UserDto>>.Exito(
                    userDtos,
                    "Usuarios activos obtenidos correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios activos.");
                return OperationResult<List<UserDto>>.Fallo($"Error al obtener usuarios activos: {ex.Message}");
            }
        }

        public async Task<OperationResult<List<UserDto>>> GetAllAsync()
        {
            try
            {
                var users = await _userRepository.GetAllWithRoleAsync();
                var userDtos = users.Select(MapToDto).ToList();
                return OperationResult<List<UserDto>>.Exito(
                    userDtos,
                    "Lista de todos los usuarios obtenida correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios.");
                return OperationResult<List<UserDto>>.Fallo($"Error al obtener usuarios: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            if (id <= 0) return OperationResult.Fallo("ID de usuario inválido.");

            try
            {
                var exists = await _userRepository.ExistsAsync(id);
                if (!exists) return OperationResult.Fallo("Usuario no encontrado.");

                await _userRepository.DeleteAsync(id);

                return OperationResult.Exito("Usuario eliminado físicamente correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario ID: {Id}", id);
                return OperationResult.Fallo($"Error al eliminar usuario: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeactivateAsync(int id)
        {
            if (id <= 0) return OperationResult.Fallo("ID de usuario inválido.");

            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                    return OperationResult.Fallo("Usuario no encontrado.");

                if (!user.IsActive)
                    return OperationResult.Fallo("El usuario ya está inactivo.");

                user.IsActive = false;
                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateAsync(user);
                return OperationResult.Exito("Usuario desactivado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar usuario ID: {Id}", id);
                return OperationResult.Fallo($"Error al desactivar usuario: {ex.Message}");
            }
        }

        private static UserDto MapToDto(User user)
        {
            if (user == default)
                throw new ArgumentNullException(nameof(user), "El usuario no puede ser nulo al mapear a UserDto.");

            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "N/A",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
