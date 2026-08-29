using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Queries
{
    /// <summary>
    /// 查询本人负责的公司料号分页请求参数
    /// </summary>
    public class GetSalesNumberPage : PageModel
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;
    }
}
