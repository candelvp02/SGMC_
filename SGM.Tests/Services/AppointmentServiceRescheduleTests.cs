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
    public class AppointmentServiceRescheduleTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock = new();
        private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
        private readonly Mock<IDoctorAvailabilityRepository> _availabilityRepositoryMock = new();
        private readonly Mock<IAppointmentNotificationService> _notificationServiceMock = new();
        private readonly Mock<ILogger<AppointmentService>> _loggerMock = new();

        private AppointmentService CreateService() => new(
            _repositoryMock.Object,
            _patientRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _availabilityRepositoryMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        private static Appointment CrearCitaBase(int statusId = 1) => new()
        {
            AppointmentId = 1,
            PatientId = 10,
            DoctorId = 20,
            StatusId = statusId,
            AppointmentDate = DateTime.Today.AddDays(1).AddHours(10), // 10:00 AM, fecha fija
            CreatedAt = DateTime.Now.AddDays(-2)
        };

        [Fact]
        public async Task RescheduleAsync_DebeFallar_CuandoNuevoHorarioNoEstaDisponible()
        {
            // Arrange
            var cita = CrearCitaBase();
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.IsAvailableAsync(cita.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(false); // el médico no tiene ese bloque disponible

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("no tiene disponibilidad");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task RescheduleAsync_DebeBloquear_CuandoElNuevoSlotYaTieneOtraCita()
        {
            // Arrange
            var cita = CrearCitaBase();
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.IsAvailableAsync(cita.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.ExistsInTimeSlotAsync(cita.DoctorId, nuevaFecha))
                .ReturnsAsync(true); // otra cita ya ocupa ese horario

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("ya no está disponible");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Theory]
        [InlineData(3)] // Cancelada
        [InlineData(4)] // Completada
        public async Task RescheduleAsync_DebeFallar_SiEstadoNoEsPendienteNiConfirmada(int statusId)
        {
            // Arrange
            var cita = CrearCitaBase(statusId);
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("Pendiente o Confirmada");
            _availabilityRepositoryMock.Verify(
                a => a.IsAvailableAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()),
                Times.Never); // ni siquiera debe llegar a verificar disponibilidad
        }

        [Fact]
        public async Task RescheduleAsync_DebeBloquear_CuandoOtroPacienteTomaElHorarioEnLaVerificacionUltramicro()
        {
            // Arrange: simula que el slot estaba libre en la primera verificación
            // pero alguien lo tomó justo antes del guardado (condición de carrera)
            var cita = CrearCitaBase();
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);

            _availabilityRepositoryMock
                .SetupSequence(a => a.IsAvailableAsync(cita.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(true)   // primera verificación: disponible
                .ReturnsAsync(false); // verificación ultramicro: ya no disponible

            _repositoryMock
                .Setup(r => r.ExistsInTimeSlotAsync(cita.DoctorId, nuevaFecha))
                .ReturnsAsync(false); // sin conflicto de citas en ambas verificaciones

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("tomado por otro paciente");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task RescheduleAsync_DebeActualizarEstadoAPendiente_YLiberarHorarioAnterior_CuandoTodoEsValido()
        {
            // Arrange
            var cita = CrearCitaBase(statusId: 2); // estaba Confirmada
            var fechaAnterior = cita.AppointmentDate;
            var nuevaFecha = DateTime.Now.AddDays(3).Date.AddHours(10);

            var slotAnterior = new DoctorAvailability
            {
                AvailabilityId = 5,
                DoctorId = cita.DoctorId,
                AvailableDate = DateOnly.FromDateTime(fechaAnterior),
                StartTime = TimeOnly.FromDateTime(fechaAnterior).AddHours(-1),
                EndTime = TimeOnly.FromDateTime(fechaAnterior).AddHours(1),
                IsActive = false // estaba ocupado por la cita original
            };

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.IsAvailableAsync(cita.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.ExistsInTimeSlotAsync(cita.DoctorId, nuevaFecha))
                .ReturnsAsync(false);
            _availabilityRepositoryMock
                .Setup(a => a.GetByDoctorAndDateRangeAsync(
                    cita.DoctorId, DateOnly.FromDateTime(fechaAnterior), DateOnly.FromDateTime(fechaAnterior)))
                .ReturnsAsync(new List<DoctorAvailability> { slotAnterior });

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeTrue();
            cita.StatusId.Should().Be(1); // vuelve a Pendiente
            cita.AppointmentDate.Should().Be(nuevaFecha);
            cita.UpdatedAt.Should().NotBeNull();

            slotAnterior.IsActive.Should().BeTrue(); // horario anterior liberado

            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
            _availabilityRepositoryMock.Verify(a => a.UpdateAsync(slotAnterior), Times.Once);
            _notificationServiceMock.Verify(
                n => n.NotifyAppointmentRescheduledAsync(cita, fechaAnterior), Times.Once);
        }

        [Fact]
        public async Task RescheduleAsync_DebeCompletarseExitosamente_AunSiFallaElEnvioDeCorreo()
        {
            // Arrange: la notificación falla, pero la reprogramación debe quedar guardada igual
            var cita = CrearCitaBase();
            var nuevaFecha = DateTime.Now.AddDays(3);

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId))
                .ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.IsAvailableAsync(cita.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(true);
            _repositoryMock
                .Setup(r => r.ExistsInTimeSlotAsync(cita.DoctorId, nuevaFecha))
                .ReturnsAsync(false);
            _availabilityRepositoryMock
                .Setup(a => a.GetByDoctorAndDateRangeAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<DoctorAvailability>());
            _notificationServiceMock
                .Setup(n => n.NotifyAppointmentRescheduledAsync(It.IsAny<Appointment>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new InvalidOperationException("SMTP no disponible"));

            var service = CreateService();

            // Act
            var result = await service.RescheduleAsync(cita.AppointmentId, nuevaFecha);

            // Assert
            result.Exitoso.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
        }
    }
}