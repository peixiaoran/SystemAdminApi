using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace SystemAdmin.Hosting.DependencyInjection
{
    /// <summary>Serilog 日志注册扩展</summary>
    public static class SerilogExtensions
    {
        /// <summary>注册 Serilog：Logs 目录下按日期分文件夹，文件夹内再按级别（info/warning/error）分文件</summary>
        public static IHostBuilder AddSerilogSetup(this IHostBuilder host)
        {
            host.UseSerilog((context, logger) =>
            {
                var logRoot = Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs");

                logger
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Map(_ => DateTime.Now.ToString("yyyyMMdd"), (dateFolder, wt) =>
                    {
                        var dayRoot = Path.Combine(logRoot, dateFolder);
                        wt.LevelFile(dayRoot, "info", e => e.Level == LogEventLevel.Information);
                        wt.LevelFile(dayRoot, "warning", e => e.Level == LogEventLevel.Warning);
                        wt.LevelFile(dayRoot, "error", e => e.Level >= LogEventLevel.Error);
                    });
            });

            return host;
        }

        private static void LevelFile(
            this LoggerSinkConfiguration sink,
            string dayRoot,
            string name,
            Func<LogEvent, bool> filter)
        {
            sink.Logger(lc => lc
                .Filter.ByIncludingOnly(filter)
                .WriteTo.File(Path.Combine(dayRoot, $"{name}.log")));
        }
    }
}
