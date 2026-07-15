using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Insurance;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Repositories.Insurance;
using Xunit;

namespace SGMC.Tests.Services
{
    public class InsuranceProviderServiceTests
    {
        // ── Mocks compartidos ──────────────────────────────────────────
        private readonly Mock<IInsuranceProviderRepository> _repositoryMock;
        private readonly Mock<INetworkTypeRepository> _networkTypeRepositoryMock;
        private readonly Mock<ILogger<InsuranceProviderService>> _loggerMock;
        private readonly InsuranceProviderService _service;

        public InsuranceProviderServiceTests()
        {
            _repositoryMock = new Mock<IInsuranceProviderRepository>();
            _networkTypeRepositoryMock = new Mock<INetworkTypeRepository>();
            _loggerMock = new Mock<ILogger<InsuranceProviderService>>();

            _service = new InsuranceProviderService(
                _repositoryMock.Object,
                _networkTypeRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        // ── TEST 1 ─────────────────────────────────────────────────────
        // GetActiveAsync debe devolver únicamente proveedores activos
        [Fact]
        public async Task GetActiveAsync_DebeRetornarSoloProveedoresActivos()
        {
            // Arrange
            var proveedoresActivos = new List<InsuranceProvider>
            {
                new InsuranceProvider
                {
                    InsuranceProviderId = 1,
                    Name = "ARS Humano",
                    IsActive = true,
                    IsPreferred = true,
                    NetworkTypeId = 1,
                    NetworkType = new NetworkType { NetworkTypeId = 1, Name = "HMO" },
                    CreatedAt = DateTime.UtcNow
                },
                new InsuranceProvider
                {
                    InsuranceProviderId = 2,
                    Name = "ARS Salud Segura",
                    IsActive = true,
                    IsPreferred = false,
                    NetworkTypeId = 2,
                    NetworkType = new NetworkType { NetworkTypeId = 2, Name = "PPO" },
                    CreatedAt = DateTime.UtcNow
                }
            };

            _repositoryMock
                .Setup(r => r.GetActiveProviderAsync())
                .ReturnsAsync(proveedoresActivos);

            // Act
            var resultado = await _service.GetActiveAsync();

            // Assert
            resultado.Exitoso.Should().BeTrue();
            resultado.Datos.Should().NotBeNull();
            resultado.Datos!.Count.Should().Be(2);
            resultado.Datos.Should().OnlyContain(p => p.IsActive == true);
        }

        // ── TEST 2 ─────────────────────────────────────────────────────
        // GetByIdAsync debe fallar cuando el ID es menor o igual a cero
        [Fact]
        public async Task GetByIdAsync_CuandoIdEsInvalido_DebeRetornarFallo()
        {
            // Arrange
            int idInvalido = 0;

            // Act
            var resultado = await _service.GetByIdAsync(idInvalido);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El ID del proveedor es inválido");

            // Verificar que nunca se llamó al repositorio
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        // ── TEST 3 ─────────────────────────────────────────────────────
        // GetByIdAsync debe fallar cuando el proveedor no existe en la BD
        [Fact]
        public async Task GetByIdAsync_CuandoProveedorNoExiste_DebeRetornarFallo()
        {
            // Arrange
            int idInexistente = 999;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(idInexistente))
                .ReturnsAsync((InsuranceProvider?)null);

            // Act
            var resultado = await _service.GetByIdAsync(idInexistente);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("Proveedor de seguro no encontrado");
        }

        // ── TEST 4 ─────────────────────────────────────────────────────
        // CreateAsync debe fallar si el NetworkTypeId no existe en la BD
        [Fact]
        public async Task CreateAsync_CuandoTipoDeRedNoExiste_DebeRetornarFallo()
        {
            // Arrange
            var dto = new CreateInsuranceProviderDto
            {
                Name = "ARS Nueva",
                PhoneNumber = "809-000-0000",
                Email = "contacto@arsnueva.com",
                Address = "Calle Principal 1",
                NetworkTypeId = 99, // ID que no existe
                CoverageDetails = "Cobertura básica"
            };

            _networkTypeRepositoryMock
                .Setup(r => r.ExistsAsync(dto.NetworkTypeId))
                .ReturnsAsync(false);

            // Act
            var resultado = await _service.CreateAsync(dto);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El tipo de red seleccionado no existe");

            // Verificar que nunca se intentó guardar en la BD
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<InsuranceProvider>()), Times.Never);
        }

        // ── TEST 5 ─────────────────────────────────────────────────────
        // DeleteAsync debe fallar si el proveedor a eliminar no existe
        [Fact]
        public async Task DeleteAsync_CuandoProveedorNoExiste_DebeRetornarFallo()
        {
            // Arrange
            int idInexistente = 500;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(idInexistente))
                .ReturnsAsync((InsuranceProvider?)null);

            // Act
            var resultado = await _service.DeleteAsync(idInexistente);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("Proveedor de seguro no encontrado");

            // Verificar que nunca se llamó al método de eliminación
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}