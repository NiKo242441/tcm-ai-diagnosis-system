namespace TcmAiDiagnosis.Entities.Enums
{
    /// <summary>
    /// 权限检查模式枚举
    /// </summary>
    public enum PermissionCheckMode
    {
        /// <summary>
        /// 需要所有权限（AND逻辑）
        /// </summary>
        RequireAll = 1,

        /// <summary>
        /// 需要任一权限（OR逻辑）
        /// </summary>
        RequireAny = 2
    }
}
