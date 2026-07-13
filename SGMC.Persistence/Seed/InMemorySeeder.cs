using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.System;
using SGMC.Persistence.Context;

namespace SGMC.Persistence.Seed
{
    /// <summary>
    /// Datos de catálogo necesarios para que los flujos de Usuarios/Autenticación/Pacientes
    /// (PBIs #11, #12, #13, #20, #22, #25, #26) funcionen al correr la aplicación contra el
    /// proveedor EF Core InMemory, ya que este arranca con la base de datos completamente vacía
    /// (no hay migraciones ni datos precargados como en SQL Server).
    ///
    /// Solo siembra lo que necesitan esas secciones. Otros módulos (Citas, Historiales médicos,
    /// Disponibilidad de médicos, Reportes, etc.) quedan fuera a propósito — cada quien puede
    /// agregar su propio seed aquí mismo cuando le toque migrar su parte.
    /// </summary>
    public static class InMemorySeeder
    {
        public static void Seed(HealtSyncContext context)
        {
            // Evita volver a sembrar si el contexto ya tiene datos (por ejemplo, si Seed
            // se invoca más de una vez sobre la misma base en memoria).
            if (context.Roles.Any())
                return;

            var now = DateTime.Now;

            // ── Roles ── (IDs usados como literales en PatientService/DoctorService/AuthService)
            context.Roles.AddRange(
                new Role { RoleId = 1, RoleName = "Administrador", IsActive = true, CreatedAt = now },
                new Role { RoleId = 2, RoleName = "Médico", IsActive = true, CreatedAt = now },
                new Role { RoleId = 3, RoleName = "Paciente", IsActive = true, CreatedAt = now }
            );

            // ── Tipos de Red (requeridos por InsuranceProvider) ──
            var redPreferente = new NetworkType
            {
                NetworkTypeId = 1,
                Name = "Red Preferente",
                Description = "Cobertura amplia con proveedores preferentes",
                IsActive = true,
                CreatedAt = now
            };
            var redAmpliada = new NetworkType
            {
                NetworkTypeId = 2,
                Name = "Red Ampliada",
                Description = "Cobertura estándar con red ampliada de proveedores",
                IsActive = true,
                CreatedAt = now
            };
            context.NetworkTypes.AddRange(redPreferente, redAmpliada);

            // ── Proveedores de Seguro Médico ── (PBI #11 registro, PBI #25 actualización)
            context.InsuranceProviders.AddRange(
                new InsuranceProvider
                {
                    InsuranceProviderId = 1,
                    Name = "ARS Humano",
                    PhoneNumber = "809-555-0101",
                    Email = "contacto@arshumano.test",
                    Website = "https://arshumano.test",
                    Address = "Av. Principal 123",
                    City = "Santo Domingo",
                    State = "Distrito Nacional",
                    Country = "República Dominicana",
                    ZipCode = "10101",
                    CoverageDetails = "Cobertura general de consultas y hospitalización",
                    LogoUrl = string.Empty,
                    IsPreferred = true,
                    NetworkTypeId = redPreferente.NetworkTypeId,
                    CustomerSupportContact = "809-555-0102",
                    AcceptedRegions = "Nacional",
                    MaxCoverageAmount = 500000m,
                    IsActive = true,
                    CreatedAt = now
                },
                new InsuranceProvider
                {
                    InsuranceProviderId = 2,
                    Name = "Seguros Senasa",
                    PhoneNumber = "809-555-0201",
                    Email = "contacto@senasa.test",
                    Website = "https://senasa.test",
                    Address = "Calle Salud 45",
                    City = "Santiago",
                    State = "Santiago",
                    Country = "República Dominicana",
                    ZipCode = "51000",
                    CoverageDetails = "Cobertura básica de consultas médicas",
                    LogoUrl = string.Empty,
                    IsPreferred = false,
                    NetworkTypeId = redAmpliada.NetworkTypeId,
                    CustomerSupportContact = "809-555-0202",
                    AcceptedRegions = "Nacional",
                    MaxCoverageAmount = 250000m,
                    IsActive = true,
                    CreatedAt = now
                }
            );

            // ── Especialidades ── (PBI #12 registro de médico, PBI #26 visualización)
            context.Specialties.AddRange(
                new Specialty { SpecialtyId = 1, SpecialtyName = "Medicina General", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 2, SpecialtyName = "Cardiología", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 3, SpecialtyName = "Pediatría", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 4, SpecialtyName = "Dermatología", IsActive = true, CreatedAt = now }
            );

            context.SaveChanges();
        }
    }
}
