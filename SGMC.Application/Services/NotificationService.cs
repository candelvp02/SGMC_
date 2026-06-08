using Microsoft.Extensions.Logging;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Validators.System;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Repositories.System;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository repository,
            IUserRepository userRepository,
            ILogger<NotificationService> logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _logger = logger;
        }

        // metodos de notificacion

        public async Task<OperationResult> SendNotificationAsync(NotificationDto dto)
        {
            if (dto is null) return OperationResult.Fallo("Datos de notificación requeridos.");

            // validaciones de campo fuera de trycatch
            var validation = dto.IsValidDto();
            if (!validation.Exitoso) return validation;

            try
            {
                // validación de negocio
                var userExists = await _userRepository.ExistsAsync(dto.RecipientId);
                if (!userExists) return OperationResult.Fallo("El destinatario no existe.");

                // create entity
                var notification = new Notification
                {
                    RecipientId = dto.RecipientId,
                    Title = dto.Title!,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                // guardar entidad
                await _repository.AddAsync(notification);

                _logger.LogInformation("Notificación creada para usuario {Id}", dto.RecipientId);

                // Logica de envío de email/push real iría aquí

                return OperationResult.Exito("Notificación enviada y registrada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación para usuario {Id}", dto.RecipientId);
                return OperationResult.Fallo("Error interno al enviar notificación.");
            }
        }

        public async Task<OperationResult<List<NotificationDto>>> GetPendingNotificationsAsync(int userId)
        {
            if (userId <= 0) return OperationResult<List<NotificationDto>>.Fallo("ID de usuario inválido.");

            try
            {
                var notifications = await _repository.GetPendingByUserIdAsync(userId);
                var dtoList = notifications.Select(MapToDto).ToList();

                return OperationResult<List<NotificationDto>>.Exito(dtoList, "Notificaciones pendientes obtenidas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notificaciones pendientes de usuario {Id}", userId);
                return OperationResult<List<NotificationDto>>.Fallo("Error al obtener notificaciones.");
            }
        }

        public async Task<OperationResult> MarkAsReadAsync(int notificationId)
        {
            if (notificationId <= 0) return OperationResult.Fallo("ID de notificación inválido.");

            try
            {
                var notification = await _repository.GetByIdAsync(notificationId);
                if (notification is null) return OperationResult.Fallo("Notificación no encontrada.");

                if (notification.IsRead) return OperationResult.Exito("Notificación ya marcada como leída.");

                notification.IsRead = true;
                notification.UpdatedAt = DateTime.Now;

                await _repository.UpdateAsync(notification);

                return OperationResult.Exito("Notificación marcada como leída.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como leída la notificación {Id}", notificationId);
                return OperationResult.Fallo("Error al marcar como leída.");
            }
        }

        // Metodos especificos de negocio

        public Task<OperationResult> SendPasswordResetEmailAsync(string recipientEmail, int userId)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return Task.FromResult(OperationResult.Fallo("Email requerido."));

            _logger.LogWarning("SIMULACIÓN: Email de restablecimiento de contraseña enviado a {Email} (User ID: {UserId}).", recipientEmail, userId);

            return Task.FromResult(OperationResult.Exito($"Email de restablecimiento enviado a {recipientEmail}."));
        }

        public Task<OperationResult> SendAppointmentConfirmationAsync(int appointmentId)
        {
            if (appointmentId <= 0) return Task.FromResult(OperationResult.Fallo("ID de cita inválido."));

            _logger.LogInformation("SIMULACIÓN: Email de confirmación de cita {Id} enviado.", appointmentId);

            return Task.FromResult(OperationResult.Exito($"Email de confirmación para cita {appointmentId} enviado."));
        }

        public Task<OperationResult> SendAccountActivationEmailAsync(string recipientEmail, int userId)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                return Task.FromResult(OperationResult.Fallo("Email requerido."));

            // Aquí va la lógica real de envío (En Hold por el momento).
            // Por ahora simula el envío igual que los otros métodos.
            // Confirmár con Candela mas tarde si vamos a aplicar alguna lógica adicional para este caso específico.
            _logger.LogInformation(
                "SIMULACIÓN: Email de activación enviado a {Email} (Patient ID: {UserId}).",
                recipientEmail, userId);

            return Task.FromResult(
                OperationResult.Exito($"Email de activación enviado a {recipientEmail}. La cuenta está pendiente de confirmación."));
        }

        // private mapping

        private static NotificationDto MapToDto(Notification n)
        {
            return new NotificationDto
            {
                NotificationId = n.NotificationId,
                RecipientId = n.RecipientId,
                Title = n.Title,
                IsRead = n.IsRead,
                UpdatedAt = n.UpdatedAt
            };
        }
    }
}