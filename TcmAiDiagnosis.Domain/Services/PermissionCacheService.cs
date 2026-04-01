using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TcmAiDiagnosis.Domain.Services
{
    /// <summary>
    /// 权限缓存服务实现 - 使用IMemoryCache提供权限数据缓存
    /// </summary>
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PermissionCacheService> _logger;
        private readonly HashSet<string> _cacheKeys; // 用于跟踪所有缓存键，支持模式删除
        private readonly object _lockObject = new object();

        public PermissionCacheService(
            IMemoryCache memoryCache,
            ILogger<PermissionCacheService> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cacheKeys = new HashSet<string>();
        }

        /// <summary>
        /// 从缓存获取数据
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    _logger.LogDebug("缓存命中: {CacheKey}", key);
                    return value;
                }

                _logger.LogDebug("缓存未命中: {CacheKey}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "从缓存获取数据失败: {CacheKey}", key);
                return default;
            }
        }

        /// <summary>
        /// 设置缓存数据
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.Normal
                };

                // 添加缓存移除回调，用于从键集合中移除
                cacheOptions.RegisterPostEvictionCallback((k, v, r, s) =>
                {
                    lock (_lockObject)
                    {
                        _cacheKeys.Remove(k.ToString() ?? string.Empty);
                    }
                    _logger.LogDebug("缓存已移除: {CacheKey}, 原因: {Reason}", k, r);
                });

                _memoryCache.Set(key, value, cacheOptions);

                // 跟踪缓存键
                lock (_lockObject)
                {
                    _cacheKeys.Add(key);
                }

                _logger.LogDebug("缓存已设置: {CacheKey}, 过期时间: {Expiration}分钟",
                    key, (expiration ?? TimeSpan.FromMinutes(30)).TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置缓存数据失败: {CacheKey}", key);
            }
        }

        /// <summary>
        /// 移除缓存数据
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                _memoryCache.Remove(key);

                lock (_lockObject)
                {
                    _cacheKeys.Remove(key);
                }

                _logger.LogDebug("缓存已手动移除: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除缓存数据失败: {CacheKey}", key);
            }
        }

        /// <summary>
        /// 根据模式移除缓存数据
        /// </summary>
        public async Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            try
            {
                List<string> keysToRemove;

                lock (_lockObject)
                {
                    // 查找匹配模式的所有键
                    keysToRemove = _cacheKeys
                        .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // 移除匹配的缓存项
                foreach (var key in keysToRemove)
                {
                    await RemoveAsync(key);
                }

                _logger.LogInformation("根据模式移除缓存: {Pattern}, 移除数量: {Count}",
                    pattern, keysToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据模式移除缓存失败: {Pattern}", pattern);
            }
        }
    }
}
