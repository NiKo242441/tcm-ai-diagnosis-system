using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class MoxibustionMap : IEntityTypeConfiguration<Moxibustion>
    {
        public void Configure(EntityTypeBuilder<Moxibustion> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.PointName).IsRequired();
            builder.Property(m => m.Location);
            builder.Property(m => m.Method);
            builder.Property(m => m.MoxaType);
            builder.Property(m => m.Technique);
            builder.Property(m => m.TemperatureControl);
            builder.Property(m => m.Duration);
            builder.Property(m => m.Frequency);
            builder.Property(m => m.CourseDuration);
            builder.Property(m => m.Efficacy);
            builder.Property(m => m.Indications);
            builder.Property(m => m.TechniquePoints);
            builder.Property(m => m.Precautions);
            builder.Property(m => m.Contraindications);
            builder.Property(m => m.PostTreatmentCare);
            builder.Property(m => m.CombinationTherapy);
            builder.Property(m => m.PatientFriendlyName);
            builder.Property(m => m.PatientFriendlyDescription);

            builder.HasOne(m => m.Treatment)
                .WithMany(t => t.Moxibustions)
                .HasForeignKey(m => m.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}