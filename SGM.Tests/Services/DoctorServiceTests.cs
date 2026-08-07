using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
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

        private static RegisterDoctorDto GetValidDto(string licenseNumber, DateOnly licenseExpiration)
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

        [Fact]
        public async Task CreateAsync_WhenLicenseNumberEmpty_ReturnsFailure()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            var dto = GetValidDto(string.Empty, futureDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);

            var detalle = string.Join(" ", result.Errores ?? new List<string>()).ToLower();
            Assert.True(
                detalle.Contains("licencia") ||
                detalle.Contains("requerido") ||
                detalle.Contains("no se pudo"),
                $"Expected errors to contain 'licencia', 'requerido' or 'no se pudo', but got: {detalle}"
            );
        }

        [Fact]
        public async Task CreateAsync_WhenLicenseExpired_ReturnsFailure()
        {
            var expiredDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            var dto = GetValidDto("L123-VALID", expiredDate);

            _personRepoMock.Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _userRepoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
            _repoMock.Setup(r => r.ExistsByLicenseNumberAsync(It.IsAny<string>())).ReturnsAsync(false);
            _specialtyRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);

            var detalle = string.Join(" ", result.Errores ?? new List<string>()).ToLower();
            Assert.True(
                detalle.Contains("vigente") ||
                detalle.Contains("expirad") ||
                detalle.Contains("vencid") ||
                detalle.Contains("no se pudo"),
                $"Expected errors to contain 'vigente', 'expirad', 'vencid' or 'no se pudo', but got: {detalle}"
            );
        }
    }
}