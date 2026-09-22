using Scalar.AspNetCore;
using SystemAdmin.CommonSetup.DependencyInjection;
using SystemAdmin.Hosting.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 主机及基础服务
builder.Host.AddSerilogSetup();                             // 日志
builder.Services.AddControllers();                          // 控制器
builder.Services.AddHttpContextAccessor();                  // HttpContext 访问器
builder.Services.AddCorsSetup(builder.Configuration);       // 跨域
builder.Services.AddOpenApiSetup();                         // OpenAPI 文档
builder.Services.AddJwtSetup(builder.Configuration);        // JWT 认证
builder.Services.AddTurnstileSetup(builder.Configuration);  // Cloudflare Turnstile 人机验证
builder.Services.AddMinioSetup(builder.Configuration);      // MinIO 对象存储
builder.Services.AddFileUploadSetup(builder.Configuration); // 文件上传
builder.Services.AddAppUrlSetup(builder.Configuration);     // 前端地址
builder.Services.AddMailKitSetup(builder.Configuration);    // 邮件
builder.Services.AddLocalizationSetup();                    // 本地化
builder.Services.AddSqlSugarSetup(builder.Configuration);   // SqlSugar
builder.Services.AddProjectClasses();                       // 业务服务与仓储
builder.Services.AddCacheSetup();                           // HybridCache
builder.Services.AddForwardedHeadersSetup();                // Nginx 转发请求头
builder.Services.AddScalarSetup(builder.Configuration);     // Scalar 界面

// 配置 Kestrel
builder.WebHost.ConfigureKestrel((context, options) =>
{
    context.Configuration
        .GetSection("Kestrel")
        .Bind(options);

    // 项目不再限制上传文件大小，取消请求体上限（未显式配置 Kestrel:Limits:MaxRequestBodySize 时生效）
    if (context.Configuration.GetValue<long?>("Kestrel:Limits:MaxRequestBodySize") is null)
    {
        options.Limits.MaxRequestBodySize = null;
    }
});

var app = builder.Build();

// HTTP 请求管道，还原 Scheme / Host，必须放在最前面
app.UseForwardedHeaders();

// 仅开发环境开放 OpenAPI 与 Scalar 界面
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference("/systemadminapi");
    app.MapOpenApi();
}

/*
 * Nginx 已经负责公网 HTTPS。
 * 正确配置 UseForwardedHeaders 后，可以保留。
 * 如果服务器上仍出现循环重定向，可以先注释掉，
 * 因为 Kestrel 只允许本机访问，外部无法绕过 Nginx。
 */
// app.UseHttpsRedirection();

app.UseRouting();
app.UseCorsSetup();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
