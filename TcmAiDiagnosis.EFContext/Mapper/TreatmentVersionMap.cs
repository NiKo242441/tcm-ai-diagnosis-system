using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class TreatmentVersionMap : IEntityTypeConfiguration<TreatmentVersion>
    {
        public void Configure(EntityTypeBuilder<TreatmentVersion> builder)
        {
            builder.HasKey(tv => tv.Id);
            builder.Property(tv => tv.Version).IsRequired();
            builder.Property(tv => tv.Content).IsRequired();

            builder.HasOne(tv => tv.Treatment)
                .WithMany(t => t.TreatmentVersions)
                .HasForeignKey(tv => tv.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tv => tv.CreatedByUser)
                .WithMany()
                .HasForeignKey(tv => tv.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}