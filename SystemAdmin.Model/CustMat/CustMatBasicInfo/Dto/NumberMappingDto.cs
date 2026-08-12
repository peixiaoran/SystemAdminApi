using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto
{
    /// <summary>
    /// 客户料号与公司料号对照Dto
    /// </summary>
    public class NumberMappingDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long MappingId { get; set; }

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustomerPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 公司料号
        /// </summary>
        public string CompanyPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
    }
}
