using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormBasicInfo.Queries
{
    /// <summary>
    /// 查询表单栏位分页请求参数
    /// </summary>
    public class GetFormTypeFieldPage : PageModel
    {
        /// <summary>
        /// 表单类别Id
        /// </summary>
        public string FormTypeId { get; set; } = string.Empty;
    }
}
