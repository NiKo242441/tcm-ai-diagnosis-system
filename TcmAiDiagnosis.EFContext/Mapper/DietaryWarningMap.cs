using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class DietaryWarningMap : IEntityTypeConfiguration<DietaryWarning>
    {
        public void Configure(EntityTypeBuilder<DietaryWarning> builder)
        {
            builder.ToTable("DietaryWarnings");
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.Treatment)
                .WithMany()
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}