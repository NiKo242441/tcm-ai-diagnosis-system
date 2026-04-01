using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 权限表 - 定义系统中所有可分配的权限点
    /// </summary>
    [Table("permissions")]
    public class Permission
    {
        /// <summary>
        /// 权限ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("permission_id")]
        public int PermissionId { get; set; }

        /// <summary>
        /// 权限代码（唯一标识，如：patient.create, treatment.edit）
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
        /// 权限描述
        /// </summary>
        [StringLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 权限分类（如：患者管理、诊疗操作、治疗方案、病历管理等）
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("category")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 权限模块（如：PatientManagement, TreatmentPlan, MedicalRecord等）
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("module")]
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// 是否为系统权限（系统权限不可删除）
        /// </summary>
        [Column("is_system")]
        public bool IsSystem { get; set; } = true;

        /// <summary>
        /// 是否启用
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 排序号
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
