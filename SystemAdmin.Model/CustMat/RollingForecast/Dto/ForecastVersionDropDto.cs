using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 预测版本下拉Dto
    /// </summary>
    public class ForecastVersionDropDto
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
    }
}
