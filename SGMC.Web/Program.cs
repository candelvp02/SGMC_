using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Entities.Users;
using SGMC.Infrastructure.Dependencies;
using SGMC.Persistence.Context;
using SGMC.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext InMemory
builder.Services.AddDbContext<HealtSyncContext>(options =>
    options.UseInMemoryDatabase("HealtSyncDb"));

// Dependencies
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

// Consumo de capa api
builder.Services.AddHttpClient<IAppointmentApiClient, AppointmentApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7099/api/");
});

builder.Services.AddHttpClient<IPatientApiClient, PatientApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7099/api/");
});

builder.Services.AddHttpClient<IDoctorApiClient, DoctorApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7099/api/");
});



// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Seed de datos en memoria
using (var scope = app.Services.CreateScope())
{
var context = scope.ServiceProvider.GetRequiredService<HealtSyncContext>();
SeedData(context);
}

app.Run();

// ???????????????????????????????????????????????????????????????????????????
// SEED DATA
// ???????????????????????????????????????????????????????????????????????????
static void SeedData(HealtSyncContext context)
{
    // ?? Roles ?????????????????????????????????????????????????????????????
    var rolePaciente = new Role { RoleId = 1, RoleName = "Paciente", CreatedAt = DateTime.Now, IsActive = true };
    var roleDoctor = new Role { RoleId = 2, RoleName = "Doctor", CreatedAt = DateTime.Now, IsActive = true };
    var roleAdmin = new Role { RoleId = 3, RoleName = "Administrador", CreatedAt = DateTime.Now, IsActive = true };
    context.Roles.AddRange(rolePaciente, roleDoctor, roleAdmin);

    // ?? Modos de disponibilidad ????????????????????????????????????????????
    var presencial = new AvailabilityMode { AvailabilityModeId = 1, AvailabilityMode1 = "Presencial", CreatedAt = DateTime.Now, IsActive = true };
    var virtualMode = new AvailabilityMode { AvailabilityModeId = 2, AvailabilityMode1 = "Virtual", CreatedAt = DateTime.Now, IsActive = true };
    context.AvailabilityModes.AddRange(presencial, virtualMode);

    // ?? Especialidades ?????????????????????????????????????????????????????
    var cardiologia = new Specialty { SpecialtyId = 1, SpecialtyName = "Cardiología", CreatedAt = DateTime.Now, IsActive = true };
    var pediatria = new Specialty { SpecialtyId = 2, SpecialtyName = "Pediatría", CreatedAt = DateTime.Now, IsActive = true };
    var dermatologia = new Specialty { SpecialtyId = 3, SpecialtyName = "Dermatología", CreatedAt = DateTime.Now, IsActive = true };
    var ginecologia = new Specialty { SpecialtyId = 4, SpecialtyName = "Ginecología", CreatedAt = DateTime.Now, IsActive = true };
    var neurologia = new Specialty { SpecialtyId = 5, SpecialtyName = "Neurología", CreatedAt = DateTime.Now, IsActive = false };
    context.Specialties.AddRange(cardiologia, pediatria, dermatologia, ginecologia, neurologia);

    // ?? Tipos de red ???????????????????????????????????????????????????????
    var hmo = new NetworkType { NetworkTypeId = 1, Name = "HMO", Description = "Health Maintenance Organization", CreatedAt = DateTime.Now, IsActive = true };
    var ppo = new NetworkType { NetworkTypeId = 2, Name = "PPO", Description = "Preferred Provider Organization", CreatedAt = DateTime.Now, IsActive = true };
    context.NetworkTypes.AddRange(hmo, ppo);

    // ?? Statuses ?????????????????????????????????????????????????????????
    var pendiente = new Status { StatusId = 1, StatusName = "Pendiente" };
    var confirmada = new Status { StatusId = 2, StatusName = "Confirmada" };
    var cancelada = new Status { StatusId = 3, StatusName = "Cancelada" };
    var completada = new Status { StatusId = 4, StatusName = "Completada" };
    var rechazada = new Status { StatusId = 5, StatusName = "Rechazada" };
    context.Statuses.AddRange(pendiente, confirmada, cancelada, completada, rechazada);

    context.SaveChanges();

    // ?? Proveedores de seguro ??????????????????????????????????????????????
    var senasa = new InsuranceProvider
    {
        InsuranceProviderId = 1,
        Name = "SENASA",
        PhoneNumber = "809-200-8080",
        Email = "info@senasa.gob.do",
        Website = "https://www.senasa.gob.do",
        Address = "Av. Tiradentes #30",
        City = "Santo Domingo",
        State = "Distrito Nacional",
        Country = "República Dominicana",
        ZipCode = "10101",
        LogoUrl = "",
        IsPreferred = true,
        NetworkTypeId = hmo.NetworkTypeId,
        CustomerSupportContact = "809-200-8080",
        AcceptedRegions = "Nacional",
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    var humano = new InsuranceProvider
    {
        InsuranceProviderId = 2,
        Name = "Humano",
        PhoneNumber = "809-535-6262",
        Email = "info@humano.com.do",
        Website = "https://www.humano.com.do",
        Address = "Av. Abraham Lincoln #1008",
        City = "Santo Domingo",
        State = "Distrito Nacional",
        Country = "República Dominicana",
        ZipCode = "10101",
        LogoUrl = "",
        IsPreferred = true,
        NetworkTypeId = ppo.NetworkTypeId,
        CustomerSupportContact = "809-535-6262",
        AcceptedRegions = "Nacional",
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    var mapfre = new InsuranceProvider
    {
        InsuranceProviderId = 3,
        Name = "MAPFRE Salud",
        PhoneNumber = "809-476-8181",
        Email = "info@mapfre.com.do",
        Website = "https://www.mapfre.com.do",
        Address = "Av. Winston Churchill #1099",
        City = "Santo Domingo",
        State = "Distrito Nacional",
        Country = "República Dominicana",
        ZipCode = "10101",
        LogoUrl = "",
        IsPreferred = false,
        NetworkTypeId = ppo.NetworkTypeId,
        CustomerSupportContact = "809-476-8181",
        AcceptedRegions = "Nacional",
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    var reservas = new InsuranceProvider
    {
        InsuranceProviderId = 4,
        Name = "Reservas Salud",
        PhoneNumber = "809-960-1212",
        Email = "info@banreservas.com.do",
        Website = "https://www.banreservas.com.do",
        Address = "Av. Isabel Aguiar",
        City = "Santo Domingo",
        State = "Distrito Nacional",
        Country = "República Dominicana",
        ZipCode = "10101",
        LogoUrl = "",
        IsPreferred = false,
        NetworkTypeId = hmo.NetworkTypeId,
        CustomerSupportContact = "809-960-1212",
        AcceptedRegions = "Nacional",
        CreatedAt = DateTime.Now,
        IsActive = false
    };
    context.InsuranceProviders.AddRange(senasa, humano, mapfre, reservas);

    // ?? Usuarios ???????????????????????????????????????????????????????????
    var userDoctor1 = new User { UserId = 1, Email = "doctor1@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    var userDoctor2 = new User { UserId = 2, Email = "doctor2@sgmc.com", PasswordHash = "1234", RoleId = roleDoctor.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    var userPaciente1 = new User { UserId = 3, Email = "paciente1@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    var userPaciente2 = new User { UserId = 4, Email = "paciente2@sgmc.com", PasswordHash = "1234", RoleId = rolePaciente.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    var userAdmin = new User { UserId = 5, Email = "admin@sgmc.com", PasswordHash = "1234", RoleId = roleAdmin.RoleId, CreatedAt = DateTime.Now, IsActive = true };
    context.Users.AddRange(userDoctor1, userDoctor2, userPaciente1, userPaciente2, userAdmin);
    context.SaveChanges();

    // ?? Personas ???????????????????????????????????????????????????????????
    var person1 = new Person { PersonId = 1, FirstName = "Juan", LastName = "Pérez", DateOfBirth = new DateOnly(1980, 5, 10), IdentificationNumber = "001-1234567-1", Gender = "Masculino", UserId = userDoctor1.UserId };
    var person2 = new Person { PersonId = 2, FirstName = "Ana", LastName = "Gómez", DateOfBirth = new DateOnly(1985, 3, 22), IdentificationNumber = "001-1234567-2", Gender = "Femenino", UserId = userDoctor2.UserId };
    var person3 = new Person { PersonId = 3, FirstName = "Carlos", LastName = "Martínez", DateOfBirth = new DateOnly(1990, 7, 15), IdentificationNumber = "001-1234567-3", Gender = "Masculino", UserId = userPaciente1.UserId };
    var person4 = new Person { PersonId = 4, FirstName = "María", LastName = "López", DateOfBirth = new DateOnly(1995, 1, 30), IdentificationNumber = "001-1234567-4", Gender = "Femenino", UserId = userPaciente2.UserId };
    var person5 = new Person { PersonId = 5, FirstName = "Admin", LastName = "SGMC", DateOfBirth = new DateOnly(1975, 6, 1), IdentificationNumber = "001-1234567-5", Gender = "Masculino", UserId = userAdmin.UserId };
    context.Persons.AddRange(person1, person2, person3, person4, person5);
    context.SaveChanges();

    // ?? Doctores ???????????????????????????????????????????????????????????
    var doctor1 = new Doctor
    {
        DoctorId = person1.PersonId,
        SpecialtyId = cardiologia.SpecialtyId,
        LicenseNumber = "LIC-0001",
        PhoneNumber = "809-123-4567",
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
        PhoneNumber = "809-765-4321",
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

    // ?? Pacientes ??????????????????????????????????????????????????????????
    var paciente1 = new Patient
    {
        PatientId = person3.PersonId,
        Gender = "Masculino",
        PhoneNumber = "809-111-2222",
        Address = "Calle Primera #1, Santo Domingo",
        EmergencyContactName = "Laura Martínez",
        EmergencyContactPhone = "809-333-4444",
        BloodType = "O+",
        Allergies = "Ninguna",
        InsuranceProviderId = senasa.InsuranceProviderId,
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    var paciente2 = new Patient
    {
        PatientId = person4.PersonId,
        Gender = "Femenino",
        PhoneNumber = "809-555-6666",
        Address = "Calle Segunda #2, Santiago",
        EmergencyContactName = "Pedro López",
        EmergencyContactPhone = "809-777-8888",
        BloodType = "A+",
        Allergies = "Penicilina",
        InsuranceProviderId = humano.InsuranceProviderId,
        CreatedAt = DateTime.Now,
        IsActive = true
    };
    context.Patients.AddRange(paciente1, paciente2);
    context.SaveChanges();

    // ?? Historial médico (traído de dev) ????????????????????????????????????
    var record1 = new MedicalRecord
    {
        PatientId = paciente1.PatientId,
        DoctorId = doctor1.DoctorId,
        Diagnosis = "Hipertensión arterial leve",
        Treatment = "Losartan 50mg una vez al día, control en 30 días",
        Notes = "Paciente refiere dolores de cabeza ocasionales. Se recomienda reducir consumo de sal.",
        DateOfVisit = DateTime.Now.AddMonths(-2),
        CreatedAt = DateTime.Now.AddMonths(-2)
    };
    var record2 = new MedicalRecord
    {
        PatientId = paciente1.PatientId,
        DoctorId = doctor1.DoctorId,
        Diagnosis = "Control de seguimiento - hipertensión estable",
        Treatment = "Continuar Losartan 50mg, dieta baja en sodio",
        Notes = "Presión arterial dentro de rango normal en esta visita. Buena adherencia al tratamiento.",
        DateOfVisit = DateTime.Now.AddDays(-15),
        CreatedAt = DateTime.Now.AddDays(-15)
    };
    context.MedicalRecords.AddRange(record1, record2);
    context.SaveChanges();

    // ?? Citas ??????????????????????????????????????????????????????????????
    context.Appointments.AddRange(
        new Appointment { AppointmentId = 1, PatientId = paciente1.PatientId, DoctorId = doctor1.DoctorId, AppointmentDate = DateTime.Now.AddDays(3), StatusId = pendiente.StatusId, CreatedAt = DateTime.Now },
        new Appointment { AppointmentId = 2, PatientId = paciente2.PatientId, DoctorId = doctor2.DoctorId, AppointmentDate = DateTime.Now.AddDays(5), StatusId = confirmada.StatusId, CreatedAt = DateTime.Now },
        new Appointment { AppointmentId = 3, PatientId = paciente1.PatientId, DoctorId = doctor2.DoctorId, AppointmentDate = DateTime.Now.AddDays(-2), StatusId = completada.StatusId, CreatedAt = DateTime.Now.AddDays(-5) },
        new Appointment { AppointmentId = 4, PatientId = paciente2.PatientId, DoctorId = doctor1.DoctorId, AppointmentDate = DateTime.Now.AddDays(7), StatusId = pendiente.StatusId, CreatedAt = DateTime.Now }
    );
    context.SaveChanges();
}

    app.Run();