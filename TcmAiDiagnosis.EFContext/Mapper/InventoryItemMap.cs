using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.EFContext.Mapper
{
    public class InventoryItemMap : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            // 表名
            builder.ToTable("inventory_items");

            // 主键
            builder.HasKey(e => e.Id);

            // 属性配置
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.BatchNumber)
                .HasColumnName("batch_number")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.HerbId)
                .HasColumnName("herb_id")
                .IsRequired();

            builder.Property(e => e.CurrentQuantity)
                .HasColumnName("current_quantity")
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.InitialQuantity)
                .HasColumnName("initial_quantity")
                .HasColumnType("decimal(18,4)");

            builder.Property(e => e.Unit)
                .HasColumnName("unit")
                .HasMaxLength(20)
                .IsRequired();
            // 移除 .HasDefaultValue("克") - 在实体类构造函数中设置

            builder.Property(e => e.StorageLocation)
                .HasColumnName("storage_location")
                .HasMaxLength(100);

            builder.Property(e => e.PurchasePrice)
                .HasColumnName("purchase_price")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.SalePrice)
                .HasColumnName("sale_price")
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.ProductionDate)
                .HasColumnName("production_date");

            builder.Property(e => e.ExpiryDate)
                .HasColumnName("expiry_date");

            builder.Property(e => e.SupplierName)
                .HasColumnName("supplier_name")
                .HasMaxLength(200);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            // 移除 .HasDefaultValue("Normal") - 在实体类构造函数中设置

            builder.Property(e => e.QualityStatus)
                .HasColumnName("quality_status")
                .HasMaxLength(20)
                .IsRequired();
            // 移除 .HasDefaultValue("Pending") - 在实体类构造函数中设置

            builder.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active");
            // 移除 .HasDefaultValue(true) - 在实体类构造函数中设置

            // 修正：移除 HasDefaultValueSql，在实体类构造函数中设置默认值
            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at");
            // 移除 .HasDefaultValueSql("CURRENT_TIMESTAMP")

            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");
            // 移除 .HasDefaultValueSql("CURRENT_TIMESTAMP")

            builder.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            builder.Property(e => e.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            // 索引
            builder.HasIndex(e => e.BatchNumber)
                .HasDatabaseName("IX_inventory_items_batch_number");

            builder.HasIndex(e => e.HerbId)
                .HasDatabaseName("IX_inventory_items_herb_id");

            builder.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_inventory_items_tenant_id");

            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_inventory_items_status");

            builder.HasIndex(e => e.QualityStatus)
                .HasDatabaseName("IX_inventory_items_quality_status");

            builder.HasIndex(e => e.ExpiryDate)
                .HasDatabaseName("IX_inventory_items_expiry_date");

            builder.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_inventory_items_is_active");

            // 关系配置
            builder.HasOne(e => e.Herb)
                .WithMany()
                .HasForeignKey(e => e.HerbId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // 忽略计算属性
            builder.Ignore(e => e.TotalPurchaseAmount);
            builder.Ignore(e => e.TotalValue);
            builder.Ignore(e => e.DaysUntilExpiry);
            builder.Ignore(e => e.IsExpired);
            builder.Ignore(e => e.IsLowStock);
        }
    }
}