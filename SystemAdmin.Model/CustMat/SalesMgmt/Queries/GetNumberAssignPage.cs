using SqlSugar;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Queries
{
    public class GetSalesNumberPage : PageModel
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
