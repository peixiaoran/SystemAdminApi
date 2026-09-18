using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>Cloudflare Turnstile 注册扩展</summary>
    public static class TurnstileExtensions
    {
        /// <summary>绑定 Turnstile 配置并注册校验服务（Typed HttpClient）</summary>
        public static IServiceCollection AddTurnstileSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TurnstileOptions>(configuration.GetSection("Turnstile"));
            services.AddHttpClient<TurnstileService>();
            return services;
        }
    }
}
