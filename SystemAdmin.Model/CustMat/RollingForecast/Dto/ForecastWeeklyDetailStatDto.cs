using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 公司料号在单个预测版本下的用量统计（用于前端echarts展示）
    /// </summary>
    public class ForecastWeeklyDetailStatDto
    {
        /// <summary>
        /// 版本Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long VersionId { get; set; }

        /// <summary>
        /// 版本编号
        /// </summary>
        public string VersionCode { get; set; } = string.Empty;

        /// <summary>
        /// 按天预测数量合计
        /// </summary>
        public decimal DayQty { get; set; }

        /// <summary>
        /// 按周预测数量合计
        /// </summary>
        public decimal WeekQty { get; set; }

        /// <summary>
        /// 天+周预测数量合计
        /// </summary>
        public decimal TotalQty { get; set; }
    }
}
