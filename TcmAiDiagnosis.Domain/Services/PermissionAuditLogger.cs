using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限审计日志实现 - 记录权限验证活动
    /// </summary>
    public class PermissionAuditLogger : IPermissionAuditLogger
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly ILogger<PermissionAuditLogger> _logger;

        public PermissionAuditLogger(
            TcmAiDiagnosisContext context,
            ILogger<PermissionAuditLogger> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 记录权限验证失败日志
        /// </summary>
        public async Task LogPermissionDeniedAsync(
            int userId,
            int? tenantId,
            string requestPath,
            string[] requiredPermissions,
            string? ipAddress)
        {
            try
            {
                var log = new PermissionChangeLog
                {
                    ChangeType = "PermissionDenied",
                    TargetType = "User",
                    TargetId = userId,
                    TargetName = $"User_{userId}",
                    PermissionId = 0, // 权限拒绝时可能没有具体权限ID
                    PermissionCode = string.Join(", ", requiredPermissions),
                    PermissionName = "访问被拒绝",
                    ChangeReason = $"访问被拒绝 - 路径: {requestPath}",
                    OperatedBy = userId,
                    OperatedByName = $"User_{userId}",
                    OperatedAt = DateTime.UtcNow,
                    ClientIp = ipAddress ?? "Unknown",
                    TenantId = tenantId
                };

                _context.PermissionChangeLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogWarning(
                    "权限验证失败 - 用户ID: {UserId}, 租户ID: {TenantId}, 路径: {Path}, 所需权限: [{Permissions}], IP: {IpAddress}",
                    userId, tenantId, requestPath, string.Join(", ", requiredPermissions), ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "记录权限拒绝日志失败 - 用户ID: {UserId}, 路径: {Path}",
                    userId, requestPath);
            }
        }

        /// <summary>
        /// 记录临时权限使用日志
        /// </summary>
        public async Task LogTemporaryPermissionUsedAsync(
            int userId,
            int? tenantId,
            string permissionCode,
            long temporaryPermissionId)
        {
            try
            {
                var log = new PermissionChangeLog
                {
                    ChangeType = "TemporaryPermissionUsed",
                    TargetType = "User",
                    TargetId = userId,
                    TargetName = $"User_{userId}",
                    PermissionId = 0,
                    PermissionCode = permissionCode,
                    PermissionName = "临时权限使用",
                    ChangeReason = $"使用临时权限 - 临时权限ID: {temporaryPermissionId}",
                    OperatedBy = userId,
                    OperatedByName = $"User_{userId}",
                    OperatedAt = DateTime.UtcNow,
                    ClientIp = "System",
                    TenantId = tenantId
                };

                _context.PermissionChangeLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "临时权限使用 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: {Permission}, 临时权限ID: {TempPermissionId}",
                    userId, tenantId, permissionCode, temporaryPermissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "记录临时权限使用日志失败 - 用户ID: {UserId}, 权限: {Permission}",
                    userId, permissionCode);
            }
        }
    }
}
