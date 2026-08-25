using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 预测周明细Dto
    /// </summary>
    public class FoWeeklyDetailDto
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
        /// 版本开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 列定义
        /// </summary>
        public List<FoWeeklyPeriodDto> Periods { get; set; } = [];

        /// <summary>
        /// 数据行
        /// </summary>
        public List<FoWeeklyRowDto> Rows { get; set; } = [];
    }
}
