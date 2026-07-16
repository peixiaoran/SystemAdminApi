using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 邮件发送注册扩展
    /// </summary>
    public static class MailKitExtensions
    {
        /// <summary>
        /// 注册基于 MailKit 的邮件发送组件
        /// </summary>
        public static IServiceCollection AddMailKitSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection("EmailSettings"));
            services.PostConfigure<EmailOptions>(options =>
            {
                if (string.IsNullOrWhiteSpace(options.DisplayName))
                {
                    options.DisplayName = "Systems Admin";
                }

                if (options.Timeout <= 0)
                {
                    options.Timeout = 10000;
                }
            });

            services.AddTransient<MailKitEmailSender>();

            return services;
        }
    }
}
