using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TcmAiDiagnosis.EFContext;
using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限服务实现 - 提供权限验证和查询功能
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly TcmAiDiagnosisContext _context;
        private readonly IPermissionCacheService _cacheService;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            TcmAiDiagnosisContext context,
            IPermissionCacheService cacheService,
            ILogger<PermissionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 检查用户是否具有指定权限
        /// </summary>
        public async Task<bool> CheckPermissionAsync(
            int userId,
            int? tenantId,
            string permissionCode,
            bool allowTemporary = true)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                throw new ArgumentNullException(nameof(permissionCode));
            }

            try
            {
                var userPermissions = await GetUserPermissionsAsync(userId, tenantId, allowTemporary);
                var hasPermission = userPermissions.Contains(permissionCode);

                _logger.LogDebug(
                    "权限检查 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: {Permission}, 结果: {Result}",
                    userId, tenantId, permissionCode, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "权限检查失败 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: {Permission}",
                    userId, tenantId, permissionCode);
                return false;
            }
        }

        /// <summary>
        /// 检查用户是否具有多个权限
        /// </summary>
        public async Task<bool> CheckPermissionsAsync(
            int userId,
            int? tenantId,
            string[] permissionCodes,
            PermissionCheckMode mode = PermissionCheckMode.RequireAll,
            bool allowTemporary = true)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
            {
                throw new ArgumentNullException(nameof(permissionCodes));
            }

            try
            {
                var userPermissions = await GetUserPermissionsAsync(userId, tenantId, allowTemporary);

                bool hasPermission;
                if (mode == PermissionCheckMode.RequireAll)
                {
                    // 需要所有权限
                    hasPermission = permissionCodes.All(code => userPermissions.Contains(code));
                }
                else
                {
                    // 需要任一权限
                    hasPermission = permissionCodes.Any(code => userPermissions.Contains(code));
                }

                _logger.LogDebug(
                    "权限检查 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: [{Permissions}], 模式: {Mode}, 结果: {Result}",
                    userId, tenantId, string.Join(", ", permissionCodes), mode, hasPermission);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "权限检查失败 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: [{Permissions}]",
                    userId, tenantId, string.Join(", ", permissionCodes));
                return false;
            }
        }

        /// <summary>
        /// 获取用户的所有权限列表
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(
            int userId,
            int? tenantId,
            bool includeTemporary = true)
        {
            try
            {
                // 1. 尝试从缓存获取
                var cacheKey = $"user_permissions:{userId}:{tenantId}";
                var cached = await _cacheService.GetAsync<List<string>>(cacheKey);
                if (cached != null && cached.Count > 0)
                {
                    _logger.LogDebug("从缓存获取用户权限 - 用户ID: {UserId}, 权限数量: {Count}",
                        userId, cached.Count);
                    return cached;
                }

                // 2. 从数据库查询
                var permissions = new List<string>();

                // 2.1 获取用户的角色权限
                var rolePermissions = await _context.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Roles)
                    .SelectMany(r => r.RolePermissions)
                    .Where(rp => rp.IsGranted && rp.Permission.IsActive)
                    .Select(rp => rp.Permission.PermissionCode)
                    .Distinct()
                    .ToListAsync();

                permissions.AddRange(rolePermissions);

                _logger.LogDebug("用户角色权限 - 用户ID: {UserId}, 权限数量: {Count}",
                    userId, rolePermissions.Count);

                // 2.2 获取临时权限（如果允许）
                if (includeTemporary)
                {
                    var now = DateTime.UtcNow;
                    var tempPermissions = await _context.TemporaryPermissions
                        .Where(tp =>
                            tp.UserId == userId &&
                            (tenantId == null || tp.SourceTenantId == tenantId || tp.TargetTenantId == tenantId) &&
                            tp.Status == "Active" &&
                            tp.ValidFrom <= now &&
                            tp.ValidTo >= now)
                        .Join(_context.Permissions,
                            tp => tp.PermissionId,
                            p => p.PermissionId,
                            (tp, p) => p.PermissionCode)
                        .Distinct()
                        .ToListAsync();

                    permissions.AddRange(tempPermissions);

                    _logger.LogDebug("用户临时权限 - 用户ID: {UserId}, 权限数量: {Count}",
                        userId, tempPermissions.Count);
                }

                // 3. 缓存结果
                await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(30));

                _logger.LogInformation("获取用户权限 - 用户ID: {UserId}, 租户ID: {TenantId}, 总权限数: {Count}",
                    userId, tenantId, permissions.Count);

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户权限失败 - 用户ID: {UserId}, 租户ID: {TenantId}",
                    userId, tenantId);
                return new List<string>();
            }
        }

        /// <summary>
        /// 获取角色的所有权限列表
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            try
            {
                // 1. 尝试从缓存获取
                var cacheKey = $"role_permissions:{roleId}";
                var cached = await _cacheService.GetAsync<List<string>>(cacheKey);
                if (cached != null && cached.Count > 0)
                {
                    _logger.LogDebug("从缓存获取角色权限 - 角色ID: {RoleId}, 权限数量: {Count}",
                        roleId, cached.Count);
                    return cached;
                }

                // 2. 从数据库查询
                var permissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId && rp.IsGranted && rp.Permission.IsActive)
                    .Select(rp => rp.Permission.PermissionCode)
                    .Distinct()
                    .ToListAsync();

                // 3. 缓存结果（角色权限缓存时间更长）
                await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromHours(1));

                _logger.LogInformation("获取角色权限 - 角色ID: {RoleId}, 权限数量: {Count}",
                    roleId, permissions.Count);

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取角色权限失败 - 角色ID: {RoleId}", roleId);
                return new List<string>();
            }
        }

        /// <summary>
        /// 检查临时权限
        /// </summary>
        public async Task<bool> CheckTemporaryPermissionAsync(
            int userId,
            int? tenantId,
            string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
            {
                throw new ArgumentNullException(nameof(permissionCode));
            }

            try
            {
                var now = DateTime.UtcNow;

                var hasTemporaryPermission = await _context.TemporaryPermissions
                    .AnyAsync(tp =>
                        tp.UserId == userId &&
                        (tenantId == null || tp.SourceTenantId == tenantId || tp.TargetTenantId == tenantId) &&
                        tp.Permission.PermissionCode == permissionCode &&
                        tp.Status == "Active" &&
                        tp.ValidFrom <= now &&
                        tp.ValidTo >= now);

                _logger.LogDebug(
                    "临时权限检查 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: {Permission}, 结果: {Result}",
                    userId, tenantId, permissionCode, hasTemporaryPermission);

                return hasTemporaryPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "临时权限检查失败 - 用户ID: {UserId}, 租户ID: {TenantId}, 权限: {Permission}",
                    userId, tenantId, permissionCode);
                return false;
            }
        }

        /// <summary>
        /// 刷新用户权限缓存
        /// </summary>
        public async Task RefreshUserPermissionCacheAsync(int userId, int? tenantId)
        {
            try
            {
                var cacheKey = $"user_permissions:{userId}:{tenantId}";
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogInformation("刷新用户权限缓存 - 用户ID: {UserId}, 租户ID: {TenantId}",
                    userId, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新用户权限缓存失败 - 用户ID: {UserId}, 租户ID: {TenantId}",
                    userId, tenantId);
            }
        }

        /// <summary>
        /// 刷新角色权限缓存
        /// </summary>
        public async Task RefreshRolePermissionCacheAsync(int roleId)
        {
            try
            {
                var cacheKey = $"role_permissions:{roleId}";
                await _cacheService.RemoveAsync(cacheKey);

                _logger.LogInformation("刷新角色权限缓存 - 角色ID: {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新角色权限缓存失败 - 角色ID: {RoleId}", roleId);
            }
        }
    }
}
