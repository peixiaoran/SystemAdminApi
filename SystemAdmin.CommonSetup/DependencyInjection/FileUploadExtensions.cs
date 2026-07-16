using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// 文件上传注册扩展
    /// </summary>
    public static class FileUploadExtensions
    {
        /// <summary>
        /// 注册文件上传配置，并同步表单上传大小限制
        /// </summary>
        public static IServiceCollection AddFileUploadSetup(this IServiceCollection services, IConfiguration configuration)
        {
            // 绑定配置
            services.Configure<FileUploadOptions>(configuration.GetSection("FileUpload"));

            // 同步设置表单上传大小限制
            var fileUploadSection = configuration.GetSection("FileUpload");
            var maxSizeMB = fileUploadSection.GetValue<int>("MaxSizeMB");
            var maxBytes = maxSizeMB * 1024L * 1024L;

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = maxBytes;
            });

            return services;
        }
    }
}
