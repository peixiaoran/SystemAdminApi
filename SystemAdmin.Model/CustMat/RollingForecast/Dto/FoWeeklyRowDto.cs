using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

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
        /// 业务负责人Id（仅查询所有业务人员明细时填充）
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? SalesUserId { get; set; }

        /// <summary>
        /// 业务负责人姓名（仅查询所有业务人员明细时填充）
        /// </summary>
        public string? SalesUserName { get; set; }

        /// <summary>
        /// 各周期预测数量，键为列标识
        /// </summary>
        public Dictionary<string, decimal> Quantities { get; set; } = [];

        /// <summary>
        /// 上周FO版本合计
        /// </summary>
        public decimal PreviousTotal { get; set; }

        /// <summary>
        /// 本周FO版本合计
        /// </summary>
        public decimal CurrentTotal { get; set; }

        /// <summary>
        /// 本周合计减去上周合计的差异
        /// </summary>
        public decimal TotalDiff { get; set; }

        /// <summary>
        /// 环比上周变化百分比（%，保留2位小数），上周合计为0时为空
        /// </summary>
        public decimal? TotalDiffRate { get; set; }
    }
}
