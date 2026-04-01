using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class DietaryTherapyMap : IEntityTypeConfiguration<DietaryTherapy>
    {
        public void Configure(EntityTypeBuilder<DietaryTherapy> builder)
        {
            builder.HasKey(dt => dt.Id);
            builder.Property(dt => dt.Name).IsRequired();
            builder.Property(dt => dt.Category).IsRequired();
            builder.Property(dt => dt.Description).IsRequired();
            builder.Property(dt => dt.Preparation).IsRequired();
            builder.Property(dt => dt.Efficacy).IsRequired();
            builder.Property(dt => dt.SuitableFor).IsRequired();
            builder.Property(dt => dt.Contraindications).IsRequired();
            builder.Property(dt => dt.ServingMethod).IsRequired();
            builder.Property(dt => dt.StorageMethod).IsRequired();

            builder.HasOne(dt => dt.Treatment)
                .WithMany(t => t.DietaryTherapies)
                .HasForeignKey(dt => dt.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(dt => dt.DietaryTherapyIngredients)
                .WithOne(dti => dti.DietaryTherapy)
                .HasForeignKey(dti => dti.DietaryTherapyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}