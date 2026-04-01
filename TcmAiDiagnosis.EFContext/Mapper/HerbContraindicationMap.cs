using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// HerbContraindication实体映射配置
    /// </summary>
    public class HerbContraindicationMap : IEntityTypeConfiguration<HerbContraindication>
    {
        /// <summary>
        /// 配置HerbContraindication实体的映射关系
        /// </summary>
        /// <param name="builder">实体类型构建器</param>
        public void Configure(EntityTypeBuilder<HerbContraindication> builder)
        {
            // 表名配置
            builder.ToTable("HerbContraindications");
            
            // 主键配置
            builder.HasKey(hc => hc.ContraindicationId);
            
            // 字段配置
            builder.Property(hc => hc.PrimaryHerbId)
                .IsRequired();
                
            builder.Property(hc => hc.ConflictHerbId)
                .IsRequired();
                
            builder.Property(hc => hc.ContraindicationType)
                .IsRequired();
                
            builder.Property(hc => hc.RiskLevel)
                .IsRequired()
                .HasDefaultValue(1);
                
            builder.Property(hc => hc.Description)
                .IsRequired();
                
            builder.Property(hc => hc.IsAbsoluteContraindication)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(hc => hc.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(hc => hc.CreatedAt)
                .IsRequired();
                
            builder.Property(hc => hc.UpdatedAt)
                .IsRequired();
                
            builder.Property(hc => hc.ReviewStatus)
                .IsRequired()
                .HasDefaultValue(0);
                
            // 索引配置
            builder.HasIndex(hc => hc.PrimaryHerbId);
            builder.HasIndex(hc => hc.ConflictHerbId);
            builder.HasIndex(hc => hc.ContraindicationType);
            builder.HasIndex(hc => hc.RiskLevel);
            builder.HasIndex(hc => hc.IsAbsoluteContraindication);
            builder.HasIndex(hc => hc.IsActive);
            builder.HasIndex(hc => hc.ReviewStatus);
            builder.HasIndex(hc => new { hc.PrimaryHerbId, hc.ConflictHerbId })
                .IsUnique()
                .HasFilter("[IsActive] = 1");
                
            // 外键关系配置
            builder.HasOne(hc => hc.PrimaryHerb)
                .WithMany(h => h.HerbContraindications)
                .HasForeignKey(hc => hc.PrimaryHerbId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(hc => hc.ConflictHerb)
                .WithMany()
                .HasForeignKey(hc => hc.ConflictHerbId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(hc => hc.Creator)
                .WithMany()
                .HasForeignKey(hc => hc.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.HasOne(hc => hc.Updater)
                .WithMany()
                .HasForeignKey(hc => hc.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);
                
            builder.HasOne(hc => hc.Reviewer)
                .WithMany()
                .HasForeignKey(hc => hc.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}