using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class PatientTenantMap : IEntityTypeConfiguration<PatientTenant>
    {
        public void Configure(EntityTypeBuilder<PatientTenant> builder)
        {
            // 配置复合主键
            builder.HasKey(x => new { x.PatientUserId, x.TenantId });

            // 索引配置
            builder.HasIndex(x => x.PatientUserId);
            builder.HasIndex(x => x.TenantId);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.RelationType);

            // 自动时间戳配置
            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            builder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .ValueGeneratedOnUpdate();

            // 可空字段配置
            builder.Property(x => x.AddedBy)
                .IsRequired(false);

            builder.Property(x => x.AddedAt)
                .IsRequired(false);

            builder.Property(x => x.ReactivatedBy)
                .IsRequired(false);

            builder.Property(x => x.ReactivatedAt)
                .IsRequired(false);

            builder.Property(x => x.RemovedBy)
                .IsRequired(false);

            builder.Property(x => x.RemovedAt)
                .IsRequired(false);

            // 默认值配置
            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.RelationType)
                .HasDefaultValue(PatientTenantRelationType.Original);

            // 配置导航关系
            builder.HasOne(x => x.PatientUser)
                .WithMany(x => x.PatientTenants)
                .HasForeignKey(x => x.PatientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Tenant)
                .WithMany(x => x.PatientTenants)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}