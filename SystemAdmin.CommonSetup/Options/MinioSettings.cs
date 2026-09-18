namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>Minio 配置</summary>
    public class MinioSettings
    {
        /// <summary>服务地址</summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>AccessKey</summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>SecretKey</summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>默认 Bucket</summary>
        public string DefaultBucket { get; set; } = "systemadmin";

        /// <summary>是否使用 HTTPS</summary>
        public bool UseSSL { get; set; }
    }
}
