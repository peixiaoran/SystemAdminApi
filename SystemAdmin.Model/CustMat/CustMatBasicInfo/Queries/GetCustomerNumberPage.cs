using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries
{
    public class GetCustomerNumberPage : PageModel
    {
        /// <summary>
        /// 客户料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态（1：启用，0：停用，不传则不筛选）
        /// </summary>
        public int? Status { get; set; }
    }
}
