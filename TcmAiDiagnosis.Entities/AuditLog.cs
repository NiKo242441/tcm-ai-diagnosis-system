using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 审计日志表 - 记录系统所有重要操作日志，支持四级高风险操作验证
    /// </summary>
    [Table("audit_logs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("log_id")]
        public long LogId { get; set; }

        [Required]
        [Column("operation_time")]
        public DateTime OperationTime { get; set; } = DateTime.Now;

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(45)]
        [Column("ip_address")]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("operation_module")]
        public string OperationModule { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("operation_type")]
        public string OperationType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column("operation_details")]
        public string OperationDetails { get; set; } = string.Empty;

        [Column("before_snapshot", TypeName = "text")]
        public string? BeforeSnapshot { get; set; }

        [Column("after_snapshot", TypeName = "text")]
        public string? AfterSnapshot { get; set; }

        [Required]
        [StringLength(20)]
        [Column("operation_status")]
        public string OperationStatus { get; set; } = "Success";

        [StringLength(1000)]
        [Column("error_message")]
        public string? ErrorMessage { get; set; }

        [StringLength(500)]
        [Column("request_path")]
        public string? RequestPath { get; set; }

        [Column("request_parameters", TypeName = "text")]
        public string? RequestParameters { get; set; }

        [Column("user_agent", TypeName = "text")]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        [Column("session_id")]
        public string? SessionId { get; set; }

        [Column("execution_duration")]
        public long? ExecutionDuration { get; set; }

        [Column("tenant_id")]
        public int? TenantId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        // ==================== 新增高风险操作字段 ====================

        /// <summary>
        /// 风险评分 (0-10+)
        /// 0-4: 低风险, 5-6: 中风险, 7-9: 高风险, 10+: 极高风险
        /// </summary>
        [Column("risk_score")]
        public int? RiskScore { get; set; }

        /// <summary>
        /// 验证级别: Level1/Level2/Level3/Level4
        /// </summary>
        [StringLength(20)]
        [Column("verification_level")]
        public string? VerificationLevel { get; set; }

        /// <summary>
        /// 临床理由 (三级验证必需，不少于20字)
        /// </summary>
        [Column("clinical_reason", TypeName = "text")]
        public string? ClinicalReason { get; set; }

        /// <summary>
        /// 审核人ID (四级验证时使用)
        /// </summary>
        [Column("reviewer_id")]
        public int? ReviewerId { get; set; }

        /// <summary>
        /// 审核状态: Pending/Approved/Rejected
        /// </summary>
        [StringLength(20)]
        [Column("review_status")]
        public string? ReviewStatus { get; set; } = "Pending";

        /// <summary>
        /// 审核意见
        /// </summary>
        [Column("review_comments", TypeName = "text")]
        public string? ReviewComments { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// 关联业务对象ID (如：治疗方案ID、处方ID等)
        /// </summary>
        [Column("related_entity_id")]
        public long? RelatedEntityId { get; set; }

        /// <summary>
        /// 关联业务对象类型 (如：TreatmentPlan, Prescription等)
        /// </summary>
        [StringLength(50)]
        [Column("related_entity_type")]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// 风险描述 (如：使用毒性药材、配伍禁忌等)
        /// </summary>
        [StringLength(500)]
        [Column("risk_description")]
        public string? RiskDescription { get; set; }

        [StringLength(1000)]
        [Column("remarks")]
        public string? Remarks { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        [ForeignKey("ReviewerId")]
        public virtual User Reviewer { get; set; }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// 获取风险等级显示名称
        /// </summary>
        public string GetRiskLevelDisplayName()
        {
            if (!RiskScore.HasValue) return "无风险";

            return RiskScore.Value switch
            {
                <= 4 => "低风险",
                <= 6 => "中风险",
                <= 9 => "高风险",
                _ => "极高风险"
            };
        }

        /// <summary>
        /// 获取验证级别显示名称
        /// </summary>
        public string GetVerificationLevelDisplayName()
        {
            return VerificationLevel switch
            {
                "Level1" => "一级验证",
                "Level2" => "二级验证",
                "Level3" => "三级验证",
                "Level4" => "四级验证",
                _ => "无需验证"
            };
        }

        /// <summary>
        /// 判断是否为高风险操作
        /// </summary>
        public bool IsHighRiskOperation()
        {
            return RiskScore.HasValue && RiskScore.Value >= 7;
        }

        /// <summary>
        /// 判断是否需要人工审核
        /// </summary>
        public bool RequiresManualReview()
        {
            return VerificationLevel == "Level4" && ReviewStatus == "Pending";
        }
    }
}