using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// CORS 中间件扩展
    /// </summary>
    public static class CorsAppExtensions
    {
        private const string SectionName = "Cors";

        /// <summary>
        /// 启用配置的 CORS 策略
        /// </summary>
        public static IApplicationBuilder UseCorsSetup(this IApplicationBuilder app, IConfiguration configuration)
        {
            var corsOptions = configuration.GetSection(SectionName).Get<CorsOptions>() ?? new CorsOptions();
            return app.UseCors(corsOptions.PolicyName);
        }
    }
}
