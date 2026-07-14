using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Medical;
using SGMC.Application.Services;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Repositories.Medical;
using Xunit;

namespace SGMC.Tests.Services
{
    public class SpecialtyServiceTests
    {
        private readonly Mock<ISpecialtyRepository> _repositoryMock;
        private readonly Mock<ILogger<SpecialtyService>> _loggerMock;
        private readonly SpecialtyService _service;

        public SpecialtyServiceTests()
        {
            _repositoryMock = new Mock<ISpecialtyRepository>();
            _loggerMock = new Mock<ILogger<SpecialtyService>>();
            _service = new SpecialtyService(_repositoryMock.Object, _loggerMock.Object);
        }

        // TEST 1
        // CreateAsync debe fallar si ya existe una especialidad con el mismo nombre
        [Fact]
        public async Task CreateAsync_CuandoNombreYaExiste_DebeRetornarFallo()
        {
            // Arrange
            var dto = new CreateSpecialtyDto { SpecialtyName = "Cardiología" };

            _repositoryMock
                .Setup(r => r.ExistsByNameAsync(dto.SpecialtyName))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.CreateAsync(dto);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("Ya existe una especialidad con ese nombre.");

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Specialty>()), Times.Never);
        }

        // TEST 2
        // GetByIdAsync debe fallar cuando el ID es inválido
        [Fact]
        public async Task GetByIdAsync_CuandoIdEsInvalido_DebeRetornarFallo()
        {
            // Arrange
            short idInvalido = 0;

            // Act
            var resultado = await _service.GetByIdAsync(idInvalido);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("El ID es inválido");

            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<short>()), Times.Never);
        }

        // TEST 3
        // GetByIdAsync debe fallar cuando la especialidad no existe
        [Fact]
        public async Task GetByIdAsync_CuandoEspecialidadNoExiste_DebeRetornarFallo()
        {
            // Arrange
            short idInexistente = 999;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(idInexistente))
                .ReturnsAsync((Specialty?)null);

            // Act
            var resultado = await _service.GetByIdAsync(idInexistente);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("Especialidad no encontrada");
        }

        // TEST 4
        // GetActiveAsync debe devolver solo especialidades activas
        [Fact]
        public async Task GetActiveAsync_DebeRetornarSoloEspecialidadesActivas()
        {
            // Arrange
            var especialidadesActivas = new List<Specialty>
            {
                new Specialty { SpecialtyId = 1, SpecialtyName = "Cardiología", IsActive = true, CreatedAt = DateTime.Now },
                new Specialty { SpecialtyId = 2, SpecialtyName = "Pediatría", IsActive = true, CreatedAt = DateTime.Now }
            };

            _repositoryMock
                .Setup(r => r.GetActiveAsync())
                .ReturnsAsync(especialidadesActivas);

            // Act
            var resultado = await _service.GetActiveAsync();

            // Assert
            resultado.Exitoso.Should().BeTrue();
            resultado.Datos.Should().NotBeNull();
            resultado.Datos!.Count.Should().Be(2);
            resultado.Datos.Should().OnlyContain(e => e.IsActive == true);
        }

        // TEST 5
        // DeleteAsync debe fallar si la especialidad a eliminar no existe
        [Fact]
        public async Task DeleteAsync_CuandoEspecialidadNoExiste_DebeRetornarFallo()
        {
            // Arrange
            short idInexistente = 500;

            _repositoryMock
                .Setup(r => r.GetByIdAsync(idInexistente))
                .ReturnsAsync((Specialty?)null);

            // Act
            var resultado = await _service.DeleteAsync(idInexistente);

            // Assert
            resultado.Exitoso.Should().BeFalse();
            resultado.Mensaje.Should().Be("Especialidad no encontrada");

            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Specialty>()), Times.Never);
        }
    }
}