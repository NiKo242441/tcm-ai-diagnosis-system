using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 风险操作日志数据传输对象
    /// </summary>
    public class RiskOperationLogDto
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        public int OperatorId { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        [StringLength(100, ErrorMessage = "操作人姓名长度不能超过100个字符")]
        public string OperatorName { get; set; } = string.Empty;

        /// <summary>
        /// 风险操作类型
        /// </summary>
        [Required(ErrorMessage = "风险操作类型不能为空")]
        [StringLength(100, ErrorMessage = "风险操作类型长度不能超过100个字符")]
        public string RiskOperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作对象ID
        /// </summary>
        [Required(ErrorMessage = "操作对象ID不能为空")]
        public int TargetId { get; set; }

        /// <summary>
        /// 操作对象类型
        /// </summary>
        [Required(ErrorMessage = "操作对象类型不能为空")]
        [StringLength(100, ErrorMessage = "操作对象类型长度不能超过100个字符")]
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// 操作内容描述
        /// </summary>
        [StringLength(2000, ErrorMessage = "操作内容描述长度不能超过2000个字符")]
        public string OperationDescription { get; set; } = string.Empty;

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [StringLength(50, ErrorMessage = "IP地址长度不能超过50个字符")]
        public string IpAddress { get; set; } = string.Empty;

        /// <summary>
        /// 验证信息
        /// </summary>
        [StringLength(500, ErrorMessage = "验证信息长度不能超过500个字符")]
        public string VerificationInfo { get; set; } = string.Empty;

        /// <summary>
        /// 操作结果
        /// </summary>
        [StringLength(50, ErrorMessage = "操作结果长度不能超过50个字符")]
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }
    }
}