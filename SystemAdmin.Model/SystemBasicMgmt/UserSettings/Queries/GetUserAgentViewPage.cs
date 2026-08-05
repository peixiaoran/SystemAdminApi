using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries
{
    /// <summary>
    /// 查询用户代理人选择视图请求参数
    /// </summary>
    public class GetUserAgentViewPage : PageModel
    {
        /// <summary>
        /// 被代理用户Id
        /// </summary>
        public string SubstituteUserId { get; set; } = string.Empty;

        /// <summary>
        /// 部门Id
        /// </summary>
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 代理开始时间（按时间冲突筛掉不可选的代理人）
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 代理结束时间（按时间冲突筛掉不可选的代理人）
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
