namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 租户表
    /// </summary>
    public class Tenant
    {
        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// 租户名称 (例如: 某某中医馆, 某某诊所)
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public string ContactPerson { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 租户状态 1正常 0欠费 -1禁用
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 租户过期时间
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // 导航属性
        public ICollection<User> Users { get; set; }
        public ICollection<PatientTenant> PatientTenants { get; set;}
        public ICollection<Renewal> Renewals{ get; set; }
    }
}