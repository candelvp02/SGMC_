using Microsoft.EntityFrameworkCore;
using SGMC.Application.Interfaces.Service;
using SGMC.Infrastructure.Dependencies;
using SGMC.Infrastructure.Services;
using SGMC.Persistence.Context;
using SGMC.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext — alterna entre SQL Server e InMemory según "UseInMemoryDatabase" en appsettings.
// Útil para correr/demostrar el proyecto (o partes de él) sin depender de una instancia real de SQL Server.
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<HealtSyncContext>(options =>
{
    if (useInMemoryDatabase)
        options.UseInMemoryDatabase("SGMC_InMemoryDb");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("HealtSyncConnection"));
});

// capa de application and persistence
builder.Services.AddUserDependencies();
builder.Services.AddDoctorDependencies();
builder.Services.AddPatientDependencies();
builder.Services.AddAppointmentDependencies();
builder.Services.AddAvailabilityDependencies();
builder.Services.AddInsuranceProviderDependencies();
builder.Services.AddMedicalRecordDependencies();
builder.Services.AddNotificationDependencies();
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



//controllers & swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Sembrar datos de catálogo si se está corriendo en memoria (la base InMemory arranca vacía)
if (useInMemoryDatabase)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HealtSyncContext>();
    InMemorySeeder.Seed(context);
}

//http configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed de datos en memoria (solo para pruebas)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HealtSyncContext>();
    SeedData(context);
}

app.Run();


static void SeedData(HealtSyncContext context)
{
    var doctorRole = new Role { RoleId = 1, RoleName = "Doctor", CreatedAt = DateTime.Now, IsActive = true };
    context.Roles.Add(doctorRole);

    var presencial = new AvailabilityMode { AvailabilityModeId = 1, AvailabilityMode1 = "Presencial", CreatedAt = DateTime.Now, IsActive = true };
    var virtualMode = new AvailabilityMode { AvailabilityModeId = 2, AvailabilityMode1 = "Virtual", CreatedAt = DateTime.Now, IsActive = true };
    context.AvailabilityModes.AddRange(presencial, virtualMode);

    var cardiologia = new Specialty { SpecialtyId = 1, SpecialtyName = "Cardiología", CreatedAt = DateTime.Now, IsActive = true };
    var pediatria = new Specialty { SpecialtyId = 2, SpecialtyName = "Pediatría", CreatedAt = DateTime.Now, IsActive = true };
    context.Specialties.AddRange(cardiologia, pediatria);

    context.SaveChanges();

    var user1 = new User { UserId = 1, Email = "doctor1@sgmc.com", PasswordHash = "1234", RoleId = doctorRole.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    var user2 = new User { UserId = 2, Email = "doctor2@sgmc.com", PasswordHash = "1234", RoleId = doctorRole.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    context.Users.AddRange(user1, user2);
    context.SaveChanges();

    var person1 = new Person { PersonId = 1, FirstName = "Juan", LastName = "Pérez", DateOfBirth = new DateOnly(1980, 5, 10), IdentificationNumber = "00112223334", Gender = "M", UserId = user1.UserId };
    var person2 = new Person { PersonId = 2, FirstName = "Ana", LastName = "Gómez", DateOfBirth = new DateOnly(1985, 3, 22), IdentificationNumber = "00112223335", Gender = "F", UserId = user2.UserId };
    context.Persons.AddRange(person1, person2);
    context.SaveChanges();

    var doctor1 = new Doctor
    {
        DoctorId = person1.PersonId,
        SpecialtyId = cardiologia.SpecialtyId,
        LicenseNumber = "LIC-0001",
        PhoneNumber = "8091234567",
        YearsOfExperience = 10,
        Education = "Universidad Autónoma de Santo Domingo",
        Bio = "Especialista en cardiología con 10 años de experiencia.",
        ConsultationFee = 1500m,
        ClinicAddress = "Av. Winston Churchill, Santo Domingo",
        AvailabilityModeId = presencial.AvailabilityModeId,
        LicenseExpirationDate = new DateOnly(2027, 12, 31),
        CreatedAt = DateTime.Now,
        IsActive = true
    };

    var doctor2 = new Doctor
    {
        DoctorId = person2.PersonId,
        SpecialtyId = pediatria.SpecialtyId,
        LicenseNumber = "LIC-0002",
        PhoneNumber = "8097654321",
        YearsOfExperience = 6,
        Education = "Pontificia Universidad Católica Madre y Maestra",
        Bio = "Pediatra con enfoque en atención infantil integral.",
        ConsultationFee = 1200m,
        ClinicAddress = "Av. 27 de Febrero, Santo Domingo",
        AvailabilityModeId = virtualMode.AvailabilityModeId,
        LicenseExpirationDate = new DateOnly(2026, 10, 15),
        CreatedAt = DateTime.Now,
        IsActive = true
    };

    context.Doctors.AddRange(doctor1, doctor2);
    context.SaveChanges();
}