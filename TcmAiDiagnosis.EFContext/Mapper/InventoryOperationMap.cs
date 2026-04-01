using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class InventoryOperationMap : IEntityTypeConfiguration<InventoryOperation>
    {
        public void Configure(EntityTypeBuilder<InventoryOperation> builder)
        {
            // 表名
            builder.ToTable("inventory_operations");

            // 主键
            builder.HasKey(e => e.Id);

            // 属性配置
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.InventoryItemId)
                .HasColumnName("inventory_item_id")
                .IsRequired();

            builder.Property(e => e.OperationType)
                .HasColumnName("operation_type")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.BeforeQuantity)
                .HasColumnName("before_quantity")
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.AfterQuantity)
                .HasColumnName("after_quantity")
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.ReferenceNumber)
                .HasColumnName("reference_number")
                .HasMaxLength(100);

            builder.Property(e => e.Reason)
                .HasColumnName("reason")
                .HasMaxLength(500);

            builder.Property(e => e.OperatedBy)
                .HasColumnName("operated_by")
                .IsRequired();

            // 修正：移除 HasDefaultValueSql，在实体类构造函数中设置默认值
            builder.Property(e => e.OperatedAt)
                .HasColumnName("operated_at");
            // 移除 .HasDefaultValueSql("GETDATE()")

            builder.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            builder.Property(e => e.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            // 索引
            builder.HasIndex(e => e.InventoryItemId)
                .HasDatabaseName("IX_inventory_operations_inventory_item_id");

            builder.HasIndex(e => e.OperationType)
                .HasDatabaseName("IX_inventory_operations_operation_type");

            builder.HasIndex(e => e.OperatedAt)
                .HasDatabaseName("IX_inventory_operations_operated_at");

            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_inventory_operations_tenant_id");

            builder.HasIndex(e => e.ReferenceNumber)
                .HasDatabaseName("IX_inventory_operations_reference_number");

            // 关系配置
            builder.HasOne(e => e.InventoryItem)
                .WithMany(i => i.InventoryOperations)
                .HasForeignKey(e => e.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Operator)
                .WithMany()
                .HasForeignKey(e => e.OperatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}