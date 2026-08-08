using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGMC.Domain.Entities.System;

namespace SGMC.Persistence.Configuration.System
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> entity)
        {
            entity.HasKey(r => r.ReminderId);

            entity.Property(r => r.Message).IsRequired().HasMaxLength(1000);
            entity.Property(r => r.Status).IsRequired().HasMaxLength(20);
            entity.Property(r => r.PatientName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.PatientEmail).IsRequired().HasMaxLength(150);

            entity.HasOne(r => r.Appointment)
                .WithMany()
                .HasForeignKey(r => r.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade); // si se borra la cita, se borran sus recordatorios
        }
    }
}