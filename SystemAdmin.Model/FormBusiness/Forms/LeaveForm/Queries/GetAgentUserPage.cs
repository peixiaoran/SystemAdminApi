using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Queries
{
    /// <summary>
    /// 查询请假单代理人选择视图请求参数
    /// </summary>
    public class GetAgentUserPage : PageModel
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

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
