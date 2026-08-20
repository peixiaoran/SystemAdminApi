using SqlSugar;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Queries
{
    public class GetSalesNumberPage : PageModel
    {
        /// <summary>
        /// 公司料号（模糊匹配）
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id（空字符串表示不筛选）
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人姓名（中英文模糊匹配）
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
