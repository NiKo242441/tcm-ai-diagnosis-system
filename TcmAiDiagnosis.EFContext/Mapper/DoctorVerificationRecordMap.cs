using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class DoctorVerificationRecordMap : IEntityTypeConfiguration<DoctorVerificationRecord>
    {
        public void Configure(EntityTypeBuilder<DoctorVerificationRecord> builder)
        {
            builder.ToTable("DoctorVerificationRecords");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Treatment)
                .WithMany()
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Reviewer)
                .WithMany()
                .HasForeignKey(x => x.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}