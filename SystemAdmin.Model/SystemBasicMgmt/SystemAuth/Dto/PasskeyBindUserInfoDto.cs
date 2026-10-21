namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto
{
    /// <summary>
    /// 绑定链接对应的用户信息Dto
    /// </summary>
    public class PasskeyBindUserInfoDto
    {
        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名（中文）
        /// </summary>
        public string UserNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名（英文）
        /// </summary>
        public string UserNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 链接发送到的邮箱（已脱敏）
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 链接过期时间
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 该用户已绑定的 Passkey 数量
        /// </summary>
        public int PasskeyCount { get; set; }
    }
}
