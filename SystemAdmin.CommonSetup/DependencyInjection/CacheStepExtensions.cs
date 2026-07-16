using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 缓存注册扩展
    /// </summary>
    public static class CacheStepExtensions
    {
        /// <summary>
        /// 注册 HybridCache（默认过期 5 分钟）
        /// </summary>
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            // 只注册 HybridCache（内部使用本地 MemoryCache）
            services.AddHybridCache(options =>
            {
                // 全局默认过期时间（可按需调整）
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5)
                };
            });

            return services;
        }
    }
}
