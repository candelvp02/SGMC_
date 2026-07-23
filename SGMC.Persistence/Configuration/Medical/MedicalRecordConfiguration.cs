using SGMC.Domain.Entities.Medical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SGMC.Persistence.Configuration.Medical
{
    public partial class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> entity)
        {
            entity.HasKey(e => e.RecordId).HasName("PK__MedicalR__FBDF78C96E650FFC");

            entity.ToTable("MedicalRecords", "medical");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.RecordId).HasColumnName("RecordID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasAnnotation("Relational:DefaultConstraintName", "DF__MedicalRe__Creat__5CD6CB2B")
                .HasColumnType("datetime");
            entity.Property(e => e.DateOfVisit).HasColumnType("datetime");
            entity.Property(e => e.Diagnosis)
                .IsRequired()
                .HasColumnType("text");
            entity.Property(e => e.DoctorId).HasColumnName("DoctorID");
            entity.Property(e => e.PatientId).HasColumnName("PatientID");
            entity.Property(e => e.Treatment)
                .IsRequired()
                .HasColumnType("text");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasAnnotation("Relational:DefaultConstraintName", "DF__MedicalRe__Updat__5DCAEF64")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalRecords_Patient");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<MedicalRecord> entity);
    }
}
