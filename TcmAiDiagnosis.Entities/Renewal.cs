namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 租户续费记录表
    /// </summary>
    public class Renewal
    {
        /// <summary>
        /// 续费记录ID
        /// </summary>
        public int RenewalId { get; set; }

        /// <summary>
        /// 关联的租户ID（外键）
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// 续费时间
        /// </summary>
        public DateTime RenewalDate { get; set; }

        /// <summary>
        /// 续费金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 有效期起始时间
        /// </summary>
        public DateTime ValidFrom { get; set; }

        /// <summary>
        /// 有效期结束时间
        /// </summary>
        public DateTime ValidTo { get; set; }

        // 导航属性
        public Tenant Tenant { get; set; }
    }
}