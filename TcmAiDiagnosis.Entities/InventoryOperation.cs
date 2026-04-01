using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 库存操作记录表
    /// </summary>
    [Table("inventory_operations")]
    public class InventoryOperation
    {
        public InventoryOperation()
        {
            OperatedAt = DateTime.Now;
            OperationType = string.Empty;
        }

        /// <summary>
        /// 操作记录ID（主键）
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 库存明细ID
        /// </summary>
        [Required]
        [Column("inventory_item_id")]
        public int InventoryItemId { get; set; }

        /// <summary>
        /// 操作类型：PurchaseIn-采购入库, SaleOut-销售出库, Adjust-库存调整, Transfer-调拨, Return-退货, Loss-报损
        /// </summary>
        [Required]
        [Column("operation_type")]
        [StringLength(20)]
        public string OperationType { get; set; }

        /// <summary>
        /// 操作数量
        /// </summary>
        [Required]
        [Column("quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// 操作前库存数量
        /// </summary>
        [Required]
        [Column("before_quantity")]
        public decimal BeforeQuantity { get; set; }

        /// <summary>
        /// 操作后库存数量
        /// </summary>
        [Required]
        [Column("after_quantity")]
        public decimal AfterQuantity { get; set; }

        /// <summary>
        /// 关联单据号
        /// </summary>
        [Column("reference_number")]
        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        /// <summary>
        /// 操作原因
        /// </summary>
        [Column("reason")]
        [StringLength(500)]
        public string? Reason { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        [Required]
        [Column("operated_by")]
        public int OperatedBy { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [Column("operated_at")]
        public DateTime OperatedAt { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public int? TenantId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Column("remark")]
        [StringLength(500)]
        public string? Remark { get; set; }

        // 导航属性
        [ForeignKey("InventoryItemId")]
        public virtual InventoryItem InventoryItem { get; set; } = null!;

        [ForeignKey("OperatedBy")]
        public virtual User Operator { get; set; } = null!;

        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}