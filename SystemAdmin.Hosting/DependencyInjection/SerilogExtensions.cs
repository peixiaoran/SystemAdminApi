using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>Serilog 日志注册扩展</summary>
    public static class SerilogExtensions
    {
        /// <summary>注册 Serilog：按级别分文件、按天滚动写入 Logs 目录</summary>
        public static IHostBuilder AddSerilogSetup(this IHostBuilder host)
        {
            host.UseSerilog((context, logger) =>
            {
                var logRoot = Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs");

                logger
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.LevelFile(logRoot, "info", e => e.Level == LogEventLevel.Information)
                    .WriteTo.LevelFile(logRoot, "warning", e => e.Level == LogEventLevel.Warning)
                    .WriteTo.LevelFile(logRoot, "error", e => e.Level >= LogEventLevel.Error);
            });

            return host;
        }

        private static LoggerConfiguration LevelFile(
            this LoggerSinkConfiguration sink,
            string logRoot,
            string name,
            Func<LogEvent, bool> filter)
        {
            return sink.Logger(lc => lc
                .Filter.ByIncludingOnly(filter)
                .WriteTo.File(Path.Combine(logRoot, $"{name}-.log"), rollingInterval: RollingInterval.Day));
        }
    }
}
