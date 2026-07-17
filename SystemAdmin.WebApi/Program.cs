using Scalar.AspNetCore;
using SystemAdmin.CommonSetup.DependencyInjection;
using SystemAdmin.Hosting.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 主机及基础服务
builder.Host.AddSerilogSetup();                             // 日志
builder.Services.AddControllers();                          // 控制器
builder.Services.AddHttpContextAccessor();                  // HttpContext 访问器
builder.Services.AddCorsSetup(builder.Configuration);       // 跨域
builder.Services.AddOpenApi();                              // OpenAPI
builder.Services.AddCustomOpenApiAction();                  // 自定义 OpenAPI 规范
builder.Services.AddJwtSetup(builder.Configuration);        // JWT 认证
builder.Services.AddMinioSetup(builder.Configuration);      // MinIO 对象存储
builder.Services.AddFileUploadSetup(builder.Configuration); // 文件上传
builder.Services.AddAppUrlSetup(builder.Configuration);     // 前端地址
builder.Services.AddMailKitSetup(builder.Configuration);    // 邮件
builder.Services.AddLocalizationSetup();                    // 本地化
builder.Services.AddLanguage();                             // 全局语言中间件
builder.Services.SqlSugarScopeSetup(builder.Configuration); // SqlSugar
builder.Services.AddProjectClasses();                       // 业务服务与仓储
builder.Services.AddCache();                                // HybridCache
builder.Services.AddForwardedHeadersSetup();                // Nginx 转发请求头
builder.Services.Configure<ScalarOptions>(builder.Configuration.GetSection("Scalar")); // Scalar 界面

// 配置 Kestrel
builder.WebHost.ConfigureKestrel((context, options) =>
{
    context.Configuration
        .GetSection("Kestrel")
        .Bind(options);
});

var app = builder.Build();

// HTTP 请求管道，还原 Scheme / Host，必须放在最前面
app.UseForwardedHeaders();

app.MapScalarApiReference("/systemadminapi");
app.MapOpenApi();

/*
 * Nginx 已经负责公网 HTTPS。
 * 正确配置 UseForwardedHeaders 后，可以保留。
 * 如果服务器上仍出现循环重定向，可以先注释掉，
 * 因为 Kestrel 只允许本机访问，外部无法绕过 Nginx。
 */
// app.UseHttpsRedirection();

app.UseRouting();
app.UseCorsSetup(builder.Configuration);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
