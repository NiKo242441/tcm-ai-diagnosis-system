using System.ComponentModel.DataAnnotations;

namespace TcmAiDiagnosis.Dtos
{
    /// <summary>
    /// 审计日志详情数据传输对象
    /// </summary>
    public class AuditLogDetailDto
    {
        /// <summary>
        /// 审计日志ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(100, ErrorMessage = "用户名长度不能超过100个字符")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型
        /// </summary>
        [Required(ErrorMessage = "操作类型不能为空")]
        [StringLength(100, ErrorMessage = "操作类型长度不能超过100个字符")]
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// 操作模块
        /// </summary>
        [Required(ErrorMessage = "操作模块不能为空")]
        [StringLength(100, ErrorMessage = "操作模块长度不能超过100个字符")]
        public string Module { get; set; } = string.Empty;

        /// <summary>
        /// 操作内容
        /// </summary>
        [StringLength(2000, ErrorMessage = "操作内容长度不能超过2000个字符")]
        public string OperationContent { get; set; } = string.Empty;

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
        /// 浏览器信息
        /// </summary>
        [StringLength(500, ErrorMessage = "浏览器信息长度不能超过500个字符")]
        public string BrowserInfo { get; set; } = string.Empty;

        /// <summary>
        /// 操作结果
        /// </summary>
        [StringLength(50, ErrorMessage = "操作结果长度不能超过50个字符")]
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        [StringLength(2000, ErrorMessage = "错误信息长度不能超过2000个字符")]
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 租户ID
        /// </summary>
        public int TenantId { get; set; }
    }
}