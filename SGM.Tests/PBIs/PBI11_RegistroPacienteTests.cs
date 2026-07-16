using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Users;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Base;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Insurance;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.PBIs
{
    /// <summary>
    /// PBI #11 — Registro de Usuarios Pacientes
    /// Criterios cubiertos:
    /// CA-1  Correo electrónico duplicado → error
    /// CA-2  Campos requeridos incompletos → error con indicación del campo
    /// CA-3  Género solo acepta "Masculino" / "Femenino"
    /// CA-4  Contraseña mínimo de seguridad (8 chars, mayúscula, número)
    /// CA-5  Proveedor de seguro debe ser uno activo
    /// CA-6  Registro exitoso crea el paciente correctamente
    /// </summary>
    public class PBI11_RegistroPacienteTests
    {
        // Infraestructura de mocks compartida
        private readonly Mock<IPatientRepository>          _repoMock;
        private readonly Mock<IUserRepository>             _userRepoMock;
        private readonly Mock<IPersonRepository>           _personRepoMock;
        private readonly Mock<IInsuranceProviderRepository> _insuranceRepoMock;
        private readonly IPatientService                   _service;

        public PBI11_RegistroPacienteTests()
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

        // Helper: DTO completamente válido
        private static RegisterPatientDto DtoValido() => new()
        {
            FirstName              = "María",
            LastName               = "Pérez",
            DateOfBirth            = new DateOnly(1990, 5, 15),
            IdentificationNumber   = "001-1234567-8",
            Gender                 = "F",
            Email                  = "maria.perez@correo.com",
            Password               = "Segura1234",
            PhoneNumber            = "809-555-0001",
            Address                = "Calle Primera #1",
            EmergencyContactName   = "Carlos Pérez",
            EmergencyContactPhone  = "809-555-0002",
            BloodType              = "O+",
            Allergies              = "Ninguna",
            InsuranceProviderId    = 1
        };

        // -----------------------------------------------------
        //  CA-1  Correo electrónico duplicado
        // -----------------------------------------------------

        /// <summary>CA-1: El sistema rechaza el registro si el email ya existe.</summary>
        [Fact]
        public async Task Registro_EmailDuplicado_RetornaFallo()
        {
            // Arrange
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync("maria.perez@correo.com"))
                .ReturnsAsync(true);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(DtoValido());

            // Assert
            result.Exitoso.Should().BeFalse(
                "el sistema debe rechazar el registro cuando el correo ya existe");
            result.Mensaje.ToLower().Should().ContainAny("email", "correo", "uso",
                "debe indicar que el correo ya está en uso");
        }

        /// <summary>CA-1: El mismo email no puede registrarse dos veces.</summary>
        [Fact]
        public void Validador_EmailVacio_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Email = string.Empty;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("email"),
                "el email vacío debe generar un error de validación");
        }

        // -----------------------------------------------------
        //  CA-2  Campos requeridos
        // -----------------------------------------------------

        /// <summary>CA-2: DTO nulo retorna fallo con mensaje descriptivo.</summary>
        [Fact]
        public async Task Registro_DtoNulo_RetornaFallo()
        {
            var result = await _service.CreateAsync(null!);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("requerido",
                "debe indicar que los datos son requeridos");
        }

        /// <summary>CA-2: Nombre vacío debe ser rechazado en validación.</summary>
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("A")]   // menos de 2 caracteres
        public void Validador_NombreInvalido_RetornaFallo(string nombre)
        {
            var dto = DtoValido();
            dto.FirstName = nombre;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("nombre"),
                "un nombre inválido debe generar error indicando el campo");
        }

        /// <summary>CA-2: Apellido vacío debe ser rechazado en validación.</summary>
        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("B")]
        public void Validador_ApellidoInvalido_RetornaFallo(string apellido)
        {
            var dto = DtoValido();
            dto.LastName = apellido;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("apellido"),
                "un apellido inválido debe generar error indicando el campo");
        }

        /// <summary>CA-2: Proveedor de seguro Id 0 o negativo rechazado.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validador_SeguroIdInvalido_RetornaFallo(int seguroId)
        {
            var dto = DtoValido();
            dto.InsuranceProviderId = seguroId;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("seguro"),
                "debe indicar que el proveedor de seguro es inválido");
        }

        // -----------------------------------------------------
        //  CA-3  Género
        // -----------------------------------------------------

        /// <summary>CA-3: "M" y "F" son los únicos géneros válidos para pacientes.</summary>
        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        public void Validador_GeneroValido_RetornaExito(string genero)
        {
            var dto = DtoValido();
            dto.Gender = genero;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue(
                $"'{genero}' es un género válido para el registro de pacientes");
        }

        /// <summary>CA-3: Géneros distintos a M/F deben ser rechazados.</summary>
        [Theory]
        [InlineData("Masculino")]
        [InlineData("Femenino")]
        [InlineData("male")]
        [InlineData("Otro")]
        [InlineData("")]
        [InlineData("X")]
        public void Validador_GeneroInvalido_RetornaFallo(string genero)
        {
            var dto = DtoValido();
            dto.Gender = genero;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse(
                $"'{genero}' no debe aceptarse como género para pacientes");
            result.Errores.Should().Contain(e => e.ToLower().Contains("género"));
        }

        // -----------------------------------------------------
        //  CA-4  Seguridad de contraseña
        // -----------------------------------------------------

        /// <summary>CA-4: Contraseña menor de 8 caracteres rechazada.</summary>
        [Fact]
        public void Validador_PasswordMenorDe8Caracteres_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "Ab1";   // solo 3 chars

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-4: Contraseña sin mayúscula rechazada.</summary>
        [Fact]
        public void Validador_PasswordSinMayuscula_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "sinmayus1";

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-4: Contraseña sin número rechazada.</summary>
        [Fact]
        public void Validador_PasswordSinNumero_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "SinNumeroAqui";

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-4: Contraseña que cumple todos los requisitos aceptada.</summary>
        [Theory]
        [InlineData("Segura1234")]
        [InlineData("MiPass99!")]
        [InlineData("AbcDef12")]
        public void Validador_PasswordValida_RetornaExito(string password)
        {
            var dto = DtoValido();
            dto.Password = password;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue(
                $"'{password}' cumple con los requisitos mínimos de seguridad");
        }

        // -----------------------------------------------------
        //  CA-5  Proveedor de seguro inexistente / inactivo
        // -----------------------------------------------------

        /// <summary>CA-5: Proveedor de seguro inexistente rechaza el registro.</summary>
        [Fact]
        public async Task Registro_SeguroInexistente_RetornaFallo()
        {
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _insuranceRepoMock
                .Setup(r => r.ExistsAsync(It.IsAny<int>()))
                .ReturnsAsync(false);   // seguro no existe

            var result = await _service.CreateAsync(DtoValido());

            result.Exitoso.Should().BeFalse(
                "no se puede registrar con un proveedor de seguro que no existe en el sistema");
            result.Mensaje.ToLower().Should().ContainAny("seguro", "proveedor",
                "debe indicar que el proveedor de seguro no existe");
        }

        // -----------------------------------------------------
        //  CA-6  Registro exitoso
        // -----------------------------------------------------

        /// <summary>CA-6: Registro con datos válidos crea el paciente correctamente.</summary>
        [Fact]
        public async Task Registro_DatosValidos_CreaElPaciente()
        {
            var dto = DtoValido();

            // Arrange — ningún duplicado, seguro existe
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(dto.IdentificationNumber))
                .ReturnsAsync(false);
            _insuranceRepoMock
                .Setup(r => r.ExistsAsync(dto.InsuranceProviderId))
                .ReturnsAsync(true);

            var createdUser = new User { UserId = 10, Email = dto.Email, IsActive = true };
            _userRepoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);
            _personRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Person>()))
                .ReturnsAsync((Person p) => p);

            var createdPatient = new Patient
            {
                PatientId = 10,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                BloodType = dto.BloodType,
                InsuranceProviderId = dto.InsuranceProviderId,
                IsActive = true,
                PatientNavigation = new Person
                {
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName
                }
            };
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Patient>()))
                .ReturnsAsync(createdPatient);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            result.Exitoso.Should().BeTrue("el registro con datos válidos debe completarse");
            result.Datos.Should().NotBeNull();
            result.Datos!.IsActive.Should().BeTrue();
        }

        /// <summary>CA-6: El DTO completamente válido pasa todas las validaciones.</summary>
        [Fact]
        public void Validador_DtoCompleto_RetornaExito()
        {
            var result = DtoValido().IsValidDto();

            result.Exitoso.Should().BeTrue(
                "un DTO con todos los datos correctos debe pasar la validación");
            result.Errores.Should().BeEmpty();
        }
    }
}
