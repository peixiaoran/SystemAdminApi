using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Queries
{
    /// <summary>
    /// 查询加审用户请求参数
    /// </summary>
    public class GetAddReviewUserPage : PageModel
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
