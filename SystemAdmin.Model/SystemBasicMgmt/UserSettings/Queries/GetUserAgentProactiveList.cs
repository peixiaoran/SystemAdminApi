namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries
{
    /// <summary>
    /// 查询用户代理了哪些用户请求参数
    /// </summary>
    public class GetUserAgentProactiveList
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}
