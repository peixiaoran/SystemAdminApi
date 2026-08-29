namespace SystemAdmin.Model.CustMat.RollingForecast.Queries
{
    /// <summary>
    /// 按公司料号统计多个预测版本用量请求参数
    /// </summary>
    public class GetNumberTrend
    {
        /// <summary>
        /// 公司料号（单个）
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 预测版本Id集合
        /// </summary>
        public List<string> VersionIds { get; set; } = [];
    }
}
