using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 应用地址注册扩展
    /// </summary>
    public static class AppUrlExtensions
    {
        /// <summary>
        /// 注册应用地址配置
        /// </summary>
        public static IServiceCollection AddAppUrlSetup(this IServiceCollection services, IConfiguration configuration)
        {
            // 绑定配置
            services.Configure<AppUrlOptions>(configuration.GetSection("AppUrl"));

            return services;
        }
    }
}
