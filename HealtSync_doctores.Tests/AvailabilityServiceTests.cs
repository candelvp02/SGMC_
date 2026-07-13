using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Appointments;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Tests.Services
{
    public class AvailabilityServiceTests
    {
        private readonly Mock<IDoctorAvailabilityRepository> _repoMock;
        private readonly Mock<IDoctorRepository> _doctorRepoMock;
        private readonly Mock<ILogger<AvailabilityService>> _loggerMock;
        private readonly IAvailabilityService _service;

        public AvailabilityServiceTests()
        {
            _repoMock = new Mock<IDoctorAvailabilityRepository>();
            _doctorRepoMock = new Mock<IDoctorRepository>();
            _loggerMock = new Mock<ILogger<AvailabilityService>>();

            _service = new AvailabilityService(_repoMock.Object, _doctorRepoMock.Object, _loggerMock.Object);
        }

        private static CreateAvailabilityDto GetValidCreateDto()
        {
            return new CreateAvailabilityDto
            {
                DoctorId = 1,
                AvailableDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0),
                AvailabilityModeId = 1
            };
        }

        // ---------- CreateAsync ----------

        [Fact]
        public async Task CreateAsync_WhenDoctorDoesNotExist_ReturnsFailure()
        {
            var dto = GetValidCreateDto();
            _doctorRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SGMC.Domain.Entities.Users.Doctor, bool>>>()))
                .ReturnsAsync(false);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("doctor no existe", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task CreateAsync_WhenConflictExists_ReturnsFailure()
        {
            var dto = GetValidCreateDto();
            _doctorRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SGMC.Domain.Entities.Users.Doctor, bool>>>()))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.CheckForConflictAsync(dto.DoctorId, dto.AvailableDate, dto.StartTime, dto.EndTime))
                .ReturnsAsync(true);

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("conflicto", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task CreateAsync_WhenValid_ReturnsSuccess()
        {
            var dto = GetValidCreateDto();
            var savedEntity = new DoctorAvailability
            {
                AvailabilityId = 1,
                DoctorId = dto.DoctorId,
                AvailableDate = dto.AvailableDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                AvailabilityModeId = dto.AvailabilityModeId,
                IsActive = true
            };

            _doctorRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<SGMC.Domain.Entities.Users.Doctor, bool>>>()))
                .ReturnsAsync(true);
            _repoMock.Setup(r => r.CheckForConflictAsync(dto.DoctorId, dto.AvailableDate, dto.StartTime, dto.EndTime))
                .ReturnsAsync(false);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<DoctorAvailability>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(savedEntity);

            var result = await _service.CreateAsync(dto);

            Assert.True(result.Exitoso);
            Assert.NotNull(result.Datos);
            Assert.Equal(dto.AvailableDate, result.Datos!.AvailableDate);
            Assert.Equal(dto.StartTime, result.Datos!.StartTime);
        }

        // ---------- UpdateAsync ----------

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFailure()
        {
            var dto = new UpdateAvailabilityDto
            {
                AvailabilityId = 1,
                DoctorId = 1,
                AvailableDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DoctorAvailability?)null);

            var result = await _service.UpdateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("no encontrada", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task UpdateAsync_WhenConflictExists_ReturnsFailure()
        {
            var existing = new DoctorAvailability { AvailabilityId = 1, DoctorId = 1 };
            var dto = new UpdateAvailabilityDto
            {
                AvailabilityId = 1,
                DoctorId = 1,
                AvailableDate = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.CheckForConflictExcludingCurrentAsync(1, 1, dto.AvailableDate, dto.StartTime, dto.EndTime))
                .ReturnsAsync(true);

            var result = await _service.UpdateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.Contains("conflicto", result.Mensaje.ToLower());
        }

        // ---------- DeleteAsync ----------

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
        {
            _repoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(false);

            var result = await _service.DeleteAsync(1);

            Assert.False(result.Exitoso);
        }

        [Fact]
        public async Task DeleteAsync_WhenValid_ReturnsSuccess()
        {
            _repoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            var result = await _service.DeleteAsync(1);

            Assert.True(result.Exitoso);
        }

        // ---------- GetByDoctorIdAsync ----------

        [Fact]
        public async Task GetByDoctorIdAsync_ReturnsList()
        {
            _repoMock.Setup(r => r.GetByDoctorIdAsync(1)).ReturnsAsync(new List<DoctorAvailability>
            {
                new DoctorAvailability { AvailabilityId = 1, DoctorId = 1, AvailableDate = DateOnly.FromDateTime(DateTime.Now), StartTime = new TimeOnly(8,0), EndTime = new TimeOnly(10,0), IsActive = true }
            });

            var result = await _service.GetByDoctorIdAsync(1);

            Assert.True(result.Exitoso);
            Assert.Single(result.Datos!);
        }

        [Fact]
        public async Task GetByDoctorIdAsync_WhenInvalidId_ReturnsFailure()
        {
            var result = await _service.GetByDoctorIdAsync(0);

            Assert.False(result.Exitoso);
        }
    }
}