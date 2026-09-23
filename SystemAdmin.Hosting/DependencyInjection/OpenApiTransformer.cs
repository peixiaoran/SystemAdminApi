using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>OpenAPI 文档转换器：添加 JWT Bearer 安全方案、Accept-Language 与 X-Requested-With 公共参数</summary>
    public sealed class OpenApiTransformer(IAuthenticationSchemeProvider schemeProvider) : IOpenApiDocumentTransformer
    {
        private const string SchemeKey = "Bearer";

        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var schemes = await schemeProvider.GetAllSchemesAsync();
            if (schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
                AddJwtSecurity(document);

            AddAcceptLanguageParameter(document);
            AddCsrfHeaderParameter(document);
        }

        private static void AddJwtSecurity(OpenApiDocument document)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeKey] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme"
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(SchemeKey, document)] = new List<string>()
            });
        }

        private static void AddAcceptLanguageParameter(OpenApiDocument document)
        {
            var parameter = new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Description = "语言设置：zh-CN（简体中文）、en-US（英文）",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = "zh-CN",
                    Enum = new List<JsonNode>
                    {
                        JsonValue.Create("zh-CN")!,
                        JsonValue.Create("en-US")!
                    }
                }
            };

            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations?.Values ?? Enumerable.Empty<OpenApiOperation>())
                {
                    operation.Parameters ??= new List<IOpenApiParameter>();
                    operation.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 防 CSRF 自定义头：JwtAuthorize 对非 GET/HEAD 请求强制校验 X-Requested-With: XMLHttpRequest，缺失即 403
        /// </summary>
        private static void AddCsrfHeaderParameter(OpenApiDocument document)
        {
            var parameter = new OpenApiParameter
            {
                Name = "X-Requested-With",
                In = ParameterLocation.Header,
                Description = "防 CSRF 请求头，非 GET/HEAD 请求必传，固定值 XMLHttpRequest",
                Required = true,
                Example = JsonValue.Create("XMLHttpRequest"),
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = "XMLHttpRequest",
                    Enum = new List<JsonNode>
                    {
                        JsonValue.Create("XMLHttpRequest")!
                    }
                }
            };

            foreach (var path in document.Paths.Values)
            {
                foreach (var (method, operation) in path.Operations ?? new Dictionary<HttpMethod, OpenApiOperation>())
                {
                    if (method == HttpMethod.Get || method == HttpMethod.Head)
                        continue;

                    operation.Parameters ??= new List<IOpenApiParameter>();
                    operation.Parameters.Add(parameter);
                }
            }
        }
    }
}
