using System.Security.Claims;
using TcmAiDiagnosis.Domain.Services;
using TcmAiDiagnosis.Web.Attributes;

namespace TcmAiDiagnosis.Web.Middleware
{
    /// <summary>
    /// 权限验证中间件 - 拦截HTTP请求并验证权限
    /// </summary>
    public class PermissionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PermissionAuthorizationMiddleware> _logger;
        private readonly string[] _whitelistPaths;

        public PermissionAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<PermissionAuthorizationMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // 从配置读取白名单路径
            _whitelistPaths = configuration
                .GetSection("PermissionSettings:WhitelistPaths")
                .Get<string[]>() ?? Array.Empty<string>();
        }

        public async Task InvokeAsync(
            HttpContext context,
            IPermissionService permissionService,
            IPermissionAuditLogger auditLogger)
        {
            // 1. 检查是否在白名单中
            if (IsWhitelisted(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 2. 获取端点的权限要求
            var endpoint = context.GetEndpoint();
            var permissionAttribute = endpoint?.Metadata
                .GetMetadata<RequirePermissionAttribute>();

            if (permissionAttribute == null)
            {
                // 没有权限要求，继续处理
                await _next(context);
                return;
            }

            // 3. 检查用户是否已认证
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("未认证用户尝试访问受保护资源: {Path}", context.Request.Path);

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "用户未认证，请先登录",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.ToString()
                });
                return;
            }

            // 4. 获取用户ID和租户ID
            var userId = GetUserId(context.User);
            var tenantId = GetTenantId(context.User);

            if (userId == 0)
            {
                _logger.LogError("无法从用户声明中提取用户ID: {Path}", context.Request.Path);

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "用户身份无效",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.ToString()
                });
                return;
            }

            // 5. 验证权限
            var hasPermission = await permissionService.CheckPermissionsAsync(
                userId,
                tenantId,
                permissionAttribute.PermissionCodes,
                permissionAttribute.Mode,
                permissionAttribute.AllowTemporary
            );

            if (!hasPermission)
            {
                // 6. 记录审计日志
                await auditLogger.LogPermissionDeniedAsync(
                    userId,
                    tenantId,
                    context.Request.Path,
                    permissionAttribute.PermissionCodes,
                    context.Connection.RemoteIpAddress?.ToString()
                );

                // 7. 返回403
                _logger.LogWarning(
                    "权限不足 - 用户ID: {UserId}, 租户ID: {TenantId}, 路径: {Path}, 所需权限: [{Permissions}]",
                    userId, tenantId, context.Request.Path, string.Join(", ", permissionAttribute.PermissionCodes));

                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Forbidden",
                    message = "权限不足，无法访问此资源",
                    requiredPermissions = permissionAttribute.PermissionCodes,
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.ToString()
                });
                return;
            }

            // 8. 权限验证通过，继续处理
            _logger.LogDebug(
                "权限验证通过 - 用户ID: {UserId}, 路径: {Path}",
                userId, context.Request.Path);

            await _next(context);
        }

        /// <summary>
        /// 检查路径是否在白名单中
        /// </summary>
        private bool IsWhitelisted(PathString path)
        {
            if (_whitelistPaths == null || _whitelistPaths.Length == 0)
            {
                return false;
            }

            return _whitelistPaths.Any(w =>
                path.StartsWithSegments(w, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 从用户声明中提取用户ID
        /// </summary>
        private int GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            _logger.LogWarning("无法从用户声明中提取用户ID");
            return 0;
        }

        /// <summary>
        /// 从用户声明中提取租户ID
        /// </summary>
        private int? GetTenantId(ClaimsPrincipal user)
        {
            var tenantIdClaim = user.FindFirst("TenantId");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return tenantId;
            }

            return null;
        }
    }
}
