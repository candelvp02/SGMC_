namespace SGMC.Application.Interfaces.Service
{
    // Nota: Task 108 queda pendiente hasta que conecten un SMTP local (smtp4dev/Papercut/MailHog).
    // Solo faltaría una nueva implementación de esta interfaz para concluirlo una vez que tengan el SMTP configurado.
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}