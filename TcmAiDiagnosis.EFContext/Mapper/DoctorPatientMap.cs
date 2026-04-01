using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class DoctorPatientMap : IEntityTypeConfiguration<DoctorPatient>
    {
        public void Configure(EntityTypeBuilder<DoctorPatient> builder)
        {
            // 复合主键配置
            builder.HasKey(x => new { x.DoctorUserId, x.PatientUserId });

            // 索引配置
            builder.HasIndex(x => x.DoctorUserId);
            builder.HasIndex(x => x.PatientUserId);

            // 时间戳配置
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)").ValueGeneratedOnUpdate();

            // 导航关系配置
            builder.HasOne(x => x.DoctorUser)
                .WithMany(x => x.PatientDoctors)
                .HasForeignKey(x => x.DoctorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientUser)
                .WithMany(x => x.DoctorPatients)
                .HasForeignKey(x => x.PatientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}