using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// Cloudflare Turnstile 注册扩展
    /// </summary>
    public static class TurnstileExtensions
    {
        /// <summary>
        /// 注册 Turnstile 配置与校验服务
        /// </summary>
        public static IServiceCollection AddTurnstileSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TurnstileOptions>(configuration.GetSection("Turnstile"));
            services.AddHttpClient<TurnstileService>();

            return services;
        }
    }
}
