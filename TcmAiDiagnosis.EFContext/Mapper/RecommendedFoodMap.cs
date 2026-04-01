using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class RecommendedFoodMap : IEntityTypeConfiguration<RecommendedFood>
    {
        public void Configure(EntityTypeBuilder<RecommendedFood> builder)
        {
            builder.ToTable("RecommendedFoods");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FoodName).IsRequired();
            builder.HasOne(x => x.DietaryAdvice)
                .WithMany()
                .HasForeignKey(x => x.DietaryAdviceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}