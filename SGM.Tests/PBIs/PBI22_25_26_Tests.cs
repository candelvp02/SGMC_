using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Medical;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Medical;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.PBIs
{
    // -------------------------------------------------------------------
    //  PBI #22 — Historial de Citas del Paciente
    // -------------------------------------------------------------------

    /// <summary>
    /// PBI #22 — Consulta de Historial de Citas
    /// Criterios cubiertos:
    /// CA-1  El listado solo muestra las citas del paciente autenticado
    /// CA-2  Si no tiene citas se muestra mensaje indicándolo
    /// CA-3  Obtener paciente con citas por ID válido retorna datos
    /// CA-4  ID inválido retorna fallo
    /// </summary>
    public class PBI22_HistorialCitasPacienteTests
    {
        private readonly Mock<IPatientRepository>           _repoMock;
        private readonly Mock<IUserRepository>              _userRepoMock;
        private readonly Mock<IPersonRepository>            _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceRepoMock;
        private readonly IPatientService                    _service;

        public PBI22_HistorialCitasPacienteTests()
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

        // CA-1  Solo las citas del paciente autenticado

        /// <summary>CA-1: GetWithAppointmentsAsync retorna únicamente las citas del paciente solicitado.</summary>
        [Fact]
        public async Task HistorialCitas_PacienteExistente_RetornaSusCitas()
        {
            var paciente = new Patient
            {
                PatientId = 7,
                IsActive  = true,
                PatientNavigation = new Person { FirstName = "Juan", LastName = "Díaz" }
            };

            _repoMock
                .Setup(r => r.GetByIdWithAppointmentsAsync(7))
                .ReturnsAsync(paciente);

            var result = await _service.GetWithAppointmentsAsync(7);

            result.Exitoso.Should().BeTrue("un paciente existente debe poder ver su historial");
            result.Datos.Should().NotBeNull();
            result.Datos!.Should().HaveCount(1,
                "el resultado incluye solo los datos del paciente solicitado");
            result.Datos[0].PatientId.Should().Be(7,
                "los datos devueltos deben corresponder al paciente solicitado");
        }

        // CA-2  Paciente sin citas

        /// <summary>CA-2: Paciente no encontrado retorna fallo con mensaje indicándolo.</summary>
        [Fact]
        public async Task HistorialCitas_PacienteNoEncontrado_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.GetByIdWithAppointmentsAsync(It.IsAny<int>()))
                .ReturnsAsync((Patient?)null);

            var result = await _service.GetWithAppointmentsAsync(99);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("no encontrado",
                "debe indicar que el paciente (y por ende sus citas) no existen");
        }

        // CA-3 / CA-4  ID válido e inválido

        /// <summary>CA-4: ID de paciente inválido retorna fallo sin intentar acceder a la BD.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public async Task HistorialCitas_IdInvalido_RetornaFallo(int id)
        {
            var result = await _service.GetWithAppointmentsAsync(id);

            result.Exitoso.Should().BeFalse("un ID inválido no debe consultar la BD");
            _repoMock.Verify(r => r.GetByIdWithAppointmentsAsync(It.IsAny<int>()), Times.Never,
                "no debe llamarse al repositorio si el ID es inválido");
        }

        /// <summary>CA-1/CA-3: Verificación de existencia de paciente funciona con ID válido.</summary>
        [Fact]
        public async Task ExistenciaPaciente_IdValido_RetornaResultadoCorrecto()
        {
            _repoMock
                .Setup(r => r.ExistsAsync(5))
                .ReturnsAsync(true);

            var result = await _service.ExistsAsync(5);

            result.Exitoso.Should().BeTrue();
            result.Datos.Should().BeTrue("el paciente con ID 5 existe");
        }

        /// <summary>CA-1: Paciente inexistente reportado correctamente.</summary>
        [Fact]
        public async Task ExistenciaPaciente_PacienteInexistente_RetornaFalso()
        {
            _repoMock
                .Setup(r => r.ExistsAsync(999))
                .ReturnsAsync(false);

            var result = await _service.ExistsAsync(999);

            result.Exitoso.Should().BeTrue("la consulta se completa sin error");
            result.Datos.Should().BeFalse("el paciente no existe en el sistema");
        }
    }

    // ----------------------------------------------------------------------
    //  PBI #25 — Actualización de Proveedor de Seguro
    // ----------------------------------------------------------------------

    /// <summary>
    /// PBI #25 — Actualización de Proveedor de Seguro
    /// Criterios cubiertos:
    /// CA-1  Solo se muestran proveedores activos
    /// CA-2  Proveedor inactivo rechazado
    /// CA-3  Cambio se guarda inmediatamente
    /// CA-4  Sin proveedor seleccionado mantiene valor anterior
    /// CA-5  Registro de fecha/hora de la última actualización
    /// </summary>
    public class PBI25_ActualizacionSeguroTests
    {
        private readonly Mock<IPatientRepository>           _repoMock;
        private readonly Mock<IUserRepository>              _userRepoMock;
        private readonly Mock<IPersonRepository>            _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceRepoMock;
        private readonly IPatientService                    _service;

        public PBI25_ActualizacionSeguroTests()
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

        private static Patient PacienteBase() => new()
        {
            PatientId           = 5,
            InsuranceProviderId = 1,
            IsActive            = true,
            PatientNavigation   = new Person { FirstName = "Ana", LastName = "López" }
        };

        // CA-1 / CA-2  Solo proveedores activos

        /// <summary>CA-2: Proveedor inactivo retorna fallo con mensaje claro.</summary>
        [Fact]
        public async Task ActualizarSeguro_ProveedorInactivo_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(PacienteBase());
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(50))
                .ReturnsAsync(new InsuranceProvider
                {
                    InsuranceProviderId = 50,
                    Name     = "Seguro Inactivo S.A.",
                    IsActive = false
                });

            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 50 };
            var result = await _service.PatchInsuranceProviderAsync(5, dto);

            result.Exitoso.Should().BeFalse("no se puede asignar un proveedor inactivo");
            result.Mensaje.ToLower().Should().ContainAny("activo", "inactivo", "seguro");
        }

        /// <summary>CA-1: Proveedor activo es aceptado correctamente.</summary>
        [Fact]
        public async Task ActualizarSeguro_ProveedorActivo_RetornaExito()
        {
            var paciente = PacienteBase();
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new InsuranceProvider
                {
                    InsuranceProviderId = 10,
                    Name     = "Humano Seguros",
                    IsActive = true
                });
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 10 };
            var result = await _service.PatchInsuranceProviderAsync(5, dto);

            result.Exitoso.Should().BeTrue("un proveedor activo debe poderse asignar");
        }

        // CA-3  Cambio inmediato

        /// <summary>CA-3: El nuevo proveedor se persiste y se devuelve en la respuesta.</summary>
        [Fact]
        public async Task ActualizarSeguro_NuevoProveedor_SeRefleja()
        {
            var paciente = PacienteBase();  // InsuranceProviderId = 1
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);

            var nuevoSeguro = new InsuranceProvider
            {
                InsuranceProviderId = 7,
                Name     = "ARS Plan Salud",
                IsActive = true
            };
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(7))
                .ReturnsAsync(nuevoSeguro);
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto = new PatchPatientInsuranceDto { InsuranceProviderId = 7 };
            await _service.PatchInsuranceProviderAsync(5, dto);

            paciente.InsuranceProviderId.Should().Be(7,
                "el proveedor de seguro debe actualizarse al nuevo valor de inmediato");
        }

        // CA-4  Sin selección mantiene valor anterior

        /// <summary>CA-4: InsuranceProviderId = 0 rechazado, manteniendo el anterior.</summary>
        [Fact]
        public async Task ActualizarSeguro_SinSeleccion_RetornaFallo()
        {
            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = 0 };
            var result = await _service.PatchInsuranceProviderAsync(5, dto);

            result.Exitoso.Should().BeFalse(
                "si no se selecciona un proveedor válido la actualización debe rechazarse");
        }

        // CA-5  Fecha/hora de actualización

        /// <summary>CA-5: UpdatedAt debe quedar registrado tras la actualización del seguro.</summary>
        [Fact]
        public async Task ActualizarSeguro_RegistraFechaHora()
        {
            var paciente = PacienteBase();
            paciente.UpdatedAt = null;

            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(5))
                .ReturnsAsync(paciente);
            _insuranceRepoMock
                .Setup(r => r.GetByIdAsync(3))
                .ReturnsAsync(new InsuranceProvider
                    { InsuranceProviderId = 3, Name = "Activo", IsActive = true });
            _repoMock
                .Setup(r => r.UpdateAsync(It.IsAny<Patient>()))
                .Returns(Task.CompletedTask);

            var dto = new PatchPatientInsuranceDto { InsuranceProviderId = 3 };
            await _service.PatchInsuranceProviderAsync(5, dto);

            paciente.UpdatedAt.Should().NotBeNull(
                "el sistema debe registrar la fecha y hora de la última actualización del seguro");
        }

        // Validador del DTO

        /// <summary>CA-4: Validador rechaza InsuranceProviderId negativo.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validador_SeguroIdInvalido_RetornaFallo(int id)
        {
            var dto    = new PatchPatientInsuranceDto { InsuranceProviderId = id };
            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("seguro"));
        }
    }

    // ----------------------------------------------------------------------
    //  PBI #26 — Visualización de Especialidades Disponibles
    // ----------------------------------------------------------------------

    /// <summary>
    /// PBI #26 — Especialidades Disponibles
    /// Criterios cubiertos:
    /// CA-1  Solo se muestran especialidades activas
    /// CA-2  Listado ordenado (devuelto por el repositorio)
    /// CA-3  Sin especialidades se muestra mensaje indicándolo
    /// CA-4  GetByIdAsync con ID inválido retorna fallo
    /// CA-5  Especialidad existente retorna datos correctos
    /// </summary>
    public class PBI26_EspecialidadesDisponiblesTests
    {
        private readonly Mock<ISpecialtyRepository>   _repoMock;
        private readonly ISpecialtyService            _service;

        public PBI26_EspecialidadesDisponiblesTests()
        {
            _repoMock       = new Mock<ISpecialtyRepository>();
            var loggerMock  = new Mock<ILogger<SpecialtyService>>();
            _service        = new SpecialtyService(_repoMock.Object, loggerMock.Object);
        }

        // CA-1  Solo especialidades activas

        /// <summary>CA-1: GetActiveAsync devuelve únicamente las especialidades activas.</summary>
        [Fact]
        public async Task ObtenerActivas_RetornaSoloEspecialidadesActivas()
        {
            var activas = new List<Specialty>
            {
                new() { SpecialtyId = 1, SpecialtyName = "Cardiología",   IsActive = true },
                new() { SpecialtyId = 2, SpecialtyName = "Neurología",    IsActive = true },
                new() { SpecialtyId = 3, SpecialtyName = "Dermatología",  IsActive = true }
            };

            _repoMock
                .Setup(r => r.GetActiveAsync())
                .ReturnsAsync(activas);

            var result = await _service.GetActiveAsync();

            result.Exitoso.Should().BeTrue();
            result.Datos.Should().NotBeNull();
            result.Datos!.Should().HaveCount(3);
            result.Datos.Should().AllSatisfy(e => e.IsActive.Should().BeTrue(
                "GetActiveAsync solo debe devolver especialidades activas"));
        }

        /// <summary>CA-1: Especialidad inactiva no aparece en el listado de activas.</summary>
        [Fact]
        public async Task ObtenerActivas_EspecialidadInactivaNoAparece()
        {
            // El repositorio ya filtra — se simulan solo las activas
            var activas = new List<Specialty>
            {
                new() { SpecialtyId = 1, SpecialtyName = "Pediatría", IsActive = true }
            };

            _repoMock
                .Setup(r => r.GetActiveAsync())
                .ReturnsAsync(activas);

            var result = await _service.GetActiveAsync();

            result.Exitoso.Should().BeTrue();
            result.Datos!.Should().OnlyContain(e => e.IsActive,
                "el servicio no debe exponer especialidades inactivas");
        }

        // CA-3  Sin especialidades registradas

        /// <summary>CA-3: Si no hay especialidades activas, el listado viene vacío y exitoso.</summary>
        [Fact]
        public async Task ObtenerActivas_SinEspecialidades_RetornaListaVaciaConExito()
        {
            _repoMock
                .Setup(r => r.GetActiveAsync())
                .ReturnsAsync(new List<Specialty>());

            var result = await _service.GetActiveAsync();

            result.Exitoso.Should().BeTrue(
                "una consulta exitosa de lista vacía no es un error del servicio");
            result.Datos.Should().BeEmpty(
                "si no hay especialidades registradas el listado debe estar vacío");
        }

        // CA-4 / CA-5  GetByIdAsync

        /// <summary>CA-4: ID inválido retorna fallo sin consultar la BD.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ObtenerPorId_IdInvalido_RetornaFallo(short id)
        {
            var result = await _service.GetByIdAsync(id);

            result.Exitoso.Should().BeFalse("un ID inválido no debe recuperar ninguna especialidad");
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<short>()), Times.Never);
        }

        /// <summary>CA-5: Especialidad encontrada devuelve datos correctos.</summary>
        [Fact]
        public async Task ObtenerPorId_EspecialidadExistente_RetornaDatos()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Specialty
                {
                    SpecialtyId   = 1,
                    SpecialtyName = "Cardiología",
                    IsActive      = true
                });

            var result = await _service.GetByIdAsync(1);

            result.Exitoso.Should().BeTrue();
            result.Datos.Should().NotBeNull();
            result.Datos!.SpecialtyName.Should().Be("Cardiología");
            result.Datos.IsActive.Should().BeTrue();
        }

        /// <summary>CA-5: Especialidad no encontrada devuelve fallo con mensaje claro.</summary>
        [Fact]
        public async Task ObtenerPorId_EspecialidadNoExistente_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Specialty?)null);

            var result = await _service.GetByIdAsync(999);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("no encontrada",
                "debe indicar que la especialidad no existe");
        }

        // Nombre duplicado 

        /// <summary>CA-1 (creación): Especialidad con nombre duplicado rechazada.</summary>
        [Fact]
        public async Task CrearEspecialidad_NombreDuplicado_RetornaFallo()
        {
            _repoMock
                .Setup(r => r.ExistsByNameAsync("Cardiología"))
                .ReturnsAsync(true);

            var dto    = new CreateSpecialtyDto { SpecialtyName = "Cardiología" };
            var result = await _service.CreateAsync(dto);

            result.Exitoso.Should().BeFalse("no puede existir dos especialidades con el mismo nombre");
            result.Mensaje.ToLower().Should().ContainAny("existe", "duplicad");
        }

        /// <summary>CA-1 (creación): Nueva especialidad válida se crea correctamente.</summary>
        [Fact]
        public async Task CrearEspecialidad_NombreUnico_RetornaExito()
        {
            _repoMock
                .Setup(r => r.ExistsByNameAsync("Reumatología"))
                .ReturnsAsync(false);
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Specialty>()))
                .ReturnsAsync(new Specialty
                {
                    SpecialtyId   = 10,
                    SpecialtyName = "Reumatología",
                    IsActive      = true
                });

            var dto    = new CreateSpecialtyDto { SpecialtyName = "Reumatología" };
            var result = await _service.CreateAsync(dto);

            result.Exitoso.Should().BeTrue("una especialidad con nombre único debe crearse");
            result.Datos!.SpecialtyName.Should().Be("Reumatología");
            result.Datos.IsActive.Should().BeTrue();
        }
    }
}
