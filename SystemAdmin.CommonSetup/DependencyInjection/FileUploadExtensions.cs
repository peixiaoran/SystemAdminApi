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
        /// <summary>绑定 FileUpload 配置，并把表单上传上限同步为 MaxSizeMB</summary>
        public static IServiceCollection AddFileUploadSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));

            services.AddOptions<FormOptions>()
                .Configure<IOptions<FileUploadOptions>>((form, upload) =>
                    form.MultipartBodyLengthLimit = upload.Value.MaxSizeMB * 1024L * 1024L);

            return services;
        }
    }
}
