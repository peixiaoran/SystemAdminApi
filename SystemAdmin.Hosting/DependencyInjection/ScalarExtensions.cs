using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>Scalar 文档界面注册扩展</summary>
    public static class ScalarExtensions
    {
        /// <summary>绑定 Scalar 配置，MapScalarApiReference 会通过 Options 读取</summary>
        public static IServiceCollection AddScalarSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScalarOptions>(configuration.GetSection("Scalar"));
            return services;
        }
    }
}
