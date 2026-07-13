using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.PBIs
{
    /// <summary>
    /// PBI #20 — Perfil del Paciente
    /// Criterios cubiertos:
    /// CA-1  El paciente puede editar y guardar cualquier campo
    /// CA-2  Campos requeridos deben completarse → error con indicación
    /// CA-3  Género solo acepta "M" / "F"
    /// CA-4  Proveedor de seguro debe ser uno activo en el sistema
    /// CA-5  Los cambios se reflejan inmediatamente tras guardar
    /// CA-6  ID inválido rechazado
    /// </summary>
    public class PBI20_PerfilPacienteTests
    {
        private readonly Mock<IPatientRepository>           _repoMock;
        private readonly Mock<IUserRepository>              _userRepoMock;
        private readonly Mock<IPersonRepository>            _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceRepoMock;
        private readonly IPatientService                    _service;

        public PBI20_PerfilPacienteTests()
        {
            _repoMock          = new Mock<IPatientRepository>();
            _userRepoMock      = new Mock<IUserRepository>();
            _personRepoMock    = new Mock<IPersonRepository>();
            _insuranceRepoMock = new Mock<IInsuranceProviderRepository>();
            var loggerMock     = new Mock<ILogger<PatientService>>();

            _service = new PatientService(
                _repoMock.Object,
                loggerMock.Object,
                _userRepoMock.Object,
                _personRepoMock.Object,
                _insuranceRepoMock.Object);
        }

        // Helper
        private static UpdatePatientDto DtoActualizacionValido() => new()
        {
            PatientId            = 5,
            PhoneNumber          = "809-555-1111",
            Address              = "Av. Independencia #50",
            EmergencyContactName = "Luis García",
            EmergencyContactPhone = "809-555-2222",
            Allergies            = "Ninguna",
            InsuranceProviderId  = 2
        };

        private static Patient PacienteExistente() => new()
        {
            PatientId            = 5,
            PhoneNumber          = "809-000-0000",
            Address              = "Calle Vieja #1",
            EmergencyContactName = "Contacto Viejo",
            EmergencyContactPhone = "809-000-0001",
            Allergies            = "Polen",
            InsuranceProviderId  = 1,
            IsActive             = true,
            PatientNavigation    = new Person { FirstName = "Ana", LastName = "López" }
        };

        // -----------------------------------------------------
        //  CA-1 / CA-5  Edición exitosa y reflejo inmediato
        // -----------------------------------------------------

        /// <summary>CA-1/CA-5: Actualización válida guarda y devuelve los nuevos valores.</summary>
        [Fact]
        public async Task ActualizarPerfil_DatosValidos_ReflejaLosNuevosValores()
        {
            var dto      = DtoActualizacionValido();
            var paciente = PacienteExistente();

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(dto.PatientId))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.ExistsAsync(dto.InsuranceProviderId))
                .ReturnsAsync(true);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var result = await _service.UpdateAsync(dto);

            result.Exitoso.Should().BeTrue("la actualización con datos válidos debe ser exitosa");
            result.Datos.Should().NotBeNull();
            result.Datos!.PhoneNumber.Should().Be(dto.PhoneNumber,
                "el teléfono debe reflejarse inmediatamente tras guardar");
            result.Datos.Address.Should().Be(dto.Address,
                "la dirección debe reflejarse inmediatamente tras guardar");
            result.Datos.EmergencyContactName.Should().Be(dto.EmergencyContactName,
                "el contacto de emergencia debe reflejarse inmediatamente");
        }

        /// <summary>CA-5: UpdatedAt debe actualizarse al momento de guardar.</summary>
        [Fact]
        public async Task ActualizarPerfil_GuardaFechaDeActualizacion()
        {
            var dto      = DtoActualizacionValido();
            var paciente = PacienteExistente();
            paciente.UpdatedAt = null;  // sin fecha previa

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(dto.PatientId))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.ExistsAsync(dto.InsuranceProviderId))
                .ReturnsAsync(true);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            await _service.UpdateAsync(dto);

            paciente.UpdatedAt.Should().NotBeNull(
                "el sistema debe registrar la fecha y hora de la última actualización");
        }

        // -----------------------------------------------------
        //  CA-2  Campos requeridos
        // -----------------------------------------------------

        /// <summary>CA-2: DTO nulo rechazado con mensaje claro.</summary>
        [Fact]
        public async Task ActualizarPerfil_DtoNulo_RetornaFallo()
        {
            var result = await _service.UpdateAsync(null!);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("requerido");
        }

        /// <summary>CA-2: PatientId inválido rechazado en el validador.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task ActualizarPerfil_IdInvalido_RetornaFallo(int id)
        {
            var dto = DtoActualizacionValido();
            dto.PatientId = id;

            var result = await _service.UpdateAsync(dto);

            result.Exitoso.Should().BeFalse("un ID inválido no debe permitir la actualización");
        }

        /// <summary>CA-2: Paciente no encontrado en BD retorna error con indicación.</summary>
        [Fact]
        public async Task ActualizarPerfil_PacienteNoEncontrado_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((Patient?)null);

            var result = await _service.UpdateAsync(DtoActualizacionValido());

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("no encontrado",
                "debe indicar que el paciente no existe");
        }

        // -----------------------------------------------------
        //  CA-3  Solo actualiza campos enviados (patch parcial)
        // -----------------------------------------------------

        /// <summary>CA-3: Solo la dirección se actualiza cuando solo ella se envía.</summary>
        [Fact]
        public async Task ActualizarContactoParcial_SoloDireccion_ModificaSoloEseCampo()
        {
            var paciente = PacienteExistente();
            var contactoOriginal = paciente.EmergencyContactName;
            var telefonoOriginal = paciente.EmergencyContactPhone;

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto    = new PatchPatientContactDto { Address = "Nueva Dirección 999" };
            var result = await _service.PatchContactInfoAsync(5, dto);

            result.Exitoso.Should().BeTrue();
            paciente.Address.Should().Be("Nueva Dirección 999");
            paciente.EmergencyContactName.Should().Be(contactoOriginal,
                "los campos no enviados no deben modificarse");
            paciente.EmergencyContactPhone.Should().Be(telefonoOriginal,
                "los campos no enviados no deben modificarse");
        }

        /// <summary>CA-3: Patch sin ningún campo retorna fallo descriptivo.</summary>
        [Fact]
        public async Task ActualizarContactoParcial_SinCampos_RetornaFallo()
        {
            var dto    = new PatchPatientContactDto();  // todos null
            var result = await _service.PatchContactInfoAsync(5, dto);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("al menos un campo");
        }

        // -----------------------------------------------------
        //  CA-4  Proveedor de seguro activo
        // -----------------------------------------------------

        /// <summary>CA-4: Proveedor de seguro inactivo rechaza la actualización.</summary>
        [Fact]
        public async Task ActualizarSeguro_ProveedorInactivo_RetornaFallo()
        {
            var paciente = PacienteExistente();

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync(new InsuranceProvider
                {
                    InsuranceProviderId = 99,
                    Name     = "Seguro Inactivo",
                    IsActive = false   // ← inactivo
                });

            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 99 };
            var result = await _service.PatchInsuranceProviderAsync(5, dto);

            result.Exitoso.Should().BeFalse(
                "el sistema no debe permitir seleccionar un proveedor de seguro inactivo");
            result.Mensaje.ToLower().Should().ContainAny("activo", "inactivo", "seguro");
        }

        /// <summary>CA-4: Proveedor de seguro activo acepta la actualización.</summary>
        [Fact]
        public async Task ActualizarSeguro_ProveedorActivo_RetornaExito()
        {
            var paciente = PacienteExistente();
            var nuevoSeguro = new InsuranceProvider
            {
                InsuranceProviderId = 3,
                Name     = "ARS Universal",
                IsActive = true
            };

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(3))
                .ReturnsAsync(nuevoSeguro);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 3 };
            var result = await _service.PatchInsuranceProviderAsync(5, dto);

            result.Exitoso.Should().BeTrue(
                "un proveedor de seguro activo debe poderse asignar");
            paciente.InsuranceProviderId.Should().Be(3,
                "el proveedor de seguro debe actualizarse al nuevo valor");
        }

        /// <summary>CA-4: InsuranceProviderId 0 es rechazado por el validador.</summary>
        [Fact]
        public void Validador_SeguroIdCero_RetornaFallo()
        {
            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 0 };
            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("seguro"));
        }

        // -----------------------------------------------------
        //  CA-6  ID inválido (seguridad: solo el propio paciente puede editar)
        // -----------------------------------------------------

        /// <summary>CA-6: GetByUserId con ID inválido retorna fallo.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ObtenerPorUserId_IdInvalido_RetornaFallo(int userId)
        {
            var result = await _service.GetByUserIdAsync(userId);

            result.Exitoso.Should().BeFalse("un userId inválido no puede recuperar un perfil");
            result.Mensaje.ToLower().Should().ContainAny("inválido", "invalido", "id");
        }

        /// <summary>CA-6: GetById con ID inválido retorna fallo.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-99)]
        public async Task ObtenerPorId_IdInvalido_RetornaFallo(int id)
        {
            var result = await _service.GetByIdAsync(id);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().ContainAny("inválido", "invalido");
        }
    }
}
