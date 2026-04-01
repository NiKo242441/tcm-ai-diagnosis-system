using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class TreatmentMap : IEntityTypeConfiguration<Treatment>
    {
        public void Configure(EntityTypeBuilder<Treatment> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TcmDiagnosis);
            builder.Property(t => t.SyndromeAnalysis);
            builder.Property(t => t.TreatmentPrinciple);
            builder.Property(t => t.ExpectedOutcome);
            builder.Property(t => t.Precautions);
            builder.Property(t => t.Version);

            builder.Property(t => t.Status)
                .HasDefaultValue(TreatmentStatus.Generating);

            builder.HasIndex(t => new { t.PatientId, t.VisitId, t.Version }).IsUnique();

            builder.HasOne(t => t.Patient)
                .WithMany()
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Visit)
                .WithMany()
                .HasForeignKey(t => t.VisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Syndrome)
                .WithMany()
                .HasForeignKey(t => t.SyndromeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}