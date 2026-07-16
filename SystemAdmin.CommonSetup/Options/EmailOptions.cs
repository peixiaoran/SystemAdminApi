namespace SystemAdmin.CommonSetup.Security
{
    /// <summary>
    /// 发送邮件配置选项
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// SMTP 服务器地址
        /// </summary>
        public string SmtpServer { get; set; } = default!;

        /// <summary>
        /// 发件账号
        /// </summary>
        public string UserName { get; set; } = default!;

        /// <summary>
        /// SMTP 授权码
        /// </summary>
        public string Password { get; set; } = default!;

        /// <summary>
        /// 发件人邮箱
        /// </summary>
        public string From { get; set; } = default!;

        /// <summary>
        /// 发件人显示名称
        /// </summary>
        public string DisplayName { get; set; } = "System Admin";

        /// <summary>
        /// 发送超时（毫秒）
        /// </summary>
        public int Timeout { get; set; } = 10000;
    }
}
