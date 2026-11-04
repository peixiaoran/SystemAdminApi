using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>邮件队列注册扩展</summary>
    public static class EmailQueueExtensions
    {
        /// <summary>注册邮件待发送队列</summary>
        public static IServiceCollection AddEmailQueueSetup(this IServiceCollection services)
        {
            services.AddSingleton<EmailQueue>();
            return services;
        }
    }
}
