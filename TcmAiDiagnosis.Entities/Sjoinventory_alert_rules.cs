using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 库存预警规则表
    /// </summary>
    [Table("inventory_alert_rules")]
    public class InventoryAlertRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        [Required]
        [Column("rule_name")]
        [StringLength(100)]
        public string RuleName { get; set; }

        /// <summary>
        /// 药材ID（为null表示适用于所有药材）
        /// </summary>
        [Column("herb_id")]
        public int? HerbId { get; set; }

        /// <summary>
        /// 预警类型：LowStock-低库存, Expiring-临期, Overstock-积压
        /// </summary>
        [Required]
        [Column("alert_type")]
        [StringLength(20)]
        public string AlertType { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        [Required]
        [Column("threshold")]
        public decimal Threshold { get; set; }

        /// <summary>
        /// 比较运算符：LT-小于, GT-大于, LTE-小于等于, GTE-大于等于
        /// </summary>
        [Required]
        [Column("comparison_operator")]
        [StringLength(10)]
        public string ComparisonOperator { get; set; } = "LT";

        /// <summary>
        /// 通知人员ID（多个用逗号分隔）
        /// </summary>
        [Column("notify_user_ids")]
        [StringLength(500)]
        public string NotifyUserIds { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 优先级
        /// </summary>
        [Column("priority")]
        public int Priority { get; set; } = 2;

        /// <summary>
        /// 租户ID
        /// </summary>
        [Column("tenant_id")]
        public int? TenantId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("remark")]
        [StringLength(500)]
        public string Remark { get; set; }

        // 导航属性
        [ForeignKey("HerbId")]
        public virtual Herb Herb { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; }
    }
}