namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 预测周明细行Dto
    /// </summary>
    public class FoWeeklyRowDto
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// 各周期预测数量，键为列标识
        /// </summary>
        public Dictionary<string, decimal> Quantities { get; set; } = [];
    }
}
