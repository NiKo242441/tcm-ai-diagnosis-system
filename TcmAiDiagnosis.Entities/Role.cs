using Microsoft.AspNetCore.Identity;

namespace TcmAiDiagnosis.Entities
{
    /// <summary>
    /// 角色字典表
    /// </summary>
    public class Role : IdentityRole<int>
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string ShowName { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public ICollection<User> Users { get; set; }

        /// <summary>
        /// 角色权限关联
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
