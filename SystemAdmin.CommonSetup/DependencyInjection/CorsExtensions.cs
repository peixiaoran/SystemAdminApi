using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>CORS 注册与中间件扩展</summary>
    public static class CorsExtensions
    {
        private const string SectionName = "Cors";

        /// <summary>按 Cors 配置注册跨域策略</summary>
        public static IServiceCollection AddCorsSetup(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection(SectionName);
            services.Configure<CorsOptions>(section);

            var corsOptions = section.Get<CorsOptions>() ?? new CorsOptions();

            services.AddCors(options =>
            {
                options.AddPolicy(corsOptions.PolicyName, policy =>
                {
                    if (corsOptions.Origins is { Length: > 0 })
                        policy.WithOrigins(corsOptions.Origins);

                    if (corsOptions.AllowAnyHeader)
                        policy.AllowAnyHeader();

                    if (corsOptions.Methods is { Length: > 0 })
                        policy.WithMethods(corsOptions.Methods);
                    else
                        policy.AllowAnyMethod();

                    if (corsOptions.AllowCredentials)
                        policy.AllowCredentials();
                });
            });

            return services;
        }

        /// <summary>启用已注册的跨域策略</summary>
        public static IApplicationBuilder UseCorsSetup(this IApplicationBuilder app)
        {
            var policyName = app.ApplicationServices.GetRequiredService<IOptions<CorsOptions>>().Value.PolicyName;
            return app.UseCors(policyName);
        }
    }
}
