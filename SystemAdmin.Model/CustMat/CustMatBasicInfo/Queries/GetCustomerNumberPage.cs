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
        /// 客户代码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 品名（中英文模糊匹配）
        /// </summary>
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态（1：启用，0：停用，不传则不筛选）
        /// </summary>
        public int? Status { get; set; }
    }
}
