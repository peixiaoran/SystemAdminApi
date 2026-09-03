using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 业务负责范围客户分布占比Dto
    /// </summary>
    public class SalesCustomerDistributionDto
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long CustomerId { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 负责的客户料号数量（去重）
        /// </summary>
        public int CustomerPartNumberCount { get; set; }

        /// <summary>
        /// 占比（%，保留2位小数）
        /// </summary>
        public decimal Percentage { get; set; }
    }
}
