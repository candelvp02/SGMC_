using SGMC.Application.Dto.System;
using SGMC.Domain.Base;

namespace SGMC.Application.Interfaces.Service
{
    public interface INotificationService
    {
        Task<OperationResult> SendNotificationAsync(NotificationDto dto);
        Task<OperationResult<List<NotificationDto>>> GetPendingNotificationsAsync(int userId);
        Task<OperationResult> MarkAsReadAsync(int notificationId);
        Task<OperationResult> SendPasswordResetEmailAsync(string recipientEmail, int userId);
        Task<OperationResult> SendAppointmentConfirmationAsync(int appointmentId);

        Task<OperationResult> SendAccountActivationEmailAsync(string recipientEmail, int userId);
    }
}