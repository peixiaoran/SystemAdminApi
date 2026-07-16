using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 多语言注册扩展
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// 注册多语言消息服务
        /// </summary>
        public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
        {
            // 多语言消息服务
            services.AddSingleton<LocalizationService>();

            return services;
        }
    }
}
