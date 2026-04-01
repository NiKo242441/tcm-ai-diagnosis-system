using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class InventoryAlertRuleMap : IEntityTypeConfiguration<InventoryAlertRule>
    {
        public void Configure(EntityTypeBuilder<InventoryAlertRule> builder)
        {
            // 表名
            builder.ToTable("inventory_alert_rules");

            // 主键
            builder.HasKey(e => e.Id);

            // 属性配置
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.RuleName)
                .HasColumnName("rule_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.HerbId)
                .HasColumnName("herb_id");

            builder.Property(e => e.AlertType)
                .HasColumnName("alert_type")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Threshold)
                .HasColumnName("threshold")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.ComparisonOperator)
                .HasColumnName("comparison_operator")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(e => e.NotifyUserIds)
                .HasColumnName("notify_user_ids")
                .HasMaxLength(500);

            builder.Property(e => e.IsEnabled)
                .HasColumnName("is_enabled");

            builder.Property(e => e.Priority)
                .HasColumnName("priority");

            builder.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            // 移除 HasDefaultValueSql，在实体类构造函数中设置默认值
            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            builder.Property(e => e.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            // 索引
            builder.HasIndex(e => e.HerbId)
                .HasDatabaseName("IX_inventory_alert_rules_herb_id");

            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_inventory_alert_rules_tenant_id");

            builder.HasIndex(e => new { e.AlertType, e.IsEnabled })
                .HasDatabaseName("IX_inventory_alert_rules_alert_type_enabled");

            builder.HasIndex(e => e.RuleName)
                .HasDatabaseName("IX_inventory_alert_rules_rule_name");

            // 关系配置
            builder.HasOne(e => e.Herb)
                .WithMany()
                .HasForeignKey(e => e.HerbId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}