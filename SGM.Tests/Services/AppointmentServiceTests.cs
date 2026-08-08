using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly Mock<IPatientRepository> _patientRepositoryMock;
        private readonly Mock<IDoctorRepository> _doctorRepositoryMock;
        private readonly Mock<IDoctorAvailabilityRepository> _availabilityRepositoryMock;
        private readonly Mock<IAppointmentNotificationService> _notificationServiceMock;
        private readonly Mock<IReminderService> _reminderServiceMock;
        private readonly Mock<ILogger<AppointmentService>> _loggerMock;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _patientRepositoryMock = new Mock<IPatientRepository>();
            _doctorRepositoryMock = new Mock<IDoctorRepository>();
            _availabilityRepositoryMock = new Mock<IDoctorAvailabilityRepository>();
            _notificationServiceMock = new Mock<IAppointmentNotificationService>();
            _reminderServiceMock = new Mock<IReminderService>();
            _loggerMock = new Mock<ILogger<AppointmentService>>();

            _service = new AppointmentService(
                _repositoryMock.Object,
                _patientRepositoryMock.Object,
                _doctorRepositoryMock.Object,
                _availabilityRepositoryMock.Object,
                _notificationServiceMock.Object,
                _reminderServiceMock.Object,
                _loggerMock.Object
            );
        }

        // TEST 1
        // CreateAsync debe fallar si el paciente no existe en la BD
        [Fact]
        public async Task CreateAsync_CuandoPacienteNoExiste_DebeRetornarFallo()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                AppointmentDate = DateTime.Now.AddDays(1),
                StatusId = 1
            };

            _patientRepositoryMock
                .Setup(r => r.ExistsAsync(dto.PatientId))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.CreateAsync(dto);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El paciente no existe.");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
        }

        // TEST 2
        // CreateAsync debe fallar si existe un conflicto de horario para el doctor
        [Fact]
        public async Task CreateAsync_CuandoExisteConflictoDeHorario_DebeRetornarFallo()
        {
            // Arrange
            var dto = new CreateAppointmentDto
            {
                PatientId = 1,
                DoctorId = 1,
                AppointmentDate = DateTime.Now.AddDays(1),
                StatusId = 1
            };

            _patientRepositoryMock
                .Setup(r => r.ExistsAsync(dto.PatientId))
                .ReturnsAsync(true);

            _doctorRepositoryMock
                .Setup(r => r.ExistsAsync(d => d.DoctorId == dto.DoctorId))
                .ReturnsAsync(true);

            // El médico sí tiene disponibilidad configurada; el conflicto viene
            // de que ya existe otra cita en ese slot exacto
            _availabilityRepositoryMock
                .Setup(a => a.IsAvailableAsync(dto.DoctorId, It.IsAny<DateOnly>(), It.IsAny<TimeOnly>()))
                .ReturnsAsync(true);

            _repositoryMock
                .Setup(r => r.ExistsInTimeSlotAsync(dto.DoctorId, dto.AppointmentDate))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.CreateAsync(dto);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Contain("ya no está disponible");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Never);
        }

        // TEST 3
        // CancelAsync debe fallar cuando el ID de la cita es inválido
        [Fact]
        public async Task CancelAsync_CuandoIdEsInvalido_DebeRetornarFallo()
        {
            // Arrange
            int idInvalido = 0;

            // Act
            var resultado = await _service.CancelAsync(idInvalido);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El ID de la cita es inválido.");

            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        // TEST 4
        // RescheduleAsync debe fallar si la cita ya está cancelada
        [Fact]
        public async Task RescheduleAsync_CuandoCitaEstaCancelada_DebeRetornarFallo()
        {
            // Arrange
            int appointmentId = 1;
            var nuevaFecha = DateTime.Now.AddDays(3);

            var citaCancelada = new Appointment
            {
                AppointmentId = appointmentId,
                PatientId = 1,
                DoctorId = 1,
                AppointmentDate = DateTime.Now.AddDays(1),
                StatusId = 3 // 3 = cancelada
            };

            // RescheduleAsync ahora consulta GetByIdWithDetailsAsync (Task 140),
            // no GetByIdAsync
            _repositoryMock
                .Setup(r => r.GetByIdWithDetailsAsync(appointmentId))
                .ReturnsAsync(citaCancelada);

            // Act
            var resultado = await _service.RescheduleAsync(appointmentId, nuevaFecha);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Contain("Pendiente o Confirmada");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }

        // TEST 5
        // GetByDateRangeAsync debe fallar si la fecha fin es menor que la fecha inicio
        [Fact]
        public async Task GetByDateRangeAsync_CuandoRangoEsInvalido_DebeRetornarFallo()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddDays(5);
            var fechaFin = DateTime.Now.AddDays(1); // fin antes que inicio

            // Act
            var resultado = await _service.GetByDateRangeAsync(fechaInicio, fechaFin);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El rango de fechas es inválido.");

            _repositoryMock.Verify(r => r.GetByDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        }
    }
}