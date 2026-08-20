using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 客户信息下拉Dto
    /// </summary>
    public class CustomerDropDto
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long CustomerId { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
    }
}
