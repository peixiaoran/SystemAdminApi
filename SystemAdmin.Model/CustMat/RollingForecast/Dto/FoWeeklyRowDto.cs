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

        /// <summary>
        /// 天数量环比上周变化百分比（%，保留2位小数），上周数量为0时为空
        /// </summary>
        public decimal? DayQtyChangeRate { get; set; }

        /// <summary>
        /// 周数量环比上周变化百分比（%，保留2位小数），上周数量为0时为空
        /// </summary>
        public decimal? WeekQtyChangeRate { get; set; }
    }
}
