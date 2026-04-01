namespace TcmAiDiagnosis.Web.Configuration
{
    /// <summary>
    /// 权限系统配置类
    /// </summary>
    public class PermissionSettings
    {
        /// <summary>
        /// 配置节名称
        /// </summary>
        public const string SectionName = "PermissionSettings";

        /// <summary>
        /// 是否启用权限检查
        /// 默认值：true
        /// </summary>
        public bool EnablePermissionCheck { get; set; } = true;

        /// <summary>
        /// 缓存过期时间（分钟）
        /// 默认值：30分钟
        /// </summary>
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// 白名单路径（无需权限验证的路径）
        /// </summary>
        public string[] WhitelistPaths { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 审计日志级别
        /// 可选值：Debug, Information, Warning, Error
        /// 默认值：Warning
        /// </summary>
        public string AuditLogLevel { get; set; } = "Warning";

        /// <summary>
        /// 是否启用详细错误信息（仅开发环境）
        /// 默认值：false
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;
    }
}
