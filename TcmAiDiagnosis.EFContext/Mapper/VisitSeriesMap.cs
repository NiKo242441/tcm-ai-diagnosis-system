using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class VisitSeriesMap : IEntityTypeConfiguration<VisitSeries>
    {
        public void Configure(EntityTypeBuilder<VisitSeries> builder)
        {
            builder.HasKey(x => x.SeriesId);
            builder.HasIndex(x => x.TenantId);
            builder.HasIndex(x => x.PatientUserId);
            builder.HasIndex(x => x.DoctorUserId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.SeriesStartDate);
            builder.HasIndex(x => x.SeriesEndDate);
            builder.HasIndex(x => x.NextFollowUpDate);
            builder.HasIndex(x => x.NextFollowUpVisitDate);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)").ValueGeneratedOnUpdate();
            
            // 外键关系配置
            builder.HasOne(x => x.DoctorUser).WithMany().HasForeignKey(x => x.DoctorUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PatientUser).WithMany().HasForeignKey(x => x.PatientUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
