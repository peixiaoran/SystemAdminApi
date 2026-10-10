using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 料号分配信息Dto
    /// </summary>
    public class NumberAssignDto
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

        /// <summary>
        /// 客户编码，经客户料号对照表关联，一个公司料号可能对应多个客户时以"、"连接，无对照关系时为空
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;
    }
}
