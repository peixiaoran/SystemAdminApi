namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 应用地址配置选项
    /// </summary>
    public class AppUrlOptions
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string BaseDomain { get; set; } = string.Empty;

        /// <summary>
        /// 登录URL
        /// </summary>
        public string LoginUrl { get; set; } = string.Empty;
    }
}
