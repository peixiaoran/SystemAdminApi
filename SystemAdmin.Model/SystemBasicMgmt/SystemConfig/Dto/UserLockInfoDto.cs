namespace SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Dto
{
    /// <summary>
    /// 用户账号锁定信息Dto
    /// </summary>
    public class UserLockDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 密码错误次数
        /// </summary>
        public int NumberErrors { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedDate { get; set; } = string.Empty;
    }
}
