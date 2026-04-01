namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限审计日志接口 - 记录权限验证活动
    /// </summary>
    public interface IPermissionAuditLogger
    {
        /// <summary>
        /// 记录权限验证失败日志
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="requestPath">请求路径</param>
        /// <param name="requiredPermissions">所需权限列表</param>
        /// <param name="ipAddress">IP地址</param>
        Task LogPermissionDeniedAsync(
            int userId,
            int? tenantId,
            string requestPath,
            string[] requiredPermissions,
            string? ipAddress);

        /// <summary>
        /// 记录临时权限使用日志
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="permissionCode">权限代码</param>
        /// <param name="temporaryPermissionId">临时权限ID</param>
        Task LogTemporaryPermissionUsedAsync(
            int userId,
            int? tenantId,
            string permissionCode,
            long temporaryPermissionId);
    }
}
