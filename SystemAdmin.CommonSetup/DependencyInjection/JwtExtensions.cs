using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>JWT 认证注册扩展</summary>
    public static class JwtExtensions
    {
        /// <summary>注册 JWT 配置、Token 服务、当前用户，并配置 JwtBearer（Header 优先，其次 Cookie）</summary>
        public static IServiceCollection AddJwtSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddSingleton<JwtTokenService>();
            services.AddScoped<CurrentUser>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

            // 验签参数直接复用 JwtTokenService，避免在注册阶段重复解析密钥
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<JwtTokenService>((options, tokenService) =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = false;
                    options.TokenValidationParameters = tokenService.ValidationParameters;
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = ExtractToken(context.Request, tokenService.CookieName);
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
            return services;
        }

        private static string? ExtractToken(HttpRequest request, string cookieName)
        {
            var authHeader = request.Headers.Authorization.ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                if (token.Length > 0) return token;
            }

            return request.Cookies.TryGetValue(cookieName, out var cookieToken) && !string.IsNullOrWhiteSpace(cookieToken)
                ? cookieToken
                : null;
        }
    }
}
