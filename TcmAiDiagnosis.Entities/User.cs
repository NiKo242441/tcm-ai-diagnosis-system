using Microsoft.AspNetCore.Identity;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    public class User : IdentityUser<int>
    {
        /// <summary>
        /// 关联的租户ID。对于患者角色，此字段应为NULL。对于其他角色，此字段不得为NULL
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// 用户全名
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 用户性别（男/女/未知）
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// 用户账户状态（0活跃/1禁用）
        /// </summary>
        public int IsDisabled { get; set; }

        /// <summary>
        /// 记录创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 记录最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // 导航属性
        public Tenant Tenant { get; set; }
        public UserDetail Detail { get; set; }
        
        // 角色关联
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
        
        // 医生患者关联（医生视角）
        public virtual ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
        // 医生患者关联（患者视角）
        public virtual ICollection<DoctorPatient> PatientDoctors { get; set; } = new List<DoctorPatient>();

        // 患者租户关联
        public virtual ICollection<PatientTenant> PatientTenants { get; set; } = new List<PatientTenant>();
    }
}
