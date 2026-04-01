using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class DietaryTherapyIngredientMap : IEntityTypeConfiguration<DietaryTherapyIngredient>
    {
        public void Configure(EntityTypeBuilder<DietaryTherapyIngredient> builder)
        {
            builder.HasKey(dti => dti.Id);
            builder.Property(dti => dti.Name).IsRequired();
            builder.Property(dti => dti.Dosage).IsRequired();
            builder.Property(dti => dti.ProcessingMethod).IsRequired();
            builder.Property(dti => dti.Notes).IsRequired(false);
        }
    }
}