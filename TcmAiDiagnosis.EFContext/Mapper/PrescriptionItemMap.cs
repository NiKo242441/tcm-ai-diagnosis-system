using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class PrescriptionItemMap : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.Name).IsRequired();
            builder.Property(pi => pi.Dosage).IsRequired();
            builder.Property(pi => pi.Unit).IsRequired();
        }
    }
}