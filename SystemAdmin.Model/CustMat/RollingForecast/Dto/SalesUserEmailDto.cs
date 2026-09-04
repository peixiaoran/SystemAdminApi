namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 业务人员邮件通知信息Dto
    /// </summary>
    public class SalesUserEmailDto
    {
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 邮件通知语言
        /// </summary>
        public string NoticeLanguage { get; set; } = string.Empty;
    }
}
