using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    public static class PdfConverterSetupExtensions
    {
        public static IServiceCollection AddPdfConverterSetup(
            this IServiceCollection services, IConfiguration configuration)
        {
            // 绑定配置节
            services.Configure<PdfConverterOptions>(
                configuration.GetSection("PdfConverter"));

            // 转换器持有长期存活的 Chrome 进程，注册 Singleton
            services.AddSingleton<HtmlToPdfConverter, HtmlToPdfConverter>();

            return services;
        }
    }
}
