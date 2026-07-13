using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SGMC.Application.Dto.Users;
using SGMC.Application.Dto.System;
using SGMC.Application.Interfaces.Service;
using SGMC.Application.Services;
using SGMC.Application.Validators.Users;
using SGMC.Domain.Entities.Users;
using SGMC.Domain.Repositories.Appointments;
using SGMC.Domain.Repositories.Medical;
using SGMC.Domain.Repositories.Users;
using Xunit;

namespace SGMC.Tests.PBIs
{
    /// <summary>
    /// PBI #12 — Registro de Usuarios Médicos
    /// Criterios cubiertos:
    /// CA-1  Correo electrónico duplicado → error
    /// CA-2  Campos requeridos incompletos → error con indicación del campo
    /// CA-3  Especialidad debe pertenecer a la lista activa del sistema
    /// CA-4  Fecha de vencimiento de licencia no puede ser pasada
    /// CA-5  Contraseña con requisitos de seguridad
    /// CA-6  Género solo acepta "Masculino" / "Femenino"
    /// CA-7  Años de experiencia >= 0
    /// CA-8  Registro exitoso crea el médico correctamente
    /// </summary>
    public class PBI12_RegistroMedicoTests
    {
        // Infraestructura de mocks compartida
        private readonly Mock<IDoctorRepository> _repoMock;
        private readonly Mock<IAppointmentRepository> _apptRepoMock;
        private readonly Mock<IDoctorAvailabilityRepository> _availabilityRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IPersonRepository> _personRepoMock;
        private readonly Mock<ISpecialtyRepository> _specialtyRepoMock;
        private readonly IDoctorService _service;

        public PBI12_RegistroMedicoTests()
        {
            _repoMock = new Mock<IDoctorRepository>();
            _apptRepoMock = new Mock<IAppointmentRepository>();
            _availabilityRepoMock = new Mock<IDoctorAvailabilityRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _personRepoMock = new Mock<IPersonRepository>();
            _specialtyRepoMock = new Mock<ISpecialtyRepository>();
            var loggerMock = new Mock<ILogger<DoctorService>>();

            _service = new DoctorService(
                _repoMock.Object,
                _apptRepoMock.Object,
                _availabilityRepoMock.Object,
                loggerMock.Object,
                _userRepoMock.Object,
                _personRepoMock.Object,
                _specialtyRepoMock.Object);
        }

