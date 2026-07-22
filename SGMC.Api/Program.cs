using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SGMC.Application.Interfaces.Service;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Entities.Users;
using SGMC.Infrastructure.Dependencies;
using SGMC.Infrastructure.Services;
using SGMC.Persistence.Context;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//DbContext - InMemory para pruebas (SQL Server desactivado temporalmente)
builder.Services.AddDbContext<HealtSyncContext>(options =>
    options.UseInMemoryDatabase("HealtSyncDb"));

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

//http configuration
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

    var person1 = new Person { PersonId = 1, FirstName = "Juan", LastName = "Pérez", DateOfBirth = new DateOnly(1980, 5, 10), IdentificationNumber = "00112223334", Gender = "Masculino", UserId = user1.UserId };
    var person2 = new Person { PersonId = 2, FirstName = "Ana", LastName = "Gómez", DateOfBirth = new DateOnly(1985, 3, 22), IdentificationNumber = "00112223335", Gender = "Femenino", UserId = user2.UserId };
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

    // Paciente de prueba
    var patientUser = new User { UserId = 3, Email = "paciente1@sgmc.com", PasswordHash = "1234", RoleId = doctorRole.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    context.Users.Add(patientUser);
    context.SaveChanges();

    var patientPerson = new Person { PersonId = 3, FirstName = "Carlos", LastName = "Ramirez", DateOfBirth = new DateOnly(1990, 7, 15), IdentificationNumber = "00112223336", Gender = "Masculino", UserId = patientUser.UserId };
    context.Persons.Add(patientPerson);
    context.SaveChanges();

    var patient1 = new Patient
    {
        PatientId = patientPerson.PersonId,
        Gender = "Masculino",
        PhoneNumber = "8095551234",
        Address = "Calle Duarte #45, Santo Domingo",
        EmergencyContactName = "Maria Ramirez",
        EmergencyContactPhone = "8095555678",
        BloodType = "O+",
        Allergies = "Ninguna conocida",
        InsuranceProviderId = 1,
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    context.Patients.Add(patient1);
    context.SaveChanges();

    // Historial médico de prueba
    var record1 = new MedicalRecord
    {
        PatientId = patient1.PatientId,
        DoctorId = doctor1.DoctorId,
        Diagnosis = "Hipertension arterial leve",
        Treatment = "Losartan 50mg una vez al dia, control en 30 dias",
        Notes = "Paciente refiere dolores de cabeza ocasionales. Se recomienda reducir consumo de sal.",
        DateOfVisit = DateTime.Now.AddMonths(-2),
        CreatedAt = DateTime.Now.AddMonths(-2)
    };

    var record2 = new MedicalRecord
    {
        PatientId = patient1.PatientId,
        DoctorId = doctor1.DoctorId,
        Diagnosis = "Control de seguimiento - hipertension estable",
        Treatment = "Continuar Losartan 50mg, dieta baja en sodio",
        Notes = "Presion arterial dentro de rango normal en esta visita. Buena adherencia al tratamiento.",
        DateOfVisit = DateTime.Now.AddDays(-15),
        CreatedAt = DateTime.Now.AddDays(-15)
    };

    // Statuses de citas (Pendiente, Confirmada, Cancelada, Completada)
    var statusPendiente = new Status { StatusId = 1, StatusName = "Pendiente" };
    var statusConfirmada = new Status { StatusId = 2, StatusName = "Confirmada" };
    var statusCancelada = new Status { StatusId = 3, StatusName = "Cancelada" };
    var statusCompletada = new Status { StatusId = 4, StatusName = "Completada" };
    context.Statuses.AddRange(statusPendiente, statusConfirmada, statusCancelada, statusCompletada);
    context.SaveChanges();

    // Citas de prueba para la agenda del doctor 1
    var appointment1 = new Appointment
    {
        PatientId = patient1.PatientId,
        DoctorId = doctor1.DoctorId,
        AppointmentDate = DateTime.Now.AddDays(2).Date.AddHours(9),
        StatusId = statusPendiente.StatusId,
        CreatedAt = DateTime.Now
    };

    var appointment2 = new Appointment
    {
        PatientId = patient1.PatientId,
        DoctorId = doctor1.DoctorId,
        AppointmentDate = DateTime.Now.AddDays(5).Date.AddHours(11),
        StatusId = statusConfirmada.StatusId,
        CreatedAt = DateTime.Now
    };

    var appointment3 = new Appointment
    {
        PatientId = patient1.PatientId,
        DoctorId = doctor1.DoctorId,
        AppointmentDate = DateTime.Now.AddDays(-10).Date.AddHours(10),
        StatusId = statusCompletada.StatusId,
        CreatedAt = DateTime.Now.AddDays(-10)
    };

    context.Appointments.AddRange(appointment1, appointment2, appointment3);
    context.SaveChanges();

    context.MedicalRecords.AddRange(record1, record2);
    context.SaveChanges();

    context.Doctors.AddRange(doctor1, doctor2);
    context.SaveChanges();
}