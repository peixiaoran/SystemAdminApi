using SqlSugar;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Queries
{
    public class GetSalesNumberPage : PageModel
    {
        /// <summary>
        /// 料号编码
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;
    }
}
