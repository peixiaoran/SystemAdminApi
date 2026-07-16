using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>
    /// OpenAPI 注册扩展
    /// </summary>
    public static class OpenApiExtensions
    {
        /// <summary>
        /// 注册自定义 OpenAPI 配置
        /// </summary>
        /// <param name="services"></param>
        /// <param name="documentName">文档名称（对应 /openapi/{documentName}.json）</param>
        public static IServiceCollection AddCustomOpenApiAction(this IServiceCollection services, string documentName = "v1")
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddOpenApi(documentName, options =>
            {
                // 指定 OpenAPI 规范版本
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

                // 文档元信息
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    ConfigureDocumentInfo(document, documentName);
                    return Task.CompletedTask;
                });

                // 注册自定义文档转换器（需实现 IOpenApiDocumentTransformer）
                options.AddDocumentTransformer<OpenApiTransformer>();
            });

            return services;
        }

        /// <summary>
        /// 配置 OpenAPI 文档元信息
        /// </summary>
        private static void ConfigureDocumentInfo(OpenApiDocument document, string documentName)
        {
            document.Info ??= new OpenApiInfo();
            document.Info.Title = "SystemAdmin";
            document.Info.Version = documentName;
            document.Info.Description = "SystemAdmin API Documentation";
        }
    }
}
