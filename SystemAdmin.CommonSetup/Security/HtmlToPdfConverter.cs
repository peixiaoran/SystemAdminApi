using Microsoft.Extensions.Options;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using SystemAdmin.CommonSetup.Options;

namespace SystemAdmin.CommonSetup.Security
{
    public class HtmlToPdfConverter : IAsyncDisposable
    {
        private readonly PdfConverterOptions _options;
        private IBrowser? _sharedBrowser;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        public HtmlToPdfConverter(IOptions<PdfConverterOptions> options)
        {
            _options = options.Value;
        }

        private async Task<IBrowser> GetBrowserAsync()
        {
            if (_sharedBrowser is not null) return _sharedBrowser;

            await _initLock.WaitAsync();
            try
            {
                if (_sharedBrowser is not null) return _sharedBrowser;

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    Args = new[]
                    {
                        "--no-sandbox",
                        "--disable-setuid-sandbox",
                        "--disable-gpu",
                        "--no-first-run",
                        "--no-default-browser-check",
                        "--hide-scrollbars",
                        "--mute-audio",
                        // Chromium 新版 headless 在 Windows 上存在偶发弹出空白窗口的已知缺陷，
                        // 把窗口移出可视区域兜底，保证任何版本下都不会在桌面上出现白框
                        "--window-position=-32000,-32000"
                    }
                };

                // 配了路径就用现成的 Chrome，否则自动下载 Chromium
                if (!string.IsNullOrWhiteSpace(_options.ExecutablePath))
                {
                    launchOptions.ExecutablePath = _options.ExecutablePath;
                }
                else
                {
                    await new BrowserFetcher().DownloadAsync();
                }

                _sharedBrowser = await Puppeteer.LaunchAsync(launchOptions);
                return _sharedBrowser;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<byte[]> ConvertAsync(string htmlContent)
        {
            var browser = await GetBrowserAsync();
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent, new SetContentOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            return await page.PdfDataAsync(new PdfOptions
            {
                Format = ParsePaperFormat(_options.PaperFormat),
                PrintBackground = _options.PrintBackground,
                MarginOptions = new MarginOptions
                {
                    Top = _options.MarginTop,
                    Bottom = _options.MarginBottom,
                    Left = _options.MarginLeft,
                    Right = _options.MarginRight
                }
            });
        }

        private static PaperFormat ParsePaperFormat(string formatName)
        {
            return formatName?.ToUpperInvariant() switch
            {
                "A3" => PaperFormat.A3,
                "A4" => PaperFormat.A4,
                "A5" => PaperFormat.A5,
                "LETTER" => PaperFormat.Letter,
                "LEGAL" => PaperFormat.Legal,
                _ => PaperFormat.A4
            };
        }

        public async ValueTask DisposeAsync()
        {
            if (_sharedBrowser is not null)
                await _sharedBrowser.DisposeAsync();
        }
    }
}
