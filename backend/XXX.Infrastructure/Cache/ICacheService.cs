namespace XXX.Infrastructure.Cache
{
    /// <summary>
    /// 缓存服务接口
    /// 提供统一的缓存操作接口
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值，不存在返回默认值</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// 设置缓存值
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expirationMinutes">过期时间（分钟），默认30分钟</param>
        Task SetAsync<T>(string key, T value, int expirationMinutes = 30);

        /// <summary>
        /// 设置缓存值（带滑动过期）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="absoluteExpirationMinutes">绝对过期时间（分钟）</param>
        /// <param name="slidingExpirationMinutes">滑动过期时间（分钟）</param>
        Task SetAsync<T>(string key, T value, int absoluteExpirationMinutes, int slidingExpirationMinutes);

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// 根据前缀移除缓存
        /// </summary>
        /// <param name="prefix">缓存键前缀</param>
        Task RemoveByPrefixAsync(string prefix);

        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 获取或设置缓存
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="factory">数据获取工厂</param>
        /// <param name="expirationMinutes">过期时间（分钟）</param>
        /// <returns>缓存值</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 30);

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        Task ClearAsync();
    }
}
