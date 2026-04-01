using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// TemporaryPermission实体映射配置
    /// </summary>
    internal class TemporaryPermissionMap : IEntityTypeConfiguration<TemporaryPermission>
    {
        public void Configure(EntityTypeBuilder<TemporaryPermission> builder)
        {
            // 表名配置
            builder.ToTable("temporary_permissions");
            
            // 主键配置
            builder.HasKey(tp => tp.TempPermissionId);
            
            // 字段配置
            builder.Property(tp => tp.ScenarioType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(tp => tp.UserId)
                .IsRequired();
                
            builder.Property(tp => tp.PermissionId)
                .IsRequired();
                
            builder.Property(tp => tp.ResourceType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(tp => tp.ResourceId)
                .IsRequired();
                
            builder.Property(tp => tp.SourceTenantId)
                .IsRequired();
                
            builder.Property(tp => tp.GrantReason)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(tp => tp.GrantedBy)
                .IsRequired();
                
            builder.Property(tp => tp.GrantedAt)
                .IsRequired();
                
            builder.Property(tp => tp.ValidFrom)
                .IsRequired();
                
            builder.Property(tp => tp.ValidTo)
                .IsRequired();
                
            builder.Property(tp => tp.RequiresPatientConsent)
                .HasDefaultValue(false);
                
            builder.Property(tp => tp.PatientConsentStatus)
                .HasMaxLength(20);
                
            builder.Property(tp => tp.RequiresAdminApproval)
                .HasDefaultValue(false);
                
            builder.Property(tp => tp.AdminApprovalStatus)
                .HasMaxLength(20);
                
            builder.Property(tp => tp.ApprovalComments)
                .HasMaxLength(500);
                
            builder.Property(tp => tp.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Active");
                
            builder.Property(tp => tp.RevokeReason)
                .HasMaxLength(500);
                
            builder.Property(tp => tp.AutoRevoke)
                .HasDefaultValue(true);
                
            builder.Property(tp => tp.AccessLimit)
                .HasDefaultValue(0);
                
            builder.Property(tp => tp.AccessCount)
                .HasDefaultValue(0);
                
            builder.Property(tp => tp.Remarks)
                .HasMaxLength(1000);
            
            // 外键关系配置
            builder.HasOne(tp => tp.User)
                .WithMany()
                .HasForeignKey(tp => tp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.Permission)
                .WithMany()
                .HasForeignKey(tp => tp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.GrantedByUser)
                .WithMany()
                .HasForeignKey(tp => tp.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.ApprovedByUser)
                .WithMany()
                .HasForeignKey(tp => tp.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.RevokedByUser)
                .WithMany()
                .HasForeignKey(tp => tp.RevokedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.SourceTenant)
                .WithMany()
                .HasForeignKey(tp => tp.SourceTenantId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(tp => tp.TargetTenant)
                .WithMany()
                .HasForeignKey(tp => tp.TargetTenantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // 索引配置
            builder.HasIndex(tp => tp.ScenarioType)
                .HasDatabaseName("IX_TemporaryPermission_ScenarioType");
                
            builder.HasIndex(tp => tp.UserId)
                .HasDatabaseName("IX_TemporaryPermission_UserId");
                
            builder.HasIndex(tp => new { tp.ResourceType, tp.ResourceId })
                .HasDatabaseName("IX_TemporaryPermission_Resource");
                
            builder.HasIndex(tp => tp.Status)
                .HasDatabaseName("IX_TemporaryPermission_Status");
                
            builder.HasIndex(tp => new { tp.ValidFrom, tp.ValidTo })
                .HasDatabaseName("IX_TemporaryPermission_ValidPeriod");
                
            builder.HasIndex(tp => tp.SourceTenantId)
                .HasDatabaseName("IX_TemporaryPermission_SourceTenantId");
        }
    }
}
