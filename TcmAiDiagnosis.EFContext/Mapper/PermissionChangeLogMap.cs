using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// PermissionChangeLog实体映射配置
    /// </summary>
    internal class PermissionChangeLogMap : IEntityTypeConfiguration<PermissionChangeLog>
    {
        public void Configure(EntityTypeBuilder<PermissionChangeLog> builder)
        {
            // 表名配置
            builder.ToTable("permission_change_logs");
            
            // 主键配置
            builder.HasKey(pcl => pcl.LogId);
            
            // 字段配置
            builder.Property(pcl => pcl.ChangeType)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(pcl => pcl.TargetType)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(pcl => pcl.TargetId)
                .IsRequired();
                
            builder.Property(pcl => pcl.TargetName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.PermissionId)
                .IsRequired();
                
            builder.Property(pcl => pcl.PermissionCode)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.PermissionName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.BeforeState)
                .HasColumnType("text");
                
            builder.Property(pcl => pcl.AfterState)
                .HasColumnType("text");
                
            builder.Property(pcl => pcl.ChangeReason)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(pcl => pcl.ApplicantName)
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.ApproverName)
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.ApprovalStatus)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Approved");
                
            builder.Property(pcl => pcl.ApprovalComments)
                .HasMaxLength(500);
                
            builder.Property(pcl => pcl.OperatedBy)
                .IsRequired();
                
            builder.Property(pcl => pcl.OperatedByName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(pcl => pcl.OperatedAt)
                .IsRequired();
                
            builder.Property(pcl => pcl.ClientIp)
                .IsRequired()
                .HasMaxLength(45);
                
            builder.Property(pcl => pcl.UserAgent)
                .HasMaxLength(500);
                
            builder.Property(pcl => pcl.IsCrossTenant)
                .HasDefaultValue(false);
                
            builder.Property(pcl => pcl.Remarks)
                .HasMaxLength(1000);
            
            // 外键关系配置
            builder.HasOne(pcl => pcl.Permission)
                .WithMany()
                .HasForeignKey(pcl => pcl.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(pcl => pcl.Applicant)
                .WithMany()
                .HasForeignKey(pcl => pcl.ApplicantId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(pcl => pcl.Approver)
                .WithMany()
                .HasForeignKey(pcl => pcl.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(pcl => pcl.Operator)
                .WithMany()
                .HasForeignKey(pcl => pcl.OperatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(pcl => pcl.Tenant)
                .WithMany()
                .HasForeignKey(pcl => pcl.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // 索引配置
            builder.HasIndex(pcl => pcl.ChangeType)
                .HasDatabaseName("IX_PermissionChangeLog_ChangeType");
                
            builder.HasIndex(pcl => new { pcl.TargetType, pcl.TargetId })
                .HasDatabaseName("IX_PermissionChangeLog_Target");
                
            builder.HasIndex(pcl => pcl.PermissionId)
                .HasDatabaseName("IX_PermissionChangeLog_PermissionId");
                
            builder.HasIndex(pcl => pcl.OperatedAt)
                .HasDatabaseName("IX_PermissionChangeLog_OperatedAt");
                
            builder.HasIndex(pcl => pcl.TenantId)
                .HasDatabaseName("IX_PermissionChangeLog_TenantId");
        }
    }
}
