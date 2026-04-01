namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限缓存服务接口 - 提供权限数据缓存功能
    /// </summary>
    public interface IPermissionCacheService
    {
        /// <summary>
        /// 从缓存获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存的数据，如果不存在则返回默认值</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 设置缓存数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">要缓存的数据</param>
        /// <param name="expiration">过期时间，如果为null则使用默认值</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// 移除缓存数据
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 根据模式移除缓存数据
        /// </summary>
        /// <param name="pattern">缓存键模式</param>
        Task RemoveByPatternAsync(string pattern);
    }
}
