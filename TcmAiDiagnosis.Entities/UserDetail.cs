namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 患者详细信息表 (一对一与User表关联，UserId也是主键)
    /// </summary>
    public class UserDetail
    {
        /// <summary>
        /// 患者用户ID (外键, 主键)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        public string? IdCard { get; set; }

        /// <summary>
        /// 家庭住址
        /// </summary>
        public string? HomeAddress { get; set; }

        /// <summary>
        /// 紧急联系人姓名
        /// </summary>
        public string? EmergencyContactName { get; set; }

        /// <summary>
        /// 紧急联系人电话
        /// </summary>
        public string? EmergencyContactPhone { get; set; }

        /// <summary>
        /// 血型
        /// </summary>
        public string? BloodType { get; set; }

        /// <summary>
        /// 慢性病史 (文本描述)
        /// </summary>
        public string? PastMedicalHistory { get; set; }

        /// <summary>
        /// 传染病史 (文本描述)
        /// </summary>
        public string? InfectiousDiseaseHistory { get; set; }

        /// <summary>
        /// 过敏史 (文本描述)
        /// </summary>
        public string? AllergyHistory { get; set; }

        /// <summary>
        /// 家族病史 (文本描述)
        /// </summary>
        public string? FamilyMedicalHistory { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // 导航属性
        public User User { get; set; }
    }
}