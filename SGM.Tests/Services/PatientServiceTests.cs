using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Tests.Services
{
    public class PatientServiceTests
    {
        private readonly Mock<IPatientRepository> _repoMock;
        private readonly Mock<ILogger<PatientService>> _loggerMock;
        private readonly IPatientService _service;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPersonRepository> _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceProviderRepoMock;

        public PatientServiceTests()
        {
            _repoMock = new Mock<IPatientRepository>();
            _loggerMock = new Mock<ILogger<PatientService>>();
            _userRepoMock = new Mock<IUserRepository>();
            _personRepoMock = new Mock<IPersonRepository>();
            _insuranceProviderRepoMock = new Mock<IInsuranceProviderRepository>();

            _service = new PatientService(
                _repoMock.Object,
                _loggerMock.Object,
                _userRepoMock.Object,
                _personRepoMock.Object,
                _insuranceProviderRepoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenDtoIsNull_ReturnsFailure()
        {
            var result = await _service.CreateAsync(null!);
            Assert.False(result.Exitoso);
            Assert.Contains("datos del paciente son requeridos", result.Mensaje.ToLower());
        }

        // ── PatchContactInfoAsync [Pruebas de Luis (Nota: Si hay algo raro o les choca, me informan)]

        [Fact]
        public async Task PatchContactInfoAsync_WhenDtoIsNull_ReturnsFailure()
        {
            var result = await _service.PatchContactInfoAsync(1, null!);
            Assert.False(result.Exitoso);
        }

        [Fact]
        public async Task PatchContactInfoAsync_WhenIdIsZero_ReturnsFailure()
        {
            var dto = new PatchPatientContactDto { Address = "Nueva Dirección" };
            var result = await _service.PatchContactInfoAsync(0, dto);
            Assert.False(result.Exitoso);
        }

        [Fact]
        public async Task PatchContactInfoAsync_WhenAllFieldsAreNull_ReturnsFailure()
        {
            var dto = new PatchPatientContactDto(); // todos null
            var result = await _service.PatchContactInfoAsync(1, dto);
            Assert.False(result.Exitoso);
            Assert.Contains("al menos un campo", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task PatchContactInfoAsync_WhenPatientNotFound_ReturnsFailure()
        {
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Patient?)null);

            var dto = new PatchPatientContactDto { Address = "Nueva Dirección" };
            var result = await _service.PatchContactInfoAsync(99, dto);

            Assert.False(result.Exitoso);
            Assert.Contains("no encontrado", result.Mensaje.ToLower());
        }

        [Fact]
        public async Task PatchContactInfoAsync_WhenOnlyAddressSent_UpdatesOnlyAddress()
        {
            var patient = new Patient
            {
                PatientId = 1,
                Address = "Dirección Vieja",
                EmergencyContactName = "Contacto Original",
                EmergencyContactPhone = "809-000-0000"
            };

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(patient);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto = new PatchPatientContactDto { Address = "Dirección Nueva" };
            var result = await _service.PatchContactInfoAsync(1, dto);

            Assert.True(result.Exitoso);
            // Address actualizado
            Assert.Equal("Dirección Nueva", patient.Address);
            // Los demás campos no se tocaron
            Assert.Equal("Contacto Original", patient.EmergencyContactName);
            Assert.Equal("809-000-0000", patient.EmergencyContactPhone);
        }

        [Fact]
        public async Task PatchContactInfoAsync_WhenAllFieldsSent_UpdatesAll()
        {
            var patient = new Patient
            {
                PatientId = 1,
                Address = "Vieja Dirección",
                EmergencyContactName = "Viejo Contacto",
                EmergencyContactPhone = "809-000-0000"
            };

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(patient);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto = new PatchPatientContactDto
            {
                Address = "Calle Nueva 123",
                EmergencyContactName = "Nuevo Contacto",
                EmergencyContactPhone = "809-111-2222"
            };

            var result = await _service.PatchContactInfoAsync(1, dto);

            Assert.True(result.Exitoso);
            Assert.Equal("Calle Nueva 123", patient.Address);
            Assert.Equal("Nuevo Contacto", patient.EmergencyContactName);
            Assert.Equal("809-111-2222", patient.EmergencyContactPhone);
        }
    }
}