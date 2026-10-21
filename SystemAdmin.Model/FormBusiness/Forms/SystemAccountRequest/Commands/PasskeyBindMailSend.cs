namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Commands
{
    /// <summary>
    /// 发送 Passkey 绑定邮件请求参数
    /// </summary>
    public class PasskeyBindMailSend
    {
        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 接收绑定链接的邮箱
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
