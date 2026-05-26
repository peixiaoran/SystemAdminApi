using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries
{
    /// <summary>
    /// 查询用户分页请求参数
    /// </summary>
    public class GetUserAgentPage : PageModel
    {
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
    }
}
