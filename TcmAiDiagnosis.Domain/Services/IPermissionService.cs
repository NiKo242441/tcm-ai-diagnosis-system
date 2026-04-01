using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限服务接口 - 提供权限验证和查询功能
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 检查用户是否具有指定权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="permissionCode">权限代码</param>
        /// <param name="allowTemporary">是否允许临时权限</param>
        /// <returns>是否具有权限</returns>
        Task<bool> CheckPermissionAsync(
            int userId,
            int? tenantId,
            string permissionCode,
            bool allowTemporary = true);

        /// <summary>
        /// 检查用户是否具有多个权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="permissionCodes">权限代码数组</param>
        /// <param name="mode">权限检查模式（RequireAll或RequireAny）</param>
        /// <param name="allowTemporary">是否允许临时权限</param>
        /// <returns>是否具有权限</returns>
        Task<bool> CheckPermissionsAsync(
            int userId,
            int? tenantId,
            string[] permissionCodes,
            PermissionCheckMode mode = PermissionCheckMode.RequireAll,
            bool allowTemporary = true);

        /// <summary>
        /// 获取用户的所有权限列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="includeTemporary">是否包含临时权限</param>
        /// <returns>权限代码列表</returns>
        Task<List<string>> GetUserPermissionsAsync(
            int userId,
            int? tenantId,
            bool includeTemporary = true);

        /// <summary>
        /// 获取角色的所有权限列表
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>权限代码列表</returns>
        Task<List<string>> GetRolePermissionsAsync(int roleId);

        /// <summary>
        /// 检查临时权限
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        /// <param name="permissionCode">权限代码</param>
        /// <returns>是否具有临时权限</returns>
        Task<bool> CheckTemporaryPermissionAsync(
            int userId,
            int? tenantId,
            string permissionCode);

        /// <summary>
        /// 刷新用户权限缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="tenantId">租户ID</param>
        Task RefreshUserPermissionCacheAsync(int userId, int? tenantId);

        /// <summary>
        /// 刷新角色权限缓存
        /// </summary>
        /// <param name="roleId">角色ID</param>
        Task RefreshRolePermissionCacheAsync(int roleId);
    }
}
