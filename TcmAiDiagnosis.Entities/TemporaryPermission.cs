using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 临时权限表 - 用于会诊、跨租户转诊等特殊场景的临时权限授予
    /// </summary>
    [Table("temporary_permissions")]
    public class TemporaryPermission
    {
        /// <summary>
        /// 临时权限ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("temp_permission_id")]
        public long TempPermissionId { get; set; }

        /// <summary>
        /// 场景类型（Consultation-会诊, Referral-转诊, Maintenance-系统维护）
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("scenario_type")]
        public string ScenarioType { get; set; } = string.Empty;

        /// <summary>
        /// 用户ID（被授权用户）
        /// </summary>
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [Required]
        [Column("permission_id")]
        public int PermissionId { get; set; }

        /// <summary>
        /// 资源类型（Patient-患者, MedicalRecord-病历, Treatment-治疗方案等）
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("resource_type")]
        public string ResourceType { get; set; } = string.Empty;

        /// <summary>
        /// 资源ID（具体资源的ID）
        /// </summary>
        [Required]
        [Column("resource_id")]
        public int ResourceId { get; set; }

        /// <summary>
        /// 授权租户ID（资源所属租户）
        /// </summary>
        [Required]
        [Column("source_tenant_id")]
        public int SourceTenantId { get; set; }

        /// <summary>
        /// 目标租户ID（被授权用户所属租户，跨租户场景）
        /// </summary>
        [Column("target_tenant_id")]
        public int? TargetTenantId { get; set; }

        /// <summary>
        /// 授权原因
        /// </summary>
        [Required]
        [StringLength(500)]
        [Column("grant_reason")]
        public string GrantReason { get; set; } = string.Empty;

        /// <summary>
        /// 授权人ID
        /// </summary>
        [Required]
        [Column("granted_by")]
        public int GrantedBy { get; set; }

        /// <summary>
        /// 授权时间
        /// </summary>
        [Required]
        [Column("granted_at")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 有效期开始时间
        /// </summary>
        [Required]
        [Column("valid_from")]
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 有效期结束时间
        /// </summary>
        [Required]
        [Column("valid_to")]
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// 是否需要患者授权
        /// </summary>
        [Column("requires_patient_consent")]
        public bool RequiresPatientConsent { get; set; } = false;

        /// <summary>
        /// 患者授权状态（Pending-待授权, Approved-已授权, Rejected-已拒绝）
        /// </summary>
        [StringLength(20)]
        [Column("patient_consent_status")]
        public string? PatientConsentStatus { get; set; }

        /// <summary>
        /// 患者授权时间
        /// </summary>
        [Column("patient_consent_at")]
        public DateTime? PatientConsentAt { get; set; }

        /// <summary>
        /// 是否需要租户管理员批准
        /// </summary>
        [Column("requires_admin_approval")]
        public bool RequiresAdminApproval { get; set; } = false;

        /// <summary>
        /// 管理员批准状态（Pending-待批准, Approved-已批准, Rejected-已拒绝）
        /// </summary>
        [StringLength(20)]
        [Column("admin_approval_status")]
        public string? AdminApprovalStatus { get; set; }

        /// <summary>
        /// 管理员批准人ID
        /// </summary>
        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        /// <summary>
        /// 管理员批准时间
        /// </summary>
        [Column("approved_at")]
        public DateTime? ApprovedAt { get; set; }

        /// <summary>
        /// 批准意见
        /// </summary>
        [StringLength(500)]
        [Column("approval_comments")]
        public string? ApprovalComments { get; set; }

        /// <summary>
        /// 权限状态（Active-生效中, Expired-已过期, Revoked-已撤销）
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Active";

        /// <summary>
        /// 撤销人ID
        /// </summary>
        [Column("revoked_by")]
        public int? RevokedBy { get; set; }

        /// <summary>
        /// 撤销时间
        /// </summary>
        [Column("revoked_at")]
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// 撤销原因
        /// </summary>
        [StringLength(500)]
        [Column("revoke_reason")]
        public string? RevokeReason { get; set; }

        /// <summary>
        /// 是否自动回收（到期自动回收）
        /// </summary>
        [Column("auto_revoke")]
        public bool AutoRevoke { get; set; } = true;

        /// <summary>
        /// 访问次数限制（0表示不限制）
        /// </summary>
        [Column("access_limit")]
        public int AccessLimit { get; set; } = 0;

        /// <summary>
        /// 已访问次数
        /// </summary>
        [Column("access_count")]
        public int AccessCount { get; set; } = 0;

        /// <summary>
        /// 最后访问时间
        /// </summary>
        [Column("last_accessed_at")]
        public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(1000)]
        [Column("remarks")]
        public string? Remarks { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("GrantedBy")]
        public virtual User GrantedByUser { get; set; } = null!;

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }

        [ForeignKey("RevokedBy")]
        public virtual User? RevokedByUser { get; set; }

        [ForeignKey("SourceTenantId")]
        public virtual Tenant SourceTenant { get; set; } = null!;

        [ForeignKey("TargetTenantId")]
        public virtual Tenant? TargetTenant { get; set; }

        // 辅助方法
        /// <summary>
        /// 判断权限是否有效
        /// </summary>
        public bool IsValid()
        {
            if (Status != "Active") return false;
            
            var now = DateTime.UtcNow;
            if (now < ValidFrom || now > ValidTo) return false;

            if (RequiresPatientConsent && PatientConsentStatus != "Approved") return false;
            if (RequiresAdminApproval && AdminApprovalStatus != "Approved") return false;

            if (AccessLimit > 0 && AccessCount >= AccessLimit) return false;

            return true;
        }

        /// <summary>
        /// 记录访问
        /// </summary>
        public void RecordAccess()
        {
            AccessCount++;
            LastAccessedAt = DateTime.UtcNow;
        }
    }
}
