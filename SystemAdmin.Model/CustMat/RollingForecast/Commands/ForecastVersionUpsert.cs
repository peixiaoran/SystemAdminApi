namespace SystemAdmin.Model.CustMat.RollingForecast.Commands
{
    /// <summary>
    /// 预测版本新增/修改类
    /// </summary>
    public class ForecastVersionUpsert
    {
        /// <summary>
        /// 版本Id
        /// </summary>
        public string VersionId { get; set; } = string.Empty;

        /// <summary>
        /// 版本编号
        /// </summary>
        public string VersionCode { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
