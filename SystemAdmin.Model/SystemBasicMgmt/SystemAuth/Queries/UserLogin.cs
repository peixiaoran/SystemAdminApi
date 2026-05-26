namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Queries
{
    /// <summary>
    /// 用户登录请求参数
    /// </summary>
    public class UserLogin
    {
        /// <summary>
        /// 登录账号
        /// </summary>
        public string LoginNo { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; } = string.Empty;
    }
}
