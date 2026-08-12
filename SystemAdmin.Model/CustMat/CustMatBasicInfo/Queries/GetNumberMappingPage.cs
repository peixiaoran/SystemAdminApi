using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries
{
    /// <summary>
    /// 查询客户料号与公司料号对照分页请求参数
    /// </summary>
    public class GetNumberMappingPage : PageModel
    {
        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustomerPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 公司料号
        /// </summary>
        public string CompanyPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
    }
}
