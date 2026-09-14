using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Queries
{
    /// <summary>
    /// 查询料号趋势下公司料号分页请求参数
    /// </summary>
    public class GetSalesNumberPage : PageModel
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务人员Id
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;
    }
}
