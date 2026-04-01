using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// RolePermission实体映射配置
    /// </summary>
    internal class RolePermissionMap : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            // 表名配置
            builder.ToTable("role_permissions");
            
            // 复合主键配置
            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            
            // 字段配置
            builder.Property(rp => rp.IsGranted)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(rp => rp.GrantedAt)
                .IsRequired();
                
            builder.Property(rp => rp.Remarks)
                .HasMaxLength(500);
            
            // 外键关系配置
            builder.HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(rp => rp.GrantedByUser)
                .WithMany()
                .HasForeignKey(rp => rp.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            // 索引配置
            builder.HasIndex(rp => rp.RoleId)
                .HasDatabaseName("IX_RolePermission_RoleId");
                
            builder.HasIndex(rp => rp.PermissionId)
                .HasDatabaseName("IX_RolePermission_PermissionId");
        }
    }
}
