using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class LifestyleAdviceMap : IEntityTypeConfiguration<LifestyleAdvice>
    {
        public void Configure(EntityTypeBuilder<LifestyleAdvice> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(64);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(128);
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.Rationale).IsRequired();
            builder.Property(x => x.Implementation).IsRequired();
            builder.Property(x => x.Frequency).IsRequired().HasMaxLength(128);
            builder.Property(x => x.Precautions).IsRequired();
            builder.Property(x => x.Benefits).IsRequired();

            builder.HasOne(la => la.Treatment)
                .WithMany(t => t.LifestyleAdvices)
                .HasForeignKey(la => la.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}