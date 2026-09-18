using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>缓存注册扩展</summary>
    public static class CacheExtensions
    {
        /// <summary>注册 HybridCache，默认过期 5 分钟</summary>
        public static IServiceCollection AddCacheSetup(this IServiceCollection services)
        {
            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(5)
                };
            });

            return services;
        }
    }
}
