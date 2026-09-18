using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>Nginx 转发请求头注册扩展</summary>
    public static class ForwardedHeadersExtensions
    {
        /// <summary>注册转发请求头，还原 Scheme / Host / 客户端 IP；只信任本机单层代理</summary>
        public static IServiceCollection AddForwardedHeadersSetup(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;

                options.KnownProxies.Add(IPAddress.Loopback);
                options.KnownProxies.Add(IPAddress.IPv6Loopback);
                options.ForwardLimit = 1;
            });

            return services;
        }
    }
}
