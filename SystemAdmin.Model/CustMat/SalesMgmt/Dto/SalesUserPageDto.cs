using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 业务人员-用户分页查询Dto
    /// </summary>
    public class SalesUserPageDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long DepartmentId { get; set; }

        /// <summary>
        /// 所属部门名称
        /// </summary>
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 职级名称
        /// </summary>
        public string PositionName { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        public int Gender { get; set; }

        /// <summary>
        /// 是否在职
        /// </summary>
        public int IsEmployed { get; set; }

        /// <summary>
        /// 是否需要审批
        /// </summary>
        public int IsReview { get; set; }

        /// <summary>
        /// 是否冻结
        /// </summary>
        public int IsFreeze { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = string.Empty;
    }
}
