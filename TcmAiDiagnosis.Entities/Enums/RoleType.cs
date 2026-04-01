namespace TcmAiDiagnosis.Entities.Enums
{
    /// <summary>
    /// 角色类型枚举 - 定义平台四级角色体系
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// 平台超级管理员 - 最高权限角色，负责全平台管理
        /// </summary>
        PlatformSuperAdmin = 1,

        /// <summary>
        /// 租户管理员 - 租户内最高权限角色，负责租户管理
        /// </summary>
        TenantAdmin = 2,

        /// <summary>
        /// 中医师 - 核心业务角色，负责诊疗业务
        /// </summary>
        Doctor = 3,

        /// <summary>
        /// 患者 - 诊疗服务接受者
        /// </summary>
        Patient = 4
    }
}
