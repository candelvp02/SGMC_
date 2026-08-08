using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Application.Tests.Services
{
    public class AppointmentServicePastDateValidationTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock = new();
        private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IDoctorAvailabilityRepository> _availabilityRepositoryMock = new();
        private readonly Mock<IAppointmentNotificationService> _notificationServiceMock = new();
        private readonly Mock<IReminderService> _reminderServiceMock = new();
        private readonly Mock<ILogger<AppointmentService>> _loggerMock = new();

        private AppointmentService CreateService() => new(
            _repositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _availabilityRepositoryMock.Object,
            _notificationServiceMock.Object,
            _reminderServiceMock.Object,
            _loggerMock.Object);

        private static Appointment CrearCitaPasada(int statusId = 1) => new()
        {
            AppointmentId = 1,
            PatientId = 10,
            DoctorId = 20,
            StatusId = statusId,
            AppointmentDate = DateTime.Now.AddDays(-2), // ya paso
            CreatedAt = DateTime.Now.AddDays(-5)
        };

        [Theory]
        [InlineData(1)] // Pendiente
        [InlineData(2)] // Confirmada
        public async Task CancelAsync_DebeFallar_CuandoLaCitaYaPaso(int statusId)
        {
            // Arrange
            var cita = CrearCitaPasada(statusId);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);

            var service = CreateService();

            // Act
            var result = await service.CancelAsync(cita.AppointmentId);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("ya pasaron");
            cita.StatusId.Should().Be(statusId); // el estado no debe cambiar
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Theory]
        [InlineData(1)] // Pendiente
        [InlineData(2)] // Confirmada
        public async Task RescheduleAsync_DebeFallar_CuandoLaCitaOriginalYaPaso(int statusId)
        {
            // Arrange
            var cita = CrearCitaPasada(statusId);
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("ya pasaron");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
            _availabilityRepositoryMock.Verify(
                a => a.IsAvailableAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()),
                Times.Never); // ni siquiera debe llegar a verificar disponibilidad
        }

        [Fact]
        public async Task RescheduleAsync_DebeFallar_CuandoLaNuevaFechaEsEnElPasado()
        {
            // Arrange: valida la regla ya existente (Paso 1 del flujo) como caso de regresion
            var appointmentId = 1;
            var fechaPasada = DateTime.Now.AddDays(-1);

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(appointmentId, fechaPasada);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("futuras");
            _repositoryMock.Verify(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()), Times.Never);
        }
    }
}