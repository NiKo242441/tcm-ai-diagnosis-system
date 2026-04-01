using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    internal class TenantMap : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            // Ö÷¼üÅäÖÃ
            builder.HasKey(x => x.TenantId);

            // Ë÷ÒýÅäÖÃ
            builder.HasIndex(x => x.TenantName);
            builder.HasIndex(x => x.Status);

            // Ä¬ÈÏÖµÅäÖÃ
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)").ValueGeneratedOnUpdate();

            // µ¼º½¹ØÏµÅäÖÃ
            builder.HasMany(x => x.Users)
                .WithOne(x => x.Tenant)
                .HasForeignKey(x => x.TenantId)
                .IsRequired(false);
        }
    }
}
