using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class AffectedMedicationMap : IEntityTypeConfiguration<AffectedMedication>
    {
        public void Configure(EntityTypeBuilder<AffectedMedication> builder)
        {
            builder.HasKey(am => am.Id);
            builder.Property(am => am.MedicationName).IsRequired().HasMaxLength(128);
        }
    }
}