using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.Services
{
    public class AppointmentServiceConfirmRejectTests
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
            CreatedAt = DateTime.Now.AddDays(-1)
        };

        // ── CONFIRM ─────────────────────────────────────────────────────────

        [Fact]
        public async Task ConfirmAsync_DebeCambiarEstadoAConfirmada_YNotificarAlPaciente()
        {
            var cita = CrearCitaBase(statusId: 1);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);

            var service = CreateService();

            var result = await service.ConfirmAsync(cita.AppointmentId);

            result.Exitoso.Should().BeTrue();
            cita.StatusId.Should().Be(2); // Confirmada
            cita.UpdatedAt.Should().NotBeNull();

            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
            _notificationServiceMock.Verify(n => n.NotifyAppointmentConfirmedAsync(cita), Times.Once);
        }

        [Theory]
        [InlineData(2)] // ya confirmada
        [InlineData(3)] // cancelada
        [InlineData(4)] // completada
        public async Task ConfirmAsync_DebeFallar_SiLaCitaNoEstaPendiente(int statusId)
        {
            var cita = CrearCitaBase(statusId);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);

            var service = CreateService();

            var result = await service.ConfirmAsync(cita.AppointmentId);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("Pendiente");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
            _notificationServiceMock.Verify(
                n => n.NotifyAppointmentConfirmedAsync(It.IsAny<Appointment>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmAsync_DebeFallar_CuandoLaCitaNoExiste()
        {
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Appointment?)null);

            var service = CreateService();

            var result = await service.ConfirmAsync(99);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("La cita no existe.");
        }

        [Fact]
        public async Task ConfirmAsync_DebeCompletarseExitosamente_AunSiFallaElEnvioDeCorreo()
        {
            var cita = CrearCitaBase(statusId: 1);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);
            _notificationServiceMock
                .Setup(n => n.NotifyAppointmentConfirmedAsync(It.IsAny<Appointment>()))
                .ThrowsAsync(new InvalidOperationException("SMTP no disponible"));

            var service = CreateService();

            var result = await service.ConfirmAsync(cita.AppointmentId);

            result.Exitoso.Should().BeTrue();
            cita.StatusId.Should().Be(2);
            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
        }

        // ── REJECT ──────────────────────────────────────────────────────────

        [Fact]
        public async Task RejectAsync_DebeCambiarEstadoACancelada_LiberarHorario_YNotificar()
        {
            var cita = CrearCitaBase(statusId: 1);

            var slot = new DoctorAvailability
            {
                AvailabilityId = 5,
                DoctorId = cita.DoctorId,
                AvailableDate = DateOnly.FromDateTime(cita.AppointmentDate),
                StartTime = TimeOnly.FromDateTime(cita.AppointmentDate).AddHours(-1),
                EndTime = TimeOnly.FromDateTime(cita.AppointmentDate).AddHours(1),
                IsActive = false
            };

            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.GetByDoctorAndDateRangeAsync(
                    cita.DoctorId,
                    DateOnly.FromDateTime(cita.AppointmentDate),
                    DateOnly.FromDateTime(cita.AppointmentDate)))
                .ReturnsAsync(new List<DoctorAvailability> { slot });

            var service = CreateService();

            var result = await service.RejectAsync(cita.AppointmentId);

            result.Exitoso.Should().BeTrue();
            cita.StatusId.Should().Be(3); // Cancelada
            cita.UpdatedAt.Should().NotBeNull();
            slot.IsActive.Should().BeTrue(); // horario liberado

            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
            _availabilityRepositoryMock.Verify(a => a.UpdateAsync(slot), Times.Once);
            _notificationServiceMock.Verify(n => n.NotifyAppointmentRejectedAsync(cita), Times.Once);
        }

        [Theory]
        [InlineData(2)] // ya confirmada, no se puede "rechazar" una solicitud que ya no es pendiente
        [InlineData(3)] // ya cancelada
        [InlineData(4)] // completada
        public async Task RejectAsync_DebeFallar_SiLaSolicitudNoEstaPendiente(int statusId)
        {
            var cita = CrearCitaBase(statusId);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);

            var service = CreateService();

            var result = await service.RejectAsync(cita.AppointmentId);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Contain("Pendiente");
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
            _availabilityRepositoryMock.Verify(
                a => a.UpdateAsync(It.IsAny<DoctorAvailability>()), Times.Never);
        }

        [Fact]
        public async Task RejectAsync_DebeFallar_CuandoLaCitaNoExiste()
        {
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Appointment?)null);

            var service = CreateService();

            var result = await service.RejectAsync(99);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.Should().Be("La cita no existe.");
        }

        [Fact]
        public async Task RejectAsync_DebeCompletarseExitosamente_AunSiFallaElEnvioDeCorreo()
        {
            var cita = CrearCitaBase(statusId: 1);
            _repositoryMock.Setup(r => r.GetByIdWithDetailsAsync(cita.AppointmentId)).ReturnsAsync(cita);
            _availabilityRepositoryMock
                .Setup(a => a.GetByDoctorAndDateRangeAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<DoctorAvailability>());
            _notificationServiceMock
                .Setup(n => n.NotifyAppointmentRejectedAsync(It.IsAny<Appointment>()))
                .ThrowsAsync(new InvalidOperationException("SMTP no disponible"));

            var service = CreateService();

            var result = await service.RejectAsync(cita.AppointmentId);

            result.Exitoso.Should().BeTrue();
            _repositoryMock.Verify(r => r.UpdateAsync(cita), Times.Once);
        }

    }
}