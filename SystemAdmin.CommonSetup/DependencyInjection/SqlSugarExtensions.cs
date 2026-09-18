using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace SystemAdmin.CommonSetup.DependencyInjection
{
    /// <summary>SqlSugar 注册扩展</summary>
    public static class SqlSugarExtensions
    {
        /// <summary>注册 SqlSugarScope 单例，并设置雪花 Id 与 SQL 超时</summary>
        public static IServiceCollection AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SystemAdminDb");
            var commandTimeout = configuration.GetValue<int?>("ConnectionStrings:CommandTimeout") ?? 60;

            SnowFlakeSingle.WorkId = configuration.GetValue<short?>("SnowFlake:WorkId") ?? 1;

            var sqlSugar = new SqlSugarScope(
                new ConnectionConfig
                {
                    ConnectionString = connectionString,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                },
                db => db.Ado.CommandTimeOut = commandTimeout);

            services.AddSingleton(sqlSugar);
            services.AddSingleton<ISqlSugarClient>(sqlSugar);
            return services;
        }
    }
}
