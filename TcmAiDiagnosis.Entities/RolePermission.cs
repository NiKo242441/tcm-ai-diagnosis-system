using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 角色权限关联表 - 实现RBAC权限模型
    /// </summary>
    [Table("role_permissions")]
    public class RolePermission
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [Key]
        [Column("role_id", Order = 0)]
        public int RoleId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [Key]
        [Column("permission_id", Order = 1)]
        public int PermissionId { get; set; }

        /// <summary>
        /// 是否允许（true-允许，false-拒绝，用于权限覆盖场景）
        /// </summary>
        [Column("is_granted")]
        public bool IsGranted { get; set; } = true;

        /// <summary>
        /// 分配时间
        /// </summary>
        [Column("granted_at")]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 分配人ID
        /// </summary>
        [Column("granted_by")]
        public int? GrantedBy { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        [Column("remarks")]
        public string? Remarks { get; set; }

        // 导航属性
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("GrantedBy")]
        public virtual User? GrantedByUser { get; set; }
    }
}
