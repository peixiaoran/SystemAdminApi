using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 请求语言注册扩展
    /// </summary>
    public static class LanguageExtensions
    {
        /// <summary>
        /// 注册请求语言（每个请求解析一次 Accept-Language）
        /// </summary>
        public static IServiceCollection AddLanguage(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            // 每个请求生成一个 RequestLanguage 实例
            services.AddScoped(sp =>
            {
                var accessor = sp.GetRequiredService<LanguageAccessor>();
                return accessor.GetRequestLanguage();
            });

            services.AddScoped<LanguageAccessor>();

            return services;
        }
    }
}
