using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;
using SGMC.Application.Services;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Dto.Medical;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Users;
using System.Linq.Expressions;

namespace HealtSync_doctores.Tests
{
    public class DoctorModuleServicesTests
    {
        [Fact]
        public async Task CreateAvailabilityAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            var mockRepo = new Mock<IDoctorAvailabilityRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();
            var mockLogger = new Mock<ILogger<AvailabilityService>>();

            // Inicializa solo las propiedades públicas
            var dto = new CreateAvailabilityDto
            {
                DoctorId = 1,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                Date = DateTime.Today
            };

            // Corrección: Usa Expression<Func<Doctor, bool>> en lugar de Func<Doctor, bool>
            mockDoctorRepo
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(true);

            mockRepo
                .Setup(r => r.CheckForConflictAsync(dto.DoctorId, 1, dto.StartTime, dto.EndTime))
                .ReturnsAsync(false);

            var service = new AvailabilityService(mockRepo.Object, mockDoctorRepo.Object, mockLogger.Object);

            var result = await service.CreateAsync(dto);

            Assert.True(result.Exitoso);
        }

        [Fact]
        public async Task CreateAvailabilityAsync_ShouldFail_WhenDoctorDoesNotExist()
        {
            var mockRepo = new Mock<IDoctorAvailabilityRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();
            var mockLogger = new Mock<ILogger<AvailabilityService>>();

            var dto = new CreateAvailabilityDto
            {
                DoctorId = 99,
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(12, 0, 0),
                Date = DateTime.Today
            };

            mockDoctorRepo
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Doctor, bool>>>()))
                .ReturnsAsync(false);

            mockRepo
                .Setup(r => r.CheckForConflictAsync(dto.DoctorId, 1, dto.StartTime, dto.EndTime))
                .ReturnsAsync(false);

            var service = new AvailabilityService(mockRepo.Object, mockDoctorRepo.Object, mockLogger.Object);

            var result = await service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("doctor no existe", result.Mensaje, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void MedicalRecordValidator_ShouldFail_WhenDiagnosisIsEmpty()
        {
            var dto = new CreateMedicalRecordDto
            {
                PatientId = 1,
                DoctorId = 1,
                Diagnosis = "",
                Treatment = "Tratamiento",
                RecordDate = DateTime.Now
            };

            var result = SGMC.Application.Validators.Medical.MedicalRecordValidator.IsValidDto(dto);

            Assert.False(result.Exitoso);
            Assert.Contains(result.Errores, e => e.Contains("diagnóstico"));
        }

        [Fact]
        public async Task GetByDoctorIdAsync_ShouldReturnAppointmentsList()
        {
            var mockRepo = new Mock<IAppointmentRepository>();
            var mockPatientRepo = new Mock<SGMC.Domain.Repositories.Users.IPatientRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();
            var mockLogger = new Mock<ILogger<SGMC.Application.Services.AppointmentService>>();

            var appointments = new List<Appointment>
                        {
                            new Appointment { AppointmentId = 1, DoctorId = 1, PatientId = 2, AppointmentDate = DateTime.Now, StatusId = 1, CreatedAt = DateTime.Now }
                        };

            mockRepo
                .Setup(r => r.GetByDoctorIdWithDetailsAsync(1))
                .ReturnsAsync(appointments);

            var service = new SGMC.Application.Services.AppointmentService(
                mockRepo.Object, mockPatientRepo.Object, mockDoctorRepo.Object, mockLogger.Object);

            var result = await service.GetByDoctorIdAsync(1);

            Assert.True(result.Exitoso);
            Assert.NotNull(result.Datos);
            Assert.Single(result.Datos);
        }
    }
}