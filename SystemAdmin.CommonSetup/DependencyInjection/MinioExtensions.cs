using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>Minio 注册扩展</summary>
    public static class MinioExtensions
    {
        /// <summary>绑定 Minio 配置，注册 IMinioClient 与 MinioService 单例</summary>
        public static IServiceCollection AddMinioSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MinioSettings>(configuration.GetSection("Minio"));

            services.AddSingleton<IMinioClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;

                var client = new MinioClient()
                    .WithEndpoint(settings.Endpoint)
                    .WithCredentials(settings.AccessKey, settings.SecretKey);

                if (settings.UseSSL)
                    client = client.WithSSL();

                return client.Build();
            });

            services.AddSingleton<MinioService>();
            return services;
        }
    }
}
