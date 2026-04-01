using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class AvoidedFoodMap : IEntityTypeConfiguration<AvoidedFood>
    {
        public void Configure(EntityTypeBuilder<AvoidedFood> builder)
        {
            builder.HasKey(af => af.Id);
            builder.Property(af => af.FoodName).IsRequired().HasMaxLength(128);
        }
    }
}