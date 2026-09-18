using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>多语言注册扩展</summary>
    public static class LocalizationExtensions
    {
        /// <summary>注册请求语言（每个请求解析一次 Accept-Language）与多语言消息服务</summary>
        public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddScoped(sp =>
            {
                var header = sp.GetRequiredService<IHttpContextAccessor>().HttpContext?
                    .Request.Headers.AcceptLanguage.ToString();
                return Language.Parse(header);
            });

            services.AddScoped<LocalizationService>();
            return services;
        }
    }
}
