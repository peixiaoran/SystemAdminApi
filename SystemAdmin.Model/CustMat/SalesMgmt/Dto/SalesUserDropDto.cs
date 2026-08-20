using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 业务人员下拉Dto
    /// </summary>
    public class SalesUserDropDto
    {
        /// <summary>
        /// 业务人员Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long SalesUserId { get; set; }

        /// <summary>
        /// 业务人员姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
