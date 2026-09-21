using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>文件上传注册扩展</summary>
    public static class FileUploadExtensions
    {
        /// <summary>绑定 FileUpload 配置（仅用于扩展名白名单校验，不再限制文件大小）</summary>
        public static IServiceCollection AddFileUploadSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));

            services.AddOptions<FormOptions>()
                .Configure(form => form.MultipartBodyLengthLimit = long.MaxValue);

            return services;
        }
    }
}
