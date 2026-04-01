using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class RenewalMap : IEntityTypeConfiguration<Renewal>
    {
        public void Configure(EntityTypeBuilder<Renewal> builder)
        {
            builder.HasKey(x => x.RenewalId);
            builder.HasIndex(x => x.TenantId);
            builder.HasIndex(x => x.RenewalDate);
            builder.HasIndex(x => x.ValidFrom);
            builder.HasIndex(x => x.ValidTo);
            builder.Property(x => x.RenewalDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            // 外键关系配置
            builder.HasOne(x => x.Tenant).WithMany(x=>x.Renewals).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
