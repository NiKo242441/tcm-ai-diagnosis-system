using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 库存明细表
    /// </summary>
    [Table("inventory_items")]
    public class InventoryItem
    {
        public InventoryItem()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
            IsActive = true;
            Status = "Normal";
            QualityStatus = "Pending";
            Unit = "克";
            BatchNumber = string.Empty;
        }

        /// <summary>
        /// 库存明细ID（主键）
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("batch_number")]
        public string BatchNumber { get; set; }

        /// <summary>
        /// 药材ID
        /// </summary>
        [Required]
        [Column("herb_id")]
        public int HerbId { get; set; }

        /// <summary>
        /// 当前库存数量
        /// </summary>
        [Required]
        [Column("current_quantity")]
        [Range(0, double.MaxValue)]
        public decimal CurrentQuantity { get; set; }

        /// <summary>
        /// 初始库存数量
        /// </summary>
        [Required]
        [Column("initial_quantity")]
        [Range(0.01, double.MaxValue)]
        public decimal InitialQuantity { get; set; }

        /// <summary>
        /// 计量单位
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("unit")]
        public string Unit { get; set; }

        /// <summary>
        /// 库存位置
        /// </summary>
        [StringLength(100)]
        [Column("storage_location")]
        public string? StorageLocation { get; set; }

        /// <summary>
        /// 采购单价
        /// </summary>
        [Required]
        [Column("purchase_price", TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        /// <summary>
        /// 销售单价
        /// </summary>
        [Column("sale_price", TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        [Column("production_date")]
        public DateTime? ProductionDate { get; set; }

        /// <summary>
        /// 有效期至
        /// </summary>
        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// 供应商名称
        /// </summary>
        [StringLength(200)]
        [Column("supplier_name")]
        public string? SupplierName { get; set; }

        /// <summary>
        /// 库存状态：Normal-正常, LowStock-低库存, Expiring-临期, Expired-过期, OutOfStock-缺货
        /// </summary>
        [Required]
        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// 质量状态：Pending-待检验, Qualified-合格, Unqualified-不合格
        /// </summary>
        [Required]
        [Column("quality_status")]
        [StringLength(20)]
        public string QualityStatus { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public int? TenantId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        [Column("created_by")]
        public int? CreatedBy { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        [StringLength(500)]
        public string? Remark { get; set; }

        // 计算属性
        [NotMapped]
        public decimal TotalPurchaseAmount => InitialQuantity * PurchasePrice;

        [NotMapped]
        public decimal TotalValue => CurrentQuantity * PurchasePrice;

        [NotMapped]
        public int? DaysUntilExpiry => ExpiryDate.HasValue ?
            (int)(ExpiryDate.Value - DateTime.Now).TotalDays : null;

        [NotMapped]
        public bool IsExpired => ExpiryDate.HasValue && DateTime.Now > ExpiryDate.Value;

        [NotMapped]
        public bool IsLowStock => CurrentQuantity < InitialQuantity * 0.1m;

        // 导航属性
        [ForeignKey("HerbId")]
        public virtual Herb Herb { get; set; } = null!;

        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? Creator { get; set; }

        public virtual ICollection<InventoryOperation> InventoryOperations { get; set; } = new List<InventoryOperation>();
    }
}