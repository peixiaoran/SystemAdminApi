namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>CORS 配置</summary>
    public sealed class CorsOptions
    {
        /// <summary>策略名称</summary>
        public string PolicyName { get; set; } = "DefaultCors";

        /// <summary>允许的前端 Origin</summary>
        public string[] Origins { get; set; } = Array.Empty<string>();

        /// <summary>允许的 HTTP 方法，为空表示全部</summary>
        public string[] Methods { get; set; } = { "POST", "OPTIONS" };

        /// <summary>允许的请求头，为空表示全部</summary>
        public string[] Headers { get; set; } = { "Content-Type", "Authorization", "X-Requested-With" };

        /// <summary>是否允许携带凭据（需显式指定 Origins）</summary>
        public bool AllowCredentials { get; set; } = true;
    }
}
