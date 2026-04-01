using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// Role实体映射配置
    /// </summary>
    internal class RoleMap : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            // 表名配置
            builder.ToTable("AspNetRoles");
            
            // 主键配置
            builder.HasKey(r => r.Id);
            
            // 字段配置
            builder.Property(r => r.ShowName)
                .IsRequired();
                
            builder.Property(r => r.ConcurrencyStamp)
                .IsConcurrencyToken();
            
            // 索引配置
            builder.HasIndex(r => r.NormalizedName)
                .IsUnique()
                .HasDatabaseName("RoleNameIndex");
        }
    }
}