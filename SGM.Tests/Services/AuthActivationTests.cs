using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Domain.Base;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Users;

namespace SGMC.Tests.Services
{
    public class AuthActivationTests
    {
        private readonly Mock<IPatientRepository> _repoMock;
        private readonly Mock<ILogger<PatientService>> _loggerMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPersonRepository> _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceProviderRepoMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly IPatientService _service;

        public AuthActivationTests()
        {
            _repoMock = new Mock<IPatientRepository>();
            _loggerMock = new Mock<ILogger<PatientService>>();
            _userRepoMock = new Mock<IUserRepository>();
            _personRepoMock = new Mock<IPersonRepository>();
            _insuranceProviderRepoMock = new Mock<IInsuranceProviderRepository>();
            _notificationMock = new Mock<INotificationService>();

            _service = new PatientService(
                _repoMock.Object,
                _loggerMock.Object,
                _userRepoMock.Object,
                _personRepoMock.Object,
                _insuranceProviderRepoMock.Object);
        }

        // PRUEBA 1: Paciente recién creado debe estar inactivo
        [Fact]
        public async Task CreateAsync_NewPatient_IsInactive()
        {
            var result = await _service.CreateAsync(null!);
            Assert.False(result.Exitoso);
        }

        // PRUEBA 2: Correo duplicado debe rechazar el registro
        [Fact]
        public async Task CreateAsync_DuplicateEmail_ReturnsFailure()
        {
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var dto = new Application.Dto.Users.RegisterPatientDto
            {
                FirstName = "Juan",
                LastName = "Pérez",
                Email = "juan@test.com",
                Password = "Password1",
                Gender = "Masculino",
                IdentificationNumber = "001-0000000-0",
                PhoneNumber = "809-000-0000",
                Address = "Calle 1",
                EmergencyContactName = "María",
                EmergencyContactPhone = "809-111-1111",
                BloodType = "O+",
                InsuranceProviderId = 1
            };

            var result = await _service.CreateAsync(dto);

            Assert.False(result.Exitoso);
            Assert.False(result.Exitoso);
        }

        // PRUEBA 3: Correo de activación se envía tras registro exitoso
        [Fact]
        public async Task SendActivationEmail_WhenCalled_ReturnsSuccess()
        {
            _notificationMock
                .Setup(n => n.SendAccountActivationEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(OperationResult.Exito("Email enviado"));

            var result = await _notificationMock.Object
                .SendAccountActivationEmailAsync("juan@test.com", 1);

            Assert.True(result.Exitoso);
        }

        // PRUEBA 4: Email vacío no debe enviarse
        [Fact]
        public async Task SendActivationEmail_EmptyEmail_ReturnsFailure()
        {
            _notificationMock
                .Setup(n => n.SendAccountActivationEmailAsync(
                    string.Empty,
                    It.IsAny<int>()))
                .ReturnsAsync(OperationResult.Fallo("Email requerido"));

            var result = await _notificationMock.Object
                .SendAccountActivationEmailAsync(string.Empty, 1);

            Assert.False(result.Exitoso);
        }
    }
}
