using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>
    /// Scalar API 文档界面注册扩展
    /// </summary>
    public static class ScalarExtensions
    {
        private const string SectionName = "Scalar";

        /// <summary>
        /// 从配置绑定 ScalarOptions（MapScalarApiReference 内部通过 IOptionsSnapshot 读取）
        /// </summary>
        public static IServiceCollection AddScalarSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScalarOptions>(configuration.GetSection(SectionName));
            return services;
        }
    }
}
