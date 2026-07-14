using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Medical;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Tests.Services
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _repoMock;
        private readonly Mock<IAppointmentRepository> _apptRepoMock;
        private readonly Mock<IDoctorAvailabilityRepository> _availabilityRepoMock;
        private readonly Mock<ILogger<DoctorService>> _loggerMock;
        private readonly IDoctorService _service;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPersonRepository> _personRepoMock;
        private readonly Mock<ISpecialtyRepository> _specialtyRepoMock;

        public DoctorServiceTests()
        {
            _repoMock = new Mock<IDoctorRepository>();
            _apptRepoMock = new Mock<IAppointmentRepository>();
            _availabilityRepoMock = new Mock<IDoctorAvailabilityRepository>();
            _loggerMock = new Mock<ILogger<DoctorService>>();
            _userRepoMock = new Mock<IUserRepository>();
            _personRepoMock = new Mock<IPersonRepository>();
            _specialtyRepoMock = new Mock<ISpecialtyRepository>();

            _service = new DoctorService(
                _repoMock.Object,
                _apptRepoMock.Object,
                _availabilityRepoMock.Object,
                _loggerMock.Object,
                _userRepoMock.Object,
                _personRepoMock.Object,
                _specialtyRepoMock.Object);
        }

        private static RegisterDoctorDto GetValidRegisterDto(string licenseNumber, DateOnly licenseExpiration)
        {
            return new RegisterDoctorDto
            {
                FirstName = "Juan",
                LastName = "Perez",
                IdentificationNumber = "001-0000001-1",
                DateOfBirth = new DateOnly(1985, 1, 1),
                Gender = "Masculino",
                Email = $"test-{Guid.NewGuid()}@doctor.com",
                Password = "ValidPassword123",
                PhoneNumber = "809-555-1234",
                SpecialtyId = 1,
                LicenseNumber = licenseNumber,
                LicenseExpirationDate = licenseExpiration,
                YearsOfExperience = 10,
                Education = "MD, University of Health Sciences",
                Bio = "Experienced cardiologist."
            };
        }

        private static Doctor GetValidDoctorEntity(int id = 1, short specialtyId = 1, bool isActive = true)
        {
            return new Doctor
            {
                DoctorId = id,
                SpecialtyId = specialtyId,
                LicenseNumber = "L-0001",
                PhoneNumber = "809-555-0000",
                YearsOfExperience = 5,
                Education = "MD",
                Bio = "Bio",
                ClinicAddress = "Address",
                LicenseExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                IsActive = isActive,
                CreatedAt = DateTime.Now
            };
        }

        // ---------- CreateAsync ----------

        [Fact]
        public async Task CreateAsync_WhenLicenseNumberEmpty_ReturnsFailure()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            var dto = GetValidRegisterDto(string.Empty, futureDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(true);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
        }

        [Fact]
        public async Task CreateAsync_WhenEmailAlreadyExists_ReturnsFailure()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            var dto = GetValidRegisterDto("L-VALID", futureDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(true);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("email", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task CreateAsync_WhenSpecialtyDoesNotExist_ReturnsFailure()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            var dto = GetValidRegisterDto("L-VALID2", futureDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(false);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("especialidad", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task CreateAsync_WhenValid_ReturnsSuccess()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            var dto = GetValidRegisterDto("L-VALID3", futureDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(true);

            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.UserId = 100; return u; });

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Doctor>()))
                .ReturnsAsync((Doctor d) => d);

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Exitoso);
            Assert.NotNull(result.Datos);
            Assert.Equal(dto.SpecialtyId, result.Datos!.SpecialtyId);
        }

        // ---------- UpdateAsync ----------

        [Fact]
        public async Task UpdateAsync_WhenDoctorNotFound_ReturnsFailure()
        {
            var dto = new UpdateDoctorDto { DoctorId = 1, SpecialtyId = 1, PhoneNumber = "809-000-0000", Education = "MD", LicenseExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Doctor?)null);

            var result = await _service.UpdateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("no encontrado", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_WhenSpecialtyDoesNotExist_ReturnsFailure()
        {
            var dto = new UpdateDoctorDto { DoctorId = 1, SpecialtyId = 99, PhoneNumber = "809-000-0000", Education = "MD", LicenseExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)) };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(GetValidDoctorEntity());
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(false);

            var result = await _service.UpdateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("especialidad", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_WhenValid_UpdatesSpecialtyAndIsActive()
        {
            var doctor = GetValidDoctorEntity(id: 1, specialtyId: 1, isActive: true);
            var dto = new UpdateDoctorDto
            {
                DoctorId = 1,
                SpecialtyId = 2,
                PhoneNumber = "809-111-2222",
                YearsOfExperience = 8,
                Education = "MD Actualizado",
                LicenseExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(2)),
                IsActive = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(It.IsAny<short>())).ReturnsAsync(true);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Doctor>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(doctor);

            var result = await _service.UpdateAsync(dto);

            Assert.True(result.Exitoso);
            Assert.Equal((short)2, doctor.SpecialtyId);
            Assert.False(doctor.IsActive);
        }

        // ---------- DeleteAsync ----------

        [Fact]
        public async Task DeleteAsync_WhenDoctorHasFutureAppointments_ReturnsFailure()
        {
            var doctor = GetValidDoctorEntity();
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
            _apptRepoMock.Setup(r => r.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment>
            {
                new Appointment { AppointmentId = 1, DoctorId = 1, AppointmentDate = DateTime.Now.AddDays(3), StatusId = 1 }
            });

            var result = await _service.DeleteAsync(1);

            Assert.False(result.Exitoso);
            Assert.Contains("citas futuras", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task DeleteAsync_WhenValid_ReturnsSuccess()
        {
            var doctor = GetValidDoctorEntity();
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
            _apptRepoMock.Setup(r => r.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment>());
            _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Doctor>())).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(1);

            Assert.True(result.Exitoso);
            Assert.False(doctor.IsActive);
        }

        // ---------- SearchAsync ----------

        [Fact]
        public async Task SearchAsync_ReturnsMatchingDoctors()
        {
            var doctors = new List<Doctor> { GetValidDoctorEntity(id: 1), GetValidDoctorEntity(id: 2) };

            _repoMock.Setup(r => r.SearchAsync("Juan", (short?)1)).ReturnsAsync(doctors);
            _availabilityRepoMock
                .Setup(r => r.GetByDoctorAndDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(new List<DoctorAvailability>());

            var result = await _service.SearchAsync("Juan", 1);

            Assert.True(result.Exitoso);
            Assert.Equal(2, result.Datos!.Count);
        }

        // ---------- GetAppointmentsByDoctorIdAsync (Agenda del Médico) ----------

        [Fact]
        public async Task GetAppointmentsByDoctorIdAsync_ReturnsAppointments()
        {
            _apptRepoMock.Setup(r => r.GetByDoctorIdAsync(1)).ReturnsAsync(new List<Appointment>
            {
                new Appointment { AppointmentId = 10, DoctorId = 1, PatientId = 5, AppointmentDate = DateTime.Now.AddDays(1), StatusId = 1 }
            });

            var result = await _service.GetAppointmentsByDoctorIdAsync(1);

            Assert.True(result.Exitoso);
            Assert.Single(result.Datos!);
            Assert.Equal(10, result.Datos![0].AppointmentId);
        }

        [Fact]
        public async Task GetAppointmentsByDoctorIdAsync_WhenInvalidId_ReturnsFailure()
        {
            var result = await _service.GetAppointmentsByDoctorIdAsync(0);

            Assert.False(result.Exitoso);
        }
    }
}