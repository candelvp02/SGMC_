using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SGMC.Infrastructure.Dependencies;
using SGMC.Persistence.Context;
using SGMC.Persistence.Seed;
using SGMC.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext — alterna entre SQL Server e InMemory según "UseInMemoryDatabase" en appsettings.
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<HealtSyncContext>(options =>
{
    if (useInMemoryDatabase)
        options.UseInMemoryDatabase("SGMC_InMemoryDb");
    else
        options.UseSqlServer(builder.Configuration.GetConnectionString("HealtSyncConnection"));
});

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
builder.Services.AddAuthDependencies();

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

// Autenticación por cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Sembrar datos de catálogo si se está corriendo en memoria (la base InMemory arranca vacía)
if (useInMemoryDatabase)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HealtSyncContext>();
    InMemorySeeder.Seed(context);
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();