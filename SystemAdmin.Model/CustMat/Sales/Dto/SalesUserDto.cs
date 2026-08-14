using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.Sales.Dto
{
    /// <summary>
    /// 业务人员信息Dto
    /// </summary>
    public class SalesUserDto
    {
        /// <summary>
        /// 用户Id（主键）
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long SalesUserId { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long SalesDeptId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名（中文）
        /// </summary>
        public string UserNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名（英文）
        /// </summary>
        public string UserNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 业务类型
        /// </summary>
        public string SalesType { get; set; } = string.Empty;

        /// <summary>
        /// 业务类型名称
        /// </summary>
        public string SalesTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
