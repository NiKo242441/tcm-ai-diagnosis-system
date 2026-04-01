using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    /// <summary>
    /// Syndrome实体映射配置
    /// </summary>
    public class SyndromeMap : IEntityTypeConfiguration<Syndrome>
    {
        /// <summary>
        /// 配置Syndrome实体的映射关系
        /// </summary>
        /// <param name="builder">实体类型构建器</param>
        public void Configure(EntityTypeBuilder<Syndrome> builder)
        {
            // 表名配置
            builder.ToTable("Syndromes");

            // 主键配置
            builder.HasKey(s => s.SyndromeId);

            // 字段配置
            builder.Property(s => s.SyndromeName)
                .IsRequired();

            builder.Property(s => s.Confidence)
                .HasPrecision(5, 2); // 精度为5位，小数点后2位

            // 索引配置
            builder.HasIndex(s => s.VisitId);
            builder.HasIndex(s => s.SyndromeName);
            builder.HasIndex(s => s.Confidence);
            builder.HasIndex(s => s.IsConfirmed);

            // 外键关系配置
            builder.HasOne(x => x.Visit)
                .WithMany()
                .HasForeignKey(s => s.VisitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}