namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 文件上传配置选项
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>
        /// 文件大小上限（MB）
        /// </summary>
        public int MaxSizeMB { get; set; }

        /// <summary>
        /// 允许的文件扩展名
        /// </summary>
        public string[] AllowExtensions { get; set; } = {
            ".xls", ".xlsx", ".csv",
            ".pdf", ".doc", ".docx", ".txt",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".zip", ".rar", ".7z"
        };
    }
}
