namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>应用地址配置</summary>
    public class AppUrlOptions
    {
        /// <summary>前端域名</summary>
        public string BaseDomain { get; set; } = string.Empty;

        /// <summary>登录页地址</summary>
        public string LoginUrl { get; set; } = string.Empty;
    }
}
