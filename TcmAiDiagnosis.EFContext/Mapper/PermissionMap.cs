using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// Permission实体映射配置
    /// </summary>
    internal class PermissionMap : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            // 表名配置
            builder.ToTable("permissions");
            
            // 主键配置
            builder.HasKey(p => p.PermissionId);
            
            // 字段配置
            builder.Property(p => p.PermissionCode)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.PermissionName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.Description)
                .HasMaxLength(500);
                
            builder.Property(p => p.Category)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Module)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.IsSystem)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(p => p.SortOrder)
                .HasDefaultValue(0);
                
            builder.Property(p => p.CreatedAt)
                .IsRequired();
                
            builder.Property(p => p.UpdatedAt)
                .IsRequired();
            
            // 索引配置
            builder.HasIndex(p => p.PermissionCode)
                .IsUnique()
                .HasDatabaseName("IX_Permission_Code");
                
            builder.HasIndex(p => p.Category)
                .HasDatabaseName("IX_Permission_Category");
                
            builder.HasIndex(p => p.Module)
                .HasDatabaseName("IX_Permission_Module");
        }
    }
}
