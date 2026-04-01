using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 权限变更日志表 - 记录所有权限分配、变更操作，确保全流程可追溯
    /// </summary>
    [Table("permission_change_logs")]
    public class PermissionChangeLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("log_id")]
        public long LogId { get; set; }

        /// <summary>
        /// 变更类型（Grant-授予, Revoke-撤销, Modify-修改）
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("change_type")]
        public string ChangeType { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型（Role-角色, User-用户）
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("target_type")]
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// 目标ID（角色ID或用户ID）
        /// </summary>
        [Required]
        [Column("target_id")]
        public int TargetId { get; set; }

        /// <summary>
        /// 目标名称（角色名或用户名）
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("target_name")]
        public string TargetName { get; set; } = string.Empty;

        /// <summary>
        /// 权限ID
        /// </summary>
        [Required]
        [Column("permission_id")]
        public int PermissionId { get; set; }

        /// <summary>
        /// 权限代码
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("permission_code")]
        public string PermissionCode { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("permission_name")]
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// 变更前状态（JSON格式）
        /// </summary>
        [Column("before_state", TypeName = "text")]
        public string? BeforeState { get; set; }

        /// <summary>
        /// 变更后状态（JSON格式）
        /// </summary>
        [Column("after_state", TypeName = "text")]
        public string? AfterState { get; set; }

        /// <summary>
        /// 变更原因
        /// </summary>
        [Required]
        [StringLength(500)]
        [Column("change_reason")]
        public string ChangeReason { get; set; } = string.Empty;

        /// <summary>
        /// 申请人ID
        /// </summary>
        [Column("applicant_id")]
        public int? ApplicantId { get; set; }

        /// <summary>
        /// 申请人姓名
        /// </summary>
        [StringLength(100)]
        [Column("applicant_name")]
        public string? ApplicantName { get; set; }

        /// <summary>
        /// 审批人ID
        /// </summary>
        [Column("approver_id")]
        public int? ApproverId { get; set; }

        /// <summary>
        /// 审批人姓名
        /// </summary>
        [StringLength(100)]
        [Column("approver_name")]
        public string? ApproverName { get; set; }

        /// <summary>
        /// 审批状态（Pending-待审批, Approved-已批准, Rejected-已拒绝）
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("approval_status")]
        public string ApprovalStatus { get; set; } = "Approved";

        /// <summary>
        /// 审批意见
        /// </summary>
        [StringLength(500)]
        [Column("approval_comments")]
        public string? ApprovalComments { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        [Required]
        [Column("operated_by")]
        public int OperatedBy { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        [Required]
        [StringLength(100)]
        [Column("operated_by_name")]
        public string OperatedByName { get; set; } = string.Empty;

        /// <summary>
        /// 操作时间
        /// </summary>
        [Required]
        [Column("operated_at")]
        public DateTime OperatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 客户端IP地址
        /// </summary>
        [Required]
        [StringLength(45)]
        [Column("client_ip")]
        public string ClientIp { get; set; } = string.Empty;

        /// <summary>
        /// 用户代理（浏览器信息）
        /// </summary>
        [StringLength(500)]
        [Column("user_agent")]
        public string? UserAgent { get; set; }

        /// <summary>
        /// 租户ID（跨租户权限变更时记录）
        /// </summary>
        [Column("tenant_id")]
        public int? TenantId { get; set; }

        /// <summary>
        /// 是否跨租户操作
        /// </summary>
        [Column("is_cross_tenant")]
        public bool IsCrossTenant { get; set; } = false;

        /// <summary>
        /// 有效期开始时间（临时权限）
        /// </summary>
        [Column("valid_from")]
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// 有效期结束时间（临时权限）
        /// </summary>
        [Column("valid_to")]
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000)]
        [Column("remarks")]
        public string? Remarks { get; set; }

        // 导航属性
        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("ApplicantId")]
        public virtual User? Applicant { get; set; }

        [ForeignKey("ApproverId")]
        public virtual User? Approver { get; set; }

        [ForeignKey("OperatedBy")]
        public virtual User Operator { get; set; } = null!;

        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
