using TcmAiDiagnosis.Entities.Enums;

namespace TcmAiDiagnosis.Web.Attributes
{
    /// <summary>
    /// 权限验证特性 - 用于标注控制器或方法的权限要求
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute
    {
        /// <summary>
        /// 所需权限代码列表
        /// </summary>
        public string[] PermissionCodes { get; }

        /// <summary>
        /// 权限验证模式：RequireAll（需要所有权限）或 RequireAny（需要任一权限）
        /// 默认值：RequireAll
        /// </summary>
        public PermissionCheckMode Mode { get; set; } = PermissionCheckMode.RequireAll;

        /// <summary>
        /// 是否允许临时权限
        /// 默认值：true
        /// </summary>
        public bool AllowTemporary { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="permissionCodes">所需权限代码列表</param>
        /// <exception cref="ArgumentNullException">权限代码列表为空时抛出</exception>
        public RequirePermissionAttribute(params string[] permissionCodes)
        {
            if (permissionCodes == null || permissionCodes.Length == 0)
            {
                throw new ArgumentNullException(nameof(permissionCodes), "权限代码列表不能为空");
            }

            PermissionCodes = permissionCodes;
        }
    }
}
