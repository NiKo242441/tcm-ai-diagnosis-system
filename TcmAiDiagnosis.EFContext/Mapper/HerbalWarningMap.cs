using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class HerbalWarningMap : IEntityTypeConfiguration<HerbalWarning>
    {
        public void Configure(EntityTypeBuilder<HerbalWarning> builder)
        {
            builder.HasKey(hw => hw.Id);

            builder.Property(hw => hw.WarningType).IsRequired().HasMaxLength(64);
            builder.Property(hw => hw.Title).IsRequired().HasMaxLength(128);
            builder.Property(hw => hw.Content).IsRequired();
            builder.Property(hw => hw.SeverityLevel).IsRequired().HasMaxLength(32);
            builder.Property(hw => hw.SymptomsToWatch).IsRequired();
            builder.Property(hw => hw.ActionRequired).IsRequired();
            builder.Property(hw => hw.PreventionMeasures).IsRequired();
            builder.Property(hw => hw.SpecialPopulations).IsRequired();

            builder.HasOne(hw => hw.Treatment)
                .WithMany(t => t.HerbalWarnings)
                .HasForeignKey(hw => hw.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(hw => hw.AffectedMedications)
                .WithOne(am => am.HerbalWarning)
                .HasForeignKey(am => am.HerbalWarningId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}