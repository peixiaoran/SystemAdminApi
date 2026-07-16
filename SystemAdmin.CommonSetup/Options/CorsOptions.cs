namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// CORS（跨域资源共享）配置选项
    /// </summary>
    public sealed class CorsOptions
    {
        /// <summary>
        /// CORS 策略名称
        /// </summary>
        public string PolicyName { get; set; } = "DefaultCors";

        /// <summary>
        /// 允许跨域的前端 Origin 列表
        /// </summary>
        public string[] Origins { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 允许的 HTTP 方法
        /// </summary>
        public string[] Methods { get; set; } = { "POST", "OPTIONS" };

        /// <summary>
        /// 是否允许任意请求头
        /// </summary>
        public bool AllowAnyHeader { get; set; } = true;

        /// <summary>
        /// 是否允许携带凭据（启用时 Origins 必须显式指定）
        /// </summary>
        public bool AllowCredentials { get; set; } = true;
    }
}
