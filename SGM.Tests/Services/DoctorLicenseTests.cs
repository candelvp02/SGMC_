using FluentAssertions;
using SGMC.Application.Dto.Users;
using SGMC.Application.Validators.Users;
using Xunit;

namespace SGMC.Tests.Services
{
    public class DoctorLicenseTests
    {
        // ════════════════════════════════════════════════════════════════
        // HELPER — DTO válido base para modificar en cada test
        // ════════════════════════════════════════════════════════════════

        private static RegisterDoctorDto DtoValido() => new()
        {
            FirstName = "Ana",
            LastName = "García",
            IdentificationNumber = "001-1234567-8",
            Gender = "Femenino",
            Email = "ana.garcia@hospital.com",
            Password = "Pass@1234",
            SpecialtyId = 1,
            LicenseNumber = "LIC-2024-00123",
            LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddYears(1)),
            YearsOfExperience = 5,
            Education = "Universidad Autónoma de Santo Domingo"
        };

        // ════════════════════════════════════════════════════════════════
        // 1. LICENCIA VENCIDA
        // ════════════════════════════════════════════════════════════════

        [Fact]
        public void IsValidDto_LicenciaVencida_DebeRetornarFallo()
        {
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e =>
                e.Contains("vencida"),
                "una licencia con fecha pasada debe ser rechazada");
        }

        [Fact]
        public void IsValidDto_LicenciaVenceHoy_DebeRetornarExito()
        {
            // Una licencia que vence HOY todavía es válida
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeTrue();
        }

        [Fact]
        public void IsValidDto_LicenciaVenceFuturo_DebeRetornarExito()
        {
            var dto = DtoValido();
            dto.LicenseExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(6));

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeTrue();
        }

        // ════════════════════════════════════════════════════════════════
        // 2. CAMPOS VACÍOS
        // ════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("A")]   // menos de 2 caracteres
        public void IsValidDto_NombreVacioOCorto_DebeRetornarFallo(string firstName)
        {
            var dto = DtoValido();
            dto.FirstName = firstName;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e => e.Contains("nombre"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("B")]
        public void IsValidDto_ApellidoVacioOCorto_DebeRetornarFallo(string lastName)
        {
            var dto = DtoValido();
            dto.LastName = lastName;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e => e.Contains("apellido"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("noesunmail")]
        [InlineData("falta@punto")]
        public void IsValidDto_EmailInvalido_DebeRetornarFallo(string email)
        {
            var dto = DtoValido();
            dto.Email = email;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e => e.Contains("email"));
        }

        [Fact]
        public void IsValidDto_EspecialidadCero_DebeRetornarFallo()
        {
            var dto = DtoValido();
            dto.SpecialtyId = 0;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e => e.Contains("especialidad"));
        }

        // ════════════════════════════════════════════════════════════════
        // 3. GÉNERO: solo "Masculino" o "Femenino"
        // ════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData("Masculino")]
        [InlineData("Femenino")]
        public void IsValidDto_GeneroValido_DebeRetornarExito(string gender)
        {
            var dto = DtoValido();
            dto.Gender = gender;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeTrue(
                $"'{gender}' es un valor de género válido");
        }

        [Theory]
        [InlineData("M")]
        [InlineData("F")]
        [InlineData("m")]
        [InlineData("f")]
        [InlineData("masculino")]   
        [InlineData("femenino")]
        [InlineData("Male")]
        [InlineData("Female")]
        [InlineData("Otro")]
        [InlineData("")]
        [InlineData("  ")]
        public void IsValidDto_GeneroAbreviadoOInvalido_DebeRetornarFallo(string gender)
        {
            var dto = DtoValido();
            dto.Gender = gender;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse(
                $"'{gender}' no debe aceptarse; solo 'Masculino' o 'Femenino'");
            resultado.Errores.Should().Contain(e => e.Contains("género"));
        }

        // ════════════════════════════════════════════════════════════════
        // 4. AÑOS DE EXPERIENCIA NEGATIVOS
        // ════════════════════════════════════════════════════════════════

        [Fact]
        public void IsValidDto_ExperienciaNegativa_DebeRetornarFallo()
        {
            var dto = DtoValido();
            dto.YearsOfExperience = -1;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeFalse();
            resultado.Errores.Should().Contain(e => e.Contains("experiencia"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(30)]
        public void IsValidDto_ExperienciaCeroOPositiva_DebeRetornarExito(int years)
        {
            var dto = DtoValido();
            dto.YearsOfExperience = years;

            var resultado = dto.IsValidDto();

            resultado.Exitoso.Should().BeTrue();
        }

        // ════════════════════════════════════════════════════════════════
        // 5. DTO COMPLETAMENTE VÁLIDO
        // ════════════════════════════════════════════════════════════════

        [Fact]
        public void IsValidDto_DtoCompleto_DebeRetornarExito()
        {
            var resultado = DtoValido().IsValidDto();

            resultado.Exitoso.Should().BeTrue(
                "un DTO con todos los datos correctos debe pasar todas las validaciones");
            resultado.Errores.Should().BeEmpty();
        }
    }
}