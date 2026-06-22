using System;
using System.Collections.Generic;
using System.Text;

namespace SystemAdmin.CommonSetup.Options
{
    public class PdfConverterOptions
    {
        /// <summary>纸张格式，对应 PaperFormat 名称，如 "A4"、"Letter"</summary>
        public string PaperFormat { get; set; } = "A4";

        public bool PrintBackground { get; set; } = true;

        public string MarginTop { get; set; } = "20mm";
        public string MarginBottom { get; set; } = "20mm";
        public string MarginLeft { get; set; } = "15mm";
        public string MarginRight { get; set; } = "15mm";

        /// <summary>
        /// 指定 Chromium 可执行档路径。内网/容器环境若已自带 Chrome，
        /// 填这里就跳过 BrowserFetcher 下载；留空则首次运行自动下载。
        /// </summary>
        public string? ExecutablePath { get; set; }
    }
}
