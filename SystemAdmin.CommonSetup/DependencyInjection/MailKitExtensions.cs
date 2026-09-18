using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>邮件发送注册扩展</summary>
    public static class MailKitExtensions
    {
        /// <summary>绑定 EmailSettings 配置并注册 MailKit 发送服务</summary>
        public static IServiceCollection AddMailKitSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection("EmailSettings"));
            services.AddSingleton<MailKitEmailSender>();
            return services;
        }
    }
}
