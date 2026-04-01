using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class DietaryAdviceMap : IEntityTypeConfiguration<DietaryAdvice>
    {
        public void Configure(EntityTypeBuilder<DietaryAdvice> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Category).IsRequired().HasMaxLength(64);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(128);
            builder.Property(x => x.DietaryPrinciples).IsRequired();
            builder.Property(x => x.MealTiming).IsRequired();
            builder.Property(x => x.CookingMethods).IsRequired();
            builder.Property(x => x.Rationale).IsRequired();
            builder.Property(x => x.SeasonalAdjustment).IsRequired();
            builder.Property(x => x.Precautions).IsRequired();

            builder.HasOne(da => da.Treatment)
                .WithMany(t => t.DietaryAdvices)
                .HasForeignKey(da => da.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(da => da.RecommendedFoods)
                .WithOne(rf => rf.DietaryAdvice)
                .HasForeignKey(rf => rf.DietaryAdviceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(da => da.AvoidedFoods)
                .WithOne(af => af.DietaryAdvice)
                .HasForeignKey(af => af.DietaryAdviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}