using SqlSugar;

namespace SystemAdmin.Model.CustMat.Sales.Queries
{
    public class GetSalesUserPage : PageModel
    {
        /// <summary>
        /// 部门Id（"0"表示不筛选）
        /// </summary>
        public string SalesDeptId { get; set; } = "0";

        /// <summary>
        /// 业务类型
        /// </summary>
        public string SalesType { get; set; } = string.Empty;

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名（中英文模糊匹配）
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
