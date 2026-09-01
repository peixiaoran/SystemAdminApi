using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 人员料号信息Dto
    /// </summary>
    public class SalesNumberDto
    {
        /// <summary>
        /// 公司料号（主键）
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long SalesUserId { get; set; }

        /// <summary>
        /// 业务负责人工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
