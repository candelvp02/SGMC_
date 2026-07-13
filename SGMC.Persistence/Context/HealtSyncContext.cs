using Microsoft.EntityFrameworkCore;
using SGMC.Domain.Entities.Appointments;
using SGMC.Domain.Entities.Insurance;
using SGMC.Domain.Entities.Medical;
using SGMC.Domain.Entities.System;
using SGMC.Domain.Entities.Users;
using SGMC.Persistence.Configuration.Appointments;
using SGMC.Persistence.Configuration.Insurance;
using SGMC.Persistence.Configuration.Medical;
using SGMC.Persistence.Configuration.System;
using SGMC.Persistence.Configuration.Users;

namespace SGMC.Persistence.Context
{
    public partial class HealtSyncContext : DbContext
    {
        public HealtSyncContext(DbContextOptions<HealtSyncContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Appointment> Appointments { get; set; }

        public virtual DbSet<AvailabilityMode> AvailabilityModes { get; set; }

        public virtual DbSet<Doctor> Doctors { get; set; }

        public virtual DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

        public virtual DbSet<Employee> Employees { get; set; }

        public virtual DbSet<InsuranceProvider> InsuranceProviders { get; set; }

        public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

        public virtual DbSet<NetworkType> NetworkTypes { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }

        public virtual DbSet<Patient> Patients { get; set; }

        public virtual DbSet<Person> Persons { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<Specialty> Specialties { get; set; }

        public virtual DbSet<Status> Statuses { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
            modelBuilder.ApplyConfiguration(new DoctorAvailabilityConfiguration());
            modelBuilder.ApplyConfiguration(new InsuranceProviderConfiguration());
            modelBuilder.ApplyConfiguration(new NetworkTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AvailabilityModeConfiguration());
            modelBuilder.ApplyConfiguration(new MedicalRecordConfiguration());
            modelBuilder.ApplyConfiguration(new SpecialtyConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new StatusConfiguration());
            modelBuilder.ApplyConfiguration(new DoctorConfiguration());
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            modelBuilder.ApplyConfiguration(new PatientConfiguration());
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    }
}
