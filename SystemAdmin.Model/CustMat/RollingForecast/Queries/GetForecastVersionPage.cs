using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Queries
{
    /// <summary>
    /// 查询预测版本分页请求参数
    /// </summary>
    public class GetForecastVersionPage : PageModel
    {
        /// <summary>
        /// 版本编号
        /// </summary>
        public string VersionCode { get; set; } = string.Empty;
    }
}
