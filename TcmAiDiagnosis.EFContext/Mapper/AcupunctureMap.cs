using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class AcupunctureMap : IEntityTypeConfiguration<Acupuncture>
    {
        public void Configure(EntityTypeBuilder<Acupuncture> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.PointName).IsRequired();
            builder.Property(a => a.Location);
            builder.Property(a => a.Method);
            builder.Property(a => a.Technique);
            builder.Property(a => a.NeedleSpecification);
            builder.Property(a => a.Depth);
            builder.Property(a => a.Duration);
            builder.Property(a => a.Frequency);
            builder.Property(a => a.Efficacy);
            builder.Property(a => a.Indications);
            builder.Property(a => a.Instructions);
            builder.Property(a => a.Notes);
            builder.Property(a => a.Contraindications);
            builder.Property(a => a.PatientFriendlyName);
            builder.Property(a => a.PatientFriendlyDescription);
            builder.Property(a => a.InstructionVideoUrl);

            builder.HasOne(a => a.Treatment)
                .WithMany(t => t.Acupunctures)
                .HasForeignKey(a => a.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}