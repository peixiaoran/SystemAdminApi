namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands
{
    /// <summary>
    /// 用户解锁修改类
    /// </summary>
    public class UserUnlock
    {
        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord = string.Empty;

        /// <summary>
        /// 验证码
        /// </summary>
        public string VerificationCode = string.Empty;
    }
}
