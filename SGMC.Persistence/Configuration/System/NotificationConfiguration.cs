using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGMC.Domain.Entities.System;

namespace SGMC.Persistence.Configuration.System
{
    public partial class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> entity)
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E327D8F016B");

            entity.ToTable("Notifications", "system");

            entity.Property(e => e.NotificationId).HasColumnName("NotificationID");
          //  entity.Property(e => e.Message)
           //     .IsRequired()
           //     .HasColumnType("text");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__619B8048")
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar el usuario se eliminan sus notificaciones

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Notification> entity);
    }
}