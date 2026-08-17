using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Entities.Users;
using SGMC.Infrastructure.Dependencies;
using SGMC.Infrastructure.Services;
using SGMC.Persistence.Context;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext — InMemory para pruebas
builder.Services.AddDbContext<HealtSyncContext>(options =>
    options.UseInMemoryDatabase("HealtSyncDb"));

// Capas de application y persistence
builder.Services.AddUserDependencies();
builder.Services.AddDoctorDependencies();
builder.Services.AddPatientDependencies();
builder.Services.AddAppointmentDependencies();
builder.Services.AddAvailabilityDependencies();
builder.Services.AddInsuranceProviderDependencies();
builder.Services.AddMedicalRecordDependencies();
builder.Services.AddNotificationDependencies();
builder.Services.AddAppointmentNotificationDependencies();
builder.Services.AddReminderDependencies();
builder.Services.AddReportDependencies();
builder.Services.AddSpecialtyDependencies();
builder.Services.AddAuthDependencies();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — requerido por app.UseCors("AllowAll") más abajo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseAuthentication();
    app.UseAuthorization();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Seed de datos en memoria
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HealtSyncContext>();
    SeedData(context);
}

app.Run();

// ═══════════════════════════════════════════════════════════════════════════
// SEED DATA
// ═══════════════════════════════════════════════════════════════════════════
static void SeedData(HealtSyncContext context)
{
    // ── Roles ─────────────────────────────────────────────────────────────
    var rolePaciente = new Role { RoleId = 1, RoleName = "Paciente", CreatedAt = DateTime.Now, IsActive = true };
    var roleDoctor = new Role { RoleId = 2, RoleName = "Médico", CreatedAt = DateTime.Now, IsActive = true };
    var roleAdmin = new Role { RoleId = 3, RoleName = "Administrador", CreatedAt = DateTime.Now, IsActive = true };
    context.Roles.AddRange(rolePaciente, roleDoctor, roleAdmin);

    // ── Modos de disponibilidad ────────────────────────────────────────────
    var presencial = new AvailabilityMode { AvailabilityModeId = 1, AvailabilityMode1 = "Presencial", CreatedAt = DateTime.Now, IsActive = true };
    var virtualMode = new AvailabilityMode { AvailabilityModeId = 2, AvailabilityMode1 = "Virtual", CreatedAt = DateTime.Now, IsActive = true };
    context.AvailabilityModes.AddRange(presencial, virtualMode);

    // ── Especialidades ─────────────────────────────────────────────────────
    var cardiologia = new Specialty { SpecialtyId = 1, SpecialtyName = "Cardiología", CreatedAt = DateTime.Now, IsActive = true };
    var pediatria = new Specialty { SpecialtyId = 2, SpecialtyName = "Pediatría", CreatedAt = DateTime.Now, IsActive = true };
    var dermatologia = new Specialty { SpecialtyId = 3, SpecialtyName = "Dermatología", CreatedAt = DateTime.Now, IsActive = true };
    var ginecologia = new Specialty { SpecialtyId = 4, SpecialtyName = "Ginecología", CreatedAt = DateTime.Now, IsActive = true };
    var neurologia = new Specialty { SpecialtyId = 5, SpecialtyName = "Neurología", CreatedAt = DateTime.Now, IsActive = false };
    var traumatologia = new Specialty { SpecialtyId = 6, SpecialtyName = "Traumatología", CreatedAt = DateTime.Now, IsActive = true };
    var oftalmologia = new Specialty { SpecialtyId = 7, SpecialtyName = "Oftalmología", CreatedAt = DateTime.Now, IsActive = true };
    var psiquiatria = new Specialty { SpecialtyId = 8, SpecialtyName = "Psiquiatría", CreatedAt = DateTime.Now, IsActive = true };
    var endocrinologia = new Specialty { SpecialtyId = 9, SpecialtyName = "Endocrinología", CreatedAt = DateTime.Now, IsActive = true };
    var otorrino = new Specialty { SpecialtyId = 10, SpecialtyName = "Otorrinolaringología", CreatedAt = DateTime.Now, IsActive = false };
    context.Specialties.AddRange(cardiologia, pediatria, dermatologia, ginecologia, neurologia,
        traumatologia, oftalmologia, psiquiatria, endocrinologia, otorrino);

    // ── Tipos de red ───────────────────────────────────────────────────────
    var hmo = new NetworkType { NetworkTypeId = 1, Name = "HMO", Description = "Health Maintenance Organization", CreatedAt = DateTime.Now, IsActive = true };
    var ppo = new NetworkType { NetworkTypeId = 2, Name = "PPO", Description = "Preferred Provider Organization", CreatedAt = DateTime.Now, IsActive = true };
    var epo = new NetworkType { NetworkTypeId = 3, Name = "EPO", Description = "Exclusive Provider Organization", CreatedAt = DateTime.Now, IsActive = true };
    context.NetworkTypes.AddRange(hmo, ppo, epo);

    // ── Statuses ─────────────────────────────────────────────────────────
    var pendiente = new Status { StatusId = 1, StatusName = "Pendiente" };
    var confirmada = new Status { StatusId = 2, StatusName = "Confirmada" };
    var cancelada = new Status { StatusId = 3, StatusName = "Cancelada" };
    var completada = new Status { StatusId = 4, StatusName = "Completada" };
    var rechazada = new Status { StatusId = 5, StatusName = "Rechazada" };
    context.Statuses.AddRange(pendiente, confirmada, cancelada, completada, rechazada);

    context.SaveChanges();

    // ── Proveedores de seguro ──────────────────────────────────────────────
    var senasa = new InsuranceProvider { InsuranceProviderId = 1, Name = "SENASA", PhoneNumber = "809-200-8080", Email = "info@senasa.gob.do", Website = "https://www.senasa.gob.do", Address = "Av. Tiradentes #30", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = true, NetworkTypeId = hmo.NetworkTypeId, CustomerSupportContact = "809-200-8080", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = true };
    var humano = new InsuranceProvider { InsuranceProviderId = 2, Name = "Humano", PhoneNumber = "809-535-6262", Email = "info@humano.com.do", Website = "https://www.humano.com.do", Address = "Av. Abraham Lincoln #1008", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = true, NetworkTypeId = ppo.NetworkTypeId, CustomerSupportContact = "809-535-6262", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = true };
    var mapfre = new InsuranceProvider { InsuranceProviderId = 3, Name = "MAPFRE Salud", PhoneNumber = "809-476-8181", Email = "info@mapfre.com.do", Website = "https://www.mapfre.com.do", Address = "Av. Winston Churchill #1099", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = false, NetworkTypeId = ppo.NetworkTypeId, CustomerSupportContact = "809-476-8181", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = true };
    var reservas = new InsuranceProvider { InsuranceProviderId = 4, Name = "Reservas Salud", PhoneNumber = "809-960-1212", Email = "info@banreservas.com.do", Website = "https://www.banreservas.com.do", Address = "Av. Isabel Aguiar", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = false, NetworkTypeId = hmo.NetworkTypeId, CustomerSupportContact = "809-960-1212", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = false };
    var palic = new InsuranceProvider { InsuranceProviderId = 5, Name = "ARS Palic Salud", PhoneNumber = "809-544-8000", Email = "info@palic.com.do", Website = "https://www.palic.com.do", Address = "Av. John F. Kennedy #123", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = false, NetworkTypeId = epo.NetworkTypeId, CustomerSupportContact = "809-544-8000", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = true };
    var universal = new InsuranceProvider { InsuranceProviderId = 6, Name = "ARS Universal", PhoneNumber = "809-682-9992", Email = "info@arsuniversal.com.do", Website = "https://www.arsuniversal.com.do", Address = "Av. Lope de Vega #29", City = "Santo Domingo", State = "Distrito Nacional", Country = "República Dominicana", ZipCode = "10101", LogoUrl = "", IsPreferred = true, NetworkTypeId = ppo.NetworkTypeId, CustomerSupportContact = "809-682-9992", AcceptedRegions = "Nacional", CreatedAt = DateTime.Now, IsActive = true };
    var yunen = new InsuranceProvider { InsuranceProviderId = 7, Name = "Yunen Seguros", PhoneNumber = "809-565-1010", Email = "info@yunenseguros.com.do", Website = "https://www.yunenseguros.com.do", Address = "Av. 27 de Febrero #390", City = "Santiago", State = "Santiago", Country = "República Dominicana", ZipCode = "51000", LogoUrl = "", IsPreferred = false, NetworkTypeId = hmo.NetworkTypeId, CustomerSupportContact = "809-565-1010", AcceptedRegions = "Regional", CreatedAt = DateTime.Now, IsActive = false };
    context.InsuranceProviders.AddRange(senasa, humano, mapfre, reservas, palic, universal, yunen);

    // ── Usuarios ───────────────────────────────────────────────────────────
    var users = new List<User>
    {
        new User { UserId = 1, Email = "doctor1@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 2, Email = "doctor2@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 3, Email = "doctor3@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 4, Email = "doctor4@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 5, Email = "doctor5@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = false },
        new User { UserId = 6, Email = "doctor6@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 7, Email = "doctor7@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 8, Email = "doctor8@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 9, Email = "doctor9@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = false },
        new User { UserId = 10, Email = "doctor10@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true },

        new User { UserId = 11, Email = "paciente1@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 12, Email = "paciente2@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 13, Email = "paciente3@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 14, Email = "paciente4@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 15, Email = "paciente5@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 16, Email = "paciente6@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 17, Email = "paciente7@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = false },
        new User { UserId = 18, Email = "paciente8@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true },

        new User { UserId = 19, Email = "admin@sgmc.com", PasswordHash = "1234", RoleId = roleAdmin.RoleId, CreatedAt = DateTime.Now, IsActive = true },
        new User { UserId = 20, Email = "admin2@sgmc.com", PasswordHash = "1234", RoleId = roleAdmin.RoleId, CreatedAt = DateTime.Now, IsActive = true },
    };
    context.Users.AddRange(users);
    context.SaveChanges();

    // ── Personas ───────────────────────────────────────────────────────────
    var persons = new List<Person>
    {
        new Person { PersonId = 1, FirstName = "Juan", LastName = "Pérez", DateOfBirth = new DateOnly(1980, 5, 10), IdentificationNumber = "001-1234567-1", Gender = "Masculino", UserId = 1 },
        new Person { PersonId = 2, FirstName = "Ana", LastName = "Gómez", DateOfBirth = new DateOnly(1985, 3, 22), IdentificationNumber = "001-1234567-2", Gender = "Femenino", UserId = 2 },
        new Person { PersonId = 3, FirstName = "Luis", LastName = "Fernández", DateOfBirth = new DateOnly(1978, 11, 2), IdentificationNumber = "001-1234567-3", Gender = "Masculino", UserId = 3 },
        new Person { PersonId = 4, FirstName = "Carmen", LastName = "Reyes", DateOfBirth = new DateOnly(1983, 9, 18), IdentificationNumber = "001-1234567-4", Gender = "Femenino", UserId = 4 },
        new Person { PersonId = 5, FirstName = "Miguel", LastName = "Cruz", DateOfBirth = new DateOnly(1975, 2, 27), IdentificationNumber = "001-1234567-5", Gender = "Masculino", UserId = 5 },
        new Person { PersonId = 6, FirstName = "Rosa", LastName = "Jiménez", DateOfBirth = new DateOnly(1988, 6, 14), IdentificationNumber = "001-1234567-6", Gender = "Femenino", UserId = 6 },
        new Person { PersonId = 7, FirstName = "Rafael", LastName = "Ortiz", DateOfBirth = new DateOnly(1982, 4, 9), IdentificationNumber = "001-1234567-7", Gender = "Masculino", UserId = 7 },
        new Person { PersonId = 8, FirstName = "Patricia", LastName = "Vargas", DateOfBirth = new DateOnly(1990, 8, 30), IdentificationNumber = "001-1234567-8", Gender = "Femenino", UserId = 8 },
        new Person { PersonId = 9, FirstName = "Eduardo", LastName = "Núñez", DateOfBirth = new DateOnly(1979, 12, 5), IdentificationNumber = "001-1234567-9", Gender = "Masculino", UserId = 9 },
        new Person { PersonId = 10, FirstName = "Sofía", LastName = "Batista", DateOfBirth = new DateOnly(1992, 1, 20), IdentificationNumber = "001-1234567-10", Gender = "Femenino", UserId = 10 },

        new Person { PersonId = 11, FirstName = "Carlos", LastName = "Martínez", DateOfBirth = new DateOnly(1990, 7, 15), IdentificationNumber = "001-2234567-1", Gender = "Masculino", UserId = 11 },
        new Person { PersonId = 12, FirstName = "María", LastName = "López", DateOfBirth = new DateOnly(1995, 1, 30), IdentificationNumber = "001-2234567-2", Gender = "Femenino", UserId = 12 },
        new Person { PersonId = 13, FirstName = "José", LastName = "Ramírez", DateOfBirth = new DateOnly(1988, 3, 12), IdentificationNumber = "001-2234567-3", Gender = "Masculino", UserId = 13 },
        new Person { PersonId = 14, FirstName = "Laura", LastName = "Sánchez", DateOfBirth = new DateOnly(1993, 5, 25), IdentificationNumber = "001-2234567-4", Gender = "Femenino", UserId = 14 },
        new Person { PersonId = 15, FirstName = "Andrés", LastName = "Peña", DateOfBirth = new DateOnly(1985, 10, 8), IdentificationNumber = "001-2234567-5", Gender = "Masculino", UserId = 15 },
        new Person { PersonId = 16, FirstName = "Gabriela", LastName = "Castillo", DateOfBirth = new DateOnly(1997, 2, 17), IdentificationNumber = "001-2234567-6", Gender = "Femenino", UserId = 16 },
        new Person { PersonId = 17, FirstName = "Tomás", LastName = "Medina", DateOfBirth = new DateOnly(1980, 9, 3), IdentificationNumber = "001-2234567-7", Gender = "Masculino", UserId = 17 },
        new Person { PersonId = 18, FirstName = "Valentina", LastName = "Rosario", DateOfBirth = new DateOnly(1999, 12, 11), IdentificationNumber = "001-2234567-8", Gender = "Femenino", UserId = 18 },

        new Person { PersonId = 19, FirstName = "Admin", LastName = "SGMC", DateOfBirth = new DateOnly(1975, 6, 1), IdentificationNumber = "001-3234567-1", Gender = "Masculino", UserId = 19 },
        new Person { PersonId = 20, FirstName = "Rosa", LastName = "Administradora", DateOfBirth = new DateOnly(1982, 4, 19), IdentificationNumber = "001-3234567-2", Gender = "Femenino", UserId = 20 },
    };
    context.Persons.AddRange(persons);
    context.SaveChanges();

    // ── Doctores ───────────────────────────────────────────────────────────
    var doctors = new List<Doctor>
    {
        new Doctor { DoctorId = 1, SpecialtyId = cardiologia.SpecialtyId, LicenseNumber = "LIC-0001", PhoneNumber = "809-123-4567", YearsOfExperience = 10, Education = "Universidad Autónoma de Santo Domingo", Bio = "Especialista en cardiología con 10 años de experiencia.", ConsultationFee = 1500m, ClinicAddress = "Av. Winston Churchill, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2027, 12, 31), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 2, SpecialtyId = pediatria.SpecialtyId, LicenseNumber = "LIC-0002", PhoneNumber = "809-765-4321", YearsOfExperience = 6, Education = "Pontificia Universidad Católica Madre y Maestra", Bio = "Pediatra con enfoque en atención infantil integral.", ConsultationFee = 1200m, ClinicAddress = "Av. 27 de Febrero, Santo Domingo", AvailabilityModeId = virtualMode.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2026, 10, 15), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 3, SpecialtyId = dermatologia.SpecialtyId, LicenseNumber = "LIC-0003", PhoneNumber = "809-222-3333", YearsOfExperience = 12, Education = "Universidad Iberoamericana", Bio = "Dermatólogo clínico y estético.", ConsultationFee = 1800m, ClinicAddress = "Av. Abraham Lincoln, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2028, 3, 20), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 4, SpecialtyId = ginecologia.SpecialtyId, LicenseNumber = "LIC-0004", PhoneNumber = "809-444-5555", YearsOfExperience = 15, Education = "Universidad Nacional Pedro Henríquez Ureña", Bio = "Ginecóloga con enfoque en salud reproductiva.", ConsultationFee = 2000m, ClinicAddress = "Av. Sarasota, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2027, 6, 30), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 5, SpecialtyId = neurologia.SpecialtyId, LicenseNumber = "LIC-0005", PhoneNumber = "809-666-7777", YearsOfExperience = 20, Education = "Universidad Autónoma de Santo Domingo", Bio = "Neurólogo, actualmente fuera de servicio.", ConsultationFee = 2200m, ClinicAddress = "Av. Independencia, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2025, 11, 1), CreatedAt = DateTime.Now, IsActive = false },
        new Doctor { DoctorId = 6, SpecialtyId = traumatologia.SpecialtyId, LicenseNumber = "LIC-0006", PhoneNumber = "809-888-9999", YearsOfExperience = 9, Education = "Universidad Tecnológica de Santiago", Bio = "Traumatólogo deportivo.", ConsultationFee = 1700m, ClinicAddress = "Av. Estrella Sadhalá, Santiago", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2028, 8, 12), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 7, SpecialtyId = oftalmologia.SpecialtyId, LicenseNumber = "LIC-0007", PhoneNumber = "809-101-2020", YearsOfExperience = 7, Education = "Universidad Iberoamericana", Bio = "Oftalmólogo, cirugía refractiva.", ConsultationFee = 1600m, ClinicAddress = "Av. Charles de Gaulle, Santo Domingo Este", AvailabilityModeId = virtualMode.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2026, 5, 5), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 8, SpecialtyId = psiquiatria.SpecialtyId, LicenseNumber = "LIC-0008", PhoneNumber = "809-303-4040", YearsOfExperience = 11, Education = "Universidad Autónoma de Santo Domingo", Bio = "Psiquiatra, terapia cognitivo-conductual.", ConsultationFee = 2100m, ClinicAddress = "Av. Roberto Pastoriza, Santo Domingo", AvailabilityModeId = virtualMode.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2027, 9, 9), CreatedAt = DateTime.Now, IsActive = true },
        new Doctor { DoctorId = 9, SpecialtyId = endocrinologia.SpecialtyId, LicenseNumber = "LIC-0009", PhoneNumber = "809-505-6060", YearsOfExperience = 14, Education = "Pontificia Universidad Católica Madre y Maestra", Bio = "Endocrinólogo, actualmente de licencia.", ConsultationFee = 1900m, ClinicAddress = "Av. Núñez de Cáceres, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2025, 12, 25), CreatedAt = DateTime.Now, IsActive = false },
        new Doctor { DoctorId = 10, SpecialtyId = otorrino.SpecialtyId, LicenseNumber = "LIC-0010", PhoneNumber = "809-707-8080", YearsOfExperience = 5, Education = "Universidad Tecnológica de Santiago", Bio = "Otorrinolaringóloga.", ConsultationFee = 1400m, ClinicAddress = "Av. Bolívar, Santo Domingo", AvailabilityModeId = presencial.AvailabilityModeId, LicenseExpirationDate = new DateOnly(2028, 1, 15), CreatedAt = DateTime.Now, IsActive = true },
    };
    context.Doctors.AddRange(doctors);
    context.SaveChanges();

    // ── Disponibilidad de doctores (próximos 14 días, lun-vie, bloques AM/PM) ──
    var availabilityId = 1;
    var doctorAvailability = new List<DoctorAvailability>();
    var slotHours = new[] { 9, 10, 11, 14, 15, 16 };

    foreach (var doctor in doctors)
    {
        for (int dayOffset = 1; dayOffset <= 14; dayOffset++)
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(dayOffset));
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            foreach (var hour in slotHours)
            {
                doctorAvailability.Add(new DoctorAvailability
                {
                    AvailabilityId = availabilityId++,
                    DoctorId = doctor.DoctorId,
                    AvailableDate = date,
                    StartTime = new TimeOnly(hour, 0),
                    EndTime = new TimeOnly(hour + 1, 0),
                    AvailabilityModeId = doctor.AvailabilityModeId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
        }
    }
    context.DoctorAvailabilities.AddRange(doctorAvailability);
    context.SaveChanges();

    // ── Pacientes ──────────────────────────────────────────────────────────
    var patients = new List<Patient>
    {
        new Patient { PatientId = 11, Gender = "Masculino", PhoneNumber = "809-111-2222", Address = "Calle Primera #1, Santo Domingo", EmergencyContactName = "Laura Martínez", EmergencyContactPhone = "809-333-4444", BloodType = "O+", Allergies = "Ninguna", InsuranceProviderId = senasa.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 12, Gender = "Femenino", PhoneNumber = "809-555-6666", Address = "Calle Segunda #2, Santiago", EmergencyContactName = "Pedro López", EmergencyContactPhone = "809-777-8888", BloodType = "A+", Allergies = "Penicilina", InsuranceProviderId = humano.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 13, Gender = "Masculino", PhoneNumber = "809-121-2121", Address = "Calle Tercera #3, Santo Domingo", EmergencyContactName = "Ana Ramírez", EmergencyContactPhone = "809-131-3131", BloodType = "B+", Allergies = "Ninguna", InsuranceProviderId = mapfre.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 14, Gender = "Femenino", PhoneNumber = "809-141-4141", Address = "Calle Cuarta #4, San Cristóbal", EmergencyContactName = "Luis Sánchez", EmergencyContactPhone = "809-151-5151", BloodType = "AB+", Allergies = "Mariscos", InsuranceProviderId = reservas.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 15, Gender = "Masculino", PhoneNumber = "809-161-6161", Address = "Calle Quinta #5, La Vega", EmergencyContactName = "Sofía Peña", EmergencyContactPhone = "809-171-7171", BloodType = "O-", Allergies = "Ninguna", InsuranceProviderId = palic.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 16, Gender = "Femenino", PhoneNumber = "809-181-8181", Address = "Calle Sexta #6, Santo Domingo Este", EmergencyContactName = "Marcos Castillo", EmergencyContactPhone = "809-191-9191", BloodType = "A-", Allergies = "Polen", InsuranceProviderId = universal.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
        new Patient { PatientId = 17, Gender = "Masculino", PhoneNumber = "809-202-1212", Address = "Calle Séptima #7, Santiago", EmergencyContactName = "Elena Medina", EmergencyContactPhone = "809-212-1313", BloodType = "B-", Allergies = "Ninguna", InsuranceProviderId = yunen.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = false },
        new Patient { PatientId = 18, Gender = "Femenino", PhoneNumber = "809-222-1414", Address = "Calle Octava #8, Santo Domingo", EmergencyContactName = "Julio Rosario", EmergencyContactPhone = "809-232-1515", BloodType = "AB-", Allergies = "Látex", InsuranceProviderId = senasa.InsuranceProviderId, CreatedAt = DateTime.Now, IsActive = true },
    };
    context.Patients.AddRange(patients);
    context.SaveChanges();

    // ── Historial médico ─────────────────────────────────────────────────
    var records = new List<MedicalRecord>
    {
        new MedicalRecord { PatientId = 11, DoctorId = 1, Diagnosis = "Hipertensión arterial leve", Treatment = "Losartan 50mg una vez al día, control en 30 días", Notes = "Paciente refiere dolores de cabeza ocasionales. Se recomienda reducir consumo de sal.", DateOfVisit = DateTime.Now.AddMonths(-2), CreatedAt = DateTime.Now.AddMonths(-2) },
        new MedicalRecord { PatientId = 11, DoctorId = 1, Diagnosis = "Control de seguimiento - hipertensión estable", Treatment = "Continuar Losartan 50mg, dieta baja en sodio", Notes = "Presión arterial dentro de rango normal en esta visita. Buena adherencia al tratamiento.", DateOfVisit = DateTime.Now.AddDays(-15), CreatedAt = DateTime.Now.AddDays(-15) },
        new MedicalRecord { PatientId = 13, DoctorId = 2, Diagnosis = "Control pediátrico de rutina", Treatment = "Vacunación al día, vitaminas", Notes = "Desarrollo acorde a la edad. Próximo control en 6 meses.", DateOfVisit = DateTime.Now.AddDays(-10), CreatedAt = DateTime.Now.AddDays(-10) },
        new MedicalRecord { PatientId = 12, DoctorId = 4, Diagnosis = "Chequeo ginecológico anual", Treatment = "Ninguno, resultados normales", Notes = "Se recomienda repetir citología en un año.", DateOfVisit = DateTime.Now.AddDays(-20), CreatedAt = DateTime.Now.AddDays(-20) },
        new MedicalRecord { PatientId = 18, DoctorId = 8, Diagnosis = "Ansiedad leve", Treatment = "Terapia cognitivo-conductual, seguimiento quincenal", Notes = "Paciente muestra mejoría progresiva.", DateOfVisit = DateTime.Now.AddDays(-7), CreatedAt = DateTime.Now.AddDays(-7) },
        new MedicalRecord { PatientId = 15, DoctorId = 10, Diagnosis = "Sinusitis aguda", Treatment = "Antibiótico por 7 días, lavados nasales", Notes = "Síntomas en remisión.", DateOfVisit = DateTime.Now.AddDays(-15), CreatedAt = DateTime.Now.AddDays(-15) },
        new MedicalRecord { PatientId = 18, DoctorId = 3, Diagnosis = "Dermatitis de contacto", Treatment = "Crema corticosteroide tópica", Notes = "Se identificó el látex como posible causante.", DateOfVisit = DateTime.Now.AddDays(-30), CreatedAt = DateTime.Now.AddDays(-30) },
    };
    context.MedicalRecords.AddRange(records);
    context.SaveChanges();

    // ── Citas (doctorId, patientId, offsetDías, statusId) ───────────────────
    var appointmentData = new (int DoctorId, int PatientId, int OffsetDays, int StatusId)[]
    {
        (1, 11, 3, pendiente.StatusId),
        (1, 12, 10, confirmada.StatusId),
        (1, 13, -5, completada.StatusId),
        (1, 14, -1, cancelada.StatusId),
        (2, 12, 5, confirmada.StatusId),
        (2, 15, 2, pendiente.StatusId),
        (2, 11, -10, completada.StatusId),
        (3, 16, 7, pendiente.StatusId),
        (3, 13, 1, confirmada.StatusId),
        (3, 17, -3, rechazada.StatusId),
        (4, 14, 4, pendiente.StatusId),
        (4, 18, 6, confirmada.StatusId),
        (4, 12, -20, completada.StatusId),
        (6, 15, 8, pendiente.StatusId),
        (6, 16, -2, cancelada.StatusId),
        (7, 17, 3, confirmada.StatusId),
        (7, 11, 12, pendiente.StatusId),
        (8, 18, -7, completada.StatusId),
        (8, 13, 9, pendiente.StatusId),
        (10, 14, 2, confirmada.StatusId),
        (10, 15, -15, completada.StatusId),
        (1, 16, 15, pendiente.StatusId),
        (2, 17, 20, confirmada.StatusId),
        (3, 18, -30, completada.StatusId),
    };

    var appointments = appointmentData.Select((a, index) => new Appointment
    {
        AppointmentId = index + 1,
        DoctorId = a.DoctorId,
        PatientId = a.PatientId,
        AppointmentDate = DateTime.Now.AddDays(a.OffsetDays),
        StatusId = a.StatusId,
        CreatedAt = DateTime.Now.AddDays(Math.Min(a.OffsetDays, 0) - 1)
    }).ToList();

    context.Appointments.AddRange(appointments);
    context.SaveChanges();
}