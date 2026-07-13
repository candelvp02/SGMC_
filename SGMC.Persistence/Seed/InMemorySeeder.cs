using SGMC.Domain.Entities.Users;
using SGMC.Domain.Entities.Appointments;
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

            // ── Proveedores de Seguro Médico ──
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

            // ── Especialidades ──
            context.Specialties.AddRange(
                new Specialty { SpecialtyId = 1, SpecialtyName = "Medicina General", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 2, SpecialtyName = "Cardiología", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 3, SpecialtyName = "Pediatría", IsActive = true, CreatedAt = now },
                new Specialty { SpecialtyId = 4, SpecialtyName = "Dermatología", IsActive = true, CreatedAt = now }
            );

            // ── Modalidades de disponibilidad ──
            var presencial = new AvailabilityMode { AvailabilityModeId = 1, AvailabilityMode1 = "Presencial", IsActive = true, CreatedAt = now };
            var virtualMode = new AvailabilityMode { AvailabilityModeId = 2, AvailabilityMode1 = "Virtual", IsActive = true, CreatedAt = now };
            context.AvailabilityModes.AddRange(presencial, virtualMode);

            context.SaveChanges();

            // ── Médicos de ejemplo ──
            var user1 = new User { UserId = 1, Email = "doctor1@sgmc.com", PasswordHash = "1234", RoleId = 2, IsActive = true, CreatedAt = now };
            var user2 = new User { UserId = 2, Email = "doctor2@sgmc.com", PasswordHash = "1234", RoleId = 2, IsActive = true, CreatedAt = now };
            context.Users.AddRange(user1, user2);
            context.SaveChanges();

            var person1 = new Person { PersonId = 1, FirstName = "Juan", LastName = "Pérez", DateOfBirth = new DateOnly(1980, 5, 10), IdentificationNumber = "00112223334", Gender = "M", UserId = user1.UserId };
            var person2 = new Person { PersonId = 2, FirstName = "Ana", LastName = "Gómez", DateOfBirth = new DateOnly(1985, 3, 22), IdentificationNumber = "00112223335", Gender = "F", UserId = user2.UserId };
            context.Persons.AddRange(person1, person2);
            context.SaveChanges();

            var doctor1 = new Doctor
            {
                DoctorId = person1.PersonId,
                SpecialtyId = 2, // Cardiología (ya sembrada arriba)
                LicenseNumber = "LIC-0001",
                PhoneNumber = "8091234567",
                YearsOfExperience = 10,
                Education = "Universidad Autónoma de Santo Domingo",
                Bio = "Especialista en cardiología con 10 años de experiencia.",
                ConsultationFee = 1500m,
                ClinicAddress = "Av. Winston Churchill, Santo Domingo",
                AvailabilityModeId = presencial.AvailabilityModeId,
                LicenseExpirationDate = new DateOnly(2027, 12, 31),
                CreatedAt = now,
                IsActive = true
            };

            var doctor2 = new Doctor
            {
                DoctorId = person2.PersonId,
                SpecialtyId = 3, // Pediatría (ya sembrada arriba)
                LicenseNumber = "LIC-0002",
                PhoneNumber = "8097654321",
                YearsOfExperience = 6,
                Education = "Pontificia Universidad Católica Madre y Maestra",
                Bio = "Pediatra con enfoque en atención infantil integral.",
                ConsultationFee = 1200m,
                ClinicAddress = "Av. 27 de Febrero, Santo Domingo",
                AvailabilityModeId = virtualMode.AvailabilityModeId,
                LicenseExpirationDate = new DateOnly(2026, 10, 15),
                CreatedAt = now,
                IsActive = true
            };

            context.Doctors.AddRange(doctor1, doctor2);

            context.SaveChanges();
        }
    }
}
