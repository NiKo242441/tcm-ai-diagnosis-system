using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class CuppingMap : IEntityTypeConfiguration<Cupping>
    {
        public void Configure(EntityTypeBuilder<Cupping> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Area).IsRequired();
            builder.Property(c => c.SpecificPoints);
            builder.Property(c => c.SuitableFor);
            builder.Property(c => c.Method);
            builder.Property(c => c.CupType);
            builder.Property(c => c.CupSize);
            builder.Property(c => c.SuctionStrength);
            builder.Property(c => c.Duration);
            builder.Property(c => c.Frequency);
            builder.Property(c => c.Efficacy);
            builder.Property(c => c.Indications);
            builder.Property(c => c.TechniquePoints);
            builder.Property(c => c.Precautions);
            builder.Property(c => c.PatientFriendlyName);

            builder.HasOne(c => c.Treatment)
                .WithMany(t => t.Cuppings)
                .HasForeignKey(c => c.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}