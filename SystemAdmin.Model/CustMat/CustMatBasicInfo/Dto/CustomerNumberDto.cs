using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto
{
    /// <summary>
    /// 客户料号信息Dto
    /// </summary>
    public class CustomerNumberDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long PartNumberId { get; set; }

        /// <summary>
        /// 客户料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名（中文）
        /// </summary>
        public string NumberNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 品名（英文）
        /// </summary>
        public string NumberNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态
        /// </summary>
        public int Status { get; set; }
    }
}
