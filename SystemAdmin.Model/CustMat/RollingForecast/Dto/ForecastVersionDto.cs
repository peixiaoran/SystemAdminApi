using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 预测版本Dto
    /// </summary>
    public class ForecastVersionDto
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
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 周次
        /// </summary>
        public int Week { get; set; }

        /// <summary>
        /// 是否最新
        /// </summary>
        public int IsLatest { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 状态名称
        /// </summary>
        public string StatusName { get; set; } = string.Empty;
    }
}
