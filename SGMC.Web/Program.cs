using Microsoft.EntityFrameworkCore;
using SGMC.Infrastructure.Dependencies;
using SGMC.Persistence.Context;
using SGMC.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<HealtSyncContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HealtSyncConnection")));

// Dependencies
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

// Consumo de capa api
builder.Services.AddHttpClient<IAppointmentApiClient, AppointmentApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7038/api/");
});

builder.Services.AddHttpClient<IPatientApiClient, PatientApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7038/api/");
});

builder.Services.AddHttpClient<IDoctorApiClient, DoctorApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7038/api/");
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

app.Run();