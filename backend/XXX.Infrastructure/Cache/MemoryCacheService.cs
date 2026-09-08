using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace XXX.Infrastructure.Cache
{
    /// <summary>
    /// 内存缓存服务实现
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _keys;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="memoryCache">内存缓存实例</param>
        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _keys = [];
        }

        /// <summary>
        /// 获取缓存值
        /// </summary>
        public Task<T?> GetAsync<T>(string key)
        {
            _memoryCache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        /// <summary>
        /// 设置缓存值
        /// </summary>
        public Task SetAsync<T>(string key, T value, int expirationMinutes = 30)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(expirationMinutes));

            _memoryCache.Set(key, value, options);
            _keys.Add(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 设置缓存值（带滑动过期）
        /// </summary>
        public Task SetAsync<T>(string key, T value, int absoluteExpirationMinutes, int slidingExpirationMinutes)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(absoluteExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpirationMinutes));

            _memoryCache.Set(key, value, options);
            _keys.Add(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            _keys.Remove(key);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 根据前缀移除缓存
        /// </summary>
        public Task RemoveByPrefixAsync(string prefix)
        {
            var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.Remove(key);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        /// <summary>
        /// 获取或设置缓存
        /// </summary>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 30)
        {
            if (_memoryCache.TryGetValue(key, out T? value) && value != null)
            {
                return value;
            }

            value = await factory();
            await SetAsync(key, value, expirationMinutes);
            return value;
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public Task ClearAsync()
        {
            foreach (var key in _keys.ToList())
            {
                _memoryCache.Remove(key);
            }
            _keys.Clear();
            return Task.CompletedTask;
        }
    }
}
