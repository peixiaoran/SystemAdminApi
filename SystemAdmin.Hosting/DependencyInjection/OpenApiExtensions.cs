using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>OpenAPI 注册扩展</summary>
    public static class OpenApiExtensions
    {
        /// <summary>注册 OpenAPI 文档（/openapi/{documentName}.json），并挂载 JWT 与 Accept-Language 转换器</summary>
        public static IServiceCollection AddOpenApiSetup(this IServiceCollection services, string documentName = "v1")
        {
            services.AddOpenApi(documentName, options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;

                options.AddDocumentTransformer((document, _, _) =>
                {
                    document.Info ??= new OpenApiInfo();
                    document.Info.Title = "SystemAdmin";
                    document.Info.Version = documentName;
                    document.Info.Description = "SystemAdmin API Documentation";
                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer<OpenApiTransformer>();
            });

            return services;
        }
    }
}
