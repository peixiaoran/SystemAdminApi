using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>
    /// SqlSugar 注册扩展
    /// </summary>
    public static class SqlSugarExtensions
    {
        /// <summary>
        /// 注册 SqlSugarScope 单例（含雪花Id、SQL 超时配置）
        /// </summary>
        public static IServiceCollection SqlSugarScopeSetup(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. 先从 json / 配置系统读取默认值
            var conn = configuration.GetConnectionString("SystemAdminDb");
            var cmdTimeout = configuration.GetValue<int?>("ConnectionStrings:CommandTimeout") ?? 60;
            var workId = configuration.GetValue<short?>("SnowFlake:WorkId") ?? 1;

            // 2. SnowFlake 配置
            SnowFlakeSingle.WorkId = workId;

            // 3. 创建 SqlSugarScope
            var sqlSugar = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = conn,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                },
                cfg =>
                {
                    // 全局 SQL 执行超时
                    cfg.Ado.CommandTimeOut = cmdTimeout;
                });

            // 4. 注册 SqlSugarScope 单例（同时注册接口方便注入）
            services.AddSingleton<ISqlSugarClient>(sqlSugar);
            services.AddSingleton(sqlSugar);

            return services;
        }
    }
}
