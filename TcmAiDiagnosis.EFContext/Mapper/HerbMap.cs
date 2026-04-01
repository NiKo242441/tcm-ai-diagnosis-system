using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// Herb实体映射配置
    /// </summary>
    public class HerbMap : IEntityTypeConfiguration<Herb>
    {
        /// <summary>
        /// 配置Herb实体的映射关系
        /// </summary>
        /// <param name="builder">实体类型构建器</param>
        public void Configure(EntityTypeBuilder<Herb> builder)
        {
            // 表名配置
            builder.ToTable("Herbs");
            
            // 主键配置 - 映射到现有的HerbId字段
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id).HasColumnName("HerbId");
            
            // 字段配置 - 映射到现有的数据库字段
            builder.Property(h => h.Name)
                .HasColumnName("HerbName")
                .IsRequired();

            builder.Property(h => h.Properties)
                .IsRequired(false);
                
            builder.Property(h => h.Meridians)
                .IsRequired(false);
                
            builder.Property(h => h.Efficacy)
                .IsRequired(false);
                
            builder.Property(h => h.Category)
                .IsRequired(false);
                
            builder.Property(h => h.ToxicityLevel)
                .IsRequired(false);
                
            builder.Property(h => h.DosageRange)
                .HasColumnName("Dosage");
                
            builder.Property(h => h.CommonUnit)
                .HasDefaultValue("g");
                
            builder.Property(h => h.Precautions)
                .IsRequired(false);
                
            builder.Property(h => h.Contraindications)
                .IsRequired(false);
                
            builder.Property(h => h.ProcessingMethods)
                .HasColumnName("ProcessingMethod")
                .IsRequired(false);
                
            builder.Property(h => h.IsCommonlyUsed)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(h => h.RequiresSpecialHandling)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(h => h.SpecialUsageInstructions)
                .HasColumnName("Remarks")
                .IsRequired(false);
                
            builder.Property(h => h.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                
            builder.Property(h => h.UpdatedAt)
                .IsRequired(false)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                
            builder.Property(h => h.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(h => h.TenantId)
                .IsRequired();
                
            builder.Property(h => h.IsAiOriginated)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(h => h.Status)
                .IsRequired()
                .HasDefaultValue(TcmAiDiagnosis.Entities.Enums.ReviewStatus.AIGenerated);
                
            // 索引配置
            builder.HasIndex(h => h.Name);
            builder.HasIndex(h => h.Category);
            builder.HasIndex(h => h.ToxicityLevel);
            builder.HasIndex(h => h.IsCommonlyUsed);
            builder.HasIndex(h => h.TenantId);
            builder.HasIndex(h => h.Status);
            builder.HasIndex(h => h.IsDeleted);
            builder.HasIndex(h => new { h.Name, h.TenantId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
                
            // 关系配置
                
            builder.HasMany(h => h.HerbContraindications)
                .WithOne(hc => hc.PrimaryHerb)
                .HasForeignKey(hc => hc.PrimaryHerbId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}