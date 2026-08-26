using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 预测周明细列Dto
    /// </summary>
    public class FoWeeklyPeriodDto
    {
        /// <summary>
        /// 列标识
        /// </summary>
        public string PeriodKey { get; set; } = string.Empty;

        /// <summary>
        /// 周期类型
        /// </summary>
        public string PeriodType { get; set; } = string.Empty;

        /// <summary>
        /// 周期日期
        /// </summary>
        [JsonConverter(typeof(DateOnlyStringConverter))]
        public DateTime StartDate { get; set; }
    }
}