        // Helper: DTO completamente válido
        private static RegisterDoctorDto DtoValido() => new()
        {
            FirstName            = "Carlos",
            LastName             = "Rodríguez",
            DateOfBirth          = new DateOnly(1980, 3, 20),
            IdentificationNumber = "001-9876543-2",
            Gender               = "Masculino",
            Email                = "carlos.rodriguez@clinica.com",
            Password             = "DocPass1234",
            PhoneNumber          = "809-777-0001",
            SpecialtyId          = 1,
            LicenseNumber        = "LIC-2024-00999",
            LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)),
            YearsOfExperience    = 12,
            Education            = "Universidad Autónoma de Santo Domingo — Medicina",
            Bio                  = "Especialista en cardiología con más de 10 años de experiencia."
        };

        // -----------------------------------------------------
        //  CA-1  Correo electrónico duplicado
        // -----------------------------------------------------

        /// <summary>CA-1: Email ya registrado debe ser rechazado.</summary>
        [Fact]
        public async Task Registro_EmailDuplicado_RetornaFallo()
        {
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await _service.CreateAsync(DtoValido());

            result.Exitoso.Should().BeFalse("el correo ya existe en el sistema");
            result.Mensaje.ToLower().Should().ContainAny("email", "correo", "uso");
        }

        /// <summary>CA-1: Email con formato inválido rechazado en el validador.</summary>
        [Theory]
        [InlineData("")]
        [InlineData("noesun@mail")]
        [InlineData("falta-arroba-punto")]
        public void Validador_EmailInvalido_RetornaFallo(string email)
        {
            var dto = DtoValido();
            dto.Email = email;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("email"));
        }

        // -----------------------------------------------------
        //  CA-2  Campos requeridos
        // -----------------------------------------------------

        /// <summary>CA-2: DTO nulo retorna fallo descriptivo.</summary>
        [Fact]
        public async Task Registro_DtoNulo_RetornaFallo()
        {
            var result = await _service.CreateAsync(null!);

            result.Exitoso.Should().BeFalse();
            result.Mensaje.ToLower().Should().Contain("requerido");
        }

        /// <summary>CA-2: Nombre muy corto rechazado con indicación del campo.</summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("A")]
        public void Validador_NombreInvalido_RetornaFallo(string nombre)
        {
            var dto = DtoValido();
            dto.FirstName = nombre;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("nombre"));
        }

        /// <summary>CA-2: Apellido vacío rechazado con indicación del campo.</summary>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Z")]
        public void Validador_ApellidoInvalido_RetornaFallo(string apellido)
        {
            var dto = DtoValido();
            dto.LastName = apellido;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("apellido"));
        }

        // -----------------------------------------------------
        //  CA-3  Especialidad
        // -----------------------------------------------------

        /// <summary>CA-3: Especialidad ID 0 rechazada en validador.</summary>
        [Fact]
        public void Validador_EspecialidadCero_RetornaFallo()
        {
            var dto = DtoValido();
            dto.SpecialtyId = 0;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("especialidad"));
        }

        /// <summary>CA-3: Especialidad inexistente en BD rechaza el registro.</summary>
        [Fact]
        public async Task Registro_EspecialidadInexistente_RetornaFallo()
        {
            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _specialtyRepoMock
                .Setup(r => r.ExistsAsync(It.IsAny<short>()))
                .ReturnsAsync(false);  // especialidad no existe

            var result = await _service.CreateAsync(DtoValido());

            result.Exitoso.Should().BeFalse(
                "no se puede registrar con una especialidad que no existe en el sistema");
            result.Mensaje.ToLower().Should().ContainAny("especialidad", "existe");
        }

        // -----------------------------------------------------
        //  CA-4  Fecha de vencimiento de licencia
        // -----------------------------------------------------

        /// <summary>CA-4: Licencia vencida (fecha pasada) rechazada.</summary>
        [Fact]
        public void Validador_LicenciaVencida_RetornaFallo()
        {
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse("una licencia ya vencida no puede registrarse");
            result.Errores.Should().Contain(e =>
                e.ToLower().Contains("vencida") ||
                e.ToLower().Contains("expirada") ||
                e.ToLower().Contains("licencia"),
                "debe indicar que la licencia está vencida");
        }

        /// <summary>CA-4: Licencia que vence hoy es considerada válida (límite inclusivo).</summary>
        [Fact]
        public void Validador_LicenciaVenceHoy_RetornaExito()
        {
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue("una licencia que vence hoy todavía es vigente");
        }

        /// <summary>CA-4: Licencia con fecha futura es válida.</summary>
        [Fact]
        public void Validador_LicenciaFutura_RetornaExito()
        {
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue("una licencia con fecha futura debe aceptarse");
        }

        // -----------------------------------------------------
        //  CA-5  Seguridad de contraseña
        // -----------------------------------------------------

        /// <summary>CA-5: Contraseña sin mayúscula rechazada.</summary>
        [Fact]
        public void Validador_PasswordSinMayuscula_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "sinmayuscula1";

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-5: Contraseña sin número rechazada.</summary>
        [Fact]
        public void Validador_PasswordSinNumero_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "SoloLetras";

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-5: Contraseña de menos de 8 caracteres rechazada.</summary>
        [Fact]
        public void Validador_PasswordCorta_RetornaFallo()
        {
            var dto = DtoValido();
            dto.Password = "Ab1";

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse();
            result.Errores.Should().Contain(e => e.ToLower().Contains("contraseña"));
        }

        /// <summary>CA-5: Contraseña segura aceptada.</summary>
        [Theory]
        [InlineData("DocPass1234")]
        [InlineData("Clinica99X")]
        [InlineData("SecurePass1")]
        public void Validador_PasswordValida_RetornaExito(string password)
        {
            var dto = DtoValido();
            dto.Password = password;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue($"'{password}' cumple los requisitos de seguridad");
        }

        // -----------------------------------------------------
        //  CA-6  Género
        // -----------------------------------------------------

        /// <summary>CA-6: "Masculino" y "Femenino" son los únicos valores válidos.</summary>
        [Theory]
        [InlineData("Masculino")]
        [InlineData("Femenino")]
        public void Validador_GeneroValido_RetornaExito(string genero)
        {
            var dto = DtoValido();
            dto.Gender = genero;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue($"'{genero}' es un género aceptado para médicos");
        }

        /// <summary>CA-6: Abreviaciones o valores distintos rechazados.</summary>
        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        [InlineData("male")]
        [InlineData("Otro")]
        [InlineData("")]
        public void Validador_GeneroInvalido_RetornaFallo(string genero)
        {
            var dto = DtoValido();
            dto.Gender = genero;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse($"'{genero}' no debe aceptarse como género para médicos");
            result.Errores.Should().Contain(e => e.ToLower().Contains("género"));
        }

        // -----------------------------------------------------
        //  CA-7  Años de experiencia
        // -----------------------------------------------------

        /// <summary>CA-7: Años de experiencia negativos rechazados.</summary>
        [Fact]
        public void Validador_ExperienciaNegativa_RetornaFallo()
        {
            var dto = DtoValido();
            dto.YearsOfExperience = -1;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeFalse("los años de experiencia no pueden ser negativos");
            result.Errores.Should().Contain(e => e.ToLower().Contains("experiencia"));
        }

        /// <summary>CA-7: Cero años de experiencia es válido (médico recién graduado).</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(40)]
        public void Validador_ExperienciaCeroOPositiva_RetornaExito(int anios)
        {
            var dto = DtoValido();
            dto.YearsOfExperience = anios;

            var result = dto.IsValidDto();

            result.Exitoso.Should().BeTrue($"{anios} años de experiencia debe aceptarse");
        }

        // -----------------------------------------------------
        //  CA-8  Registro exitoso
        // -----------------------------------------------------

        /// <summary>CA-8: Registro con datos válidos crea el médico correctamente.</summary>
        [Fact]
        public async Task Registro_DatosValidos_CreaElMedico()
        {
            var dto = DtoValido();

            _userRepoMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);
            _personRepoMock
                .Setup(r => r.ExistsByIdentificationNumberAsync(dto.IdentificationNumber))
                .ReturnsAsync(false);
            _specialtyRepoMock
                .Setup(r => r.ExistsAsync(It.IsAny<short>()))
                .ReturnsAsync(true);

            var createdUser = new User { UserId = 20, Email = dto.Email, IsActive = true };
            _userRepoMock
                .Setup(r => r.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);
            _personRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Person>()))
                .ReturnsAsync((Person p) => p);

            var createdDoctor = new Doctor
            {
                DoctorId      = 20,
                LicenseNumber = dto.LicenseNumber,
                SpecialtyId   = dto.SpecialtyId,
                IsActive      = true,
                DoctorNavigation = new Person
                {
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName
                }
            };
            _repoMock
                .Setup(r => r.AddAsync(It.IsAny<Doctor>()))
                .ReturnsAsync(createdDoctor);
            _repoMock
                .Setup(r => r.GetByIdWithDetailsAsync(createdDoctor.DoctorId))
                .ReturnsAsync(createdDoctor);

            var result = await _service.CreateAsync(dto);

            result.Exitoso.Should().BeTrue("el registro con datos válidos debe completarse");
            result.Datos.Should().NotBeNull();
            result.Datos!.LicenseNumber.Should().Be(dto.LicenseNumber);
        }

        /// <summary>CA-8: DTO completamente válido pasa todas las validaciones.</summary>
        [Fact]
        public void Validador_DtoCompleto_RetornaExito()
        {
            var result = DtoValido().IsValidDto();

            result.Exitoso.Should().BeTrue("un DTO completo y válido debe pasar todas las validaciones");
            result.Errores.Should().BeEmpty();
        }
    }
}
