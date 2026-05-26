using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto
{
    /// <summary>
    /// 用户分页Dto
    /// </summary>
    public class UserFormDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary>
        /// 职级名称
        /// </summary>
        public string PositionName { get; set; } = string.Empty;

        /// <summary>
        /// 职业名称
        /// </summary>
        public string LaborName { get; set; } = string.Empty;

        /// <summary>
        /// 国籍名称
        /// </summary>
        public string NationalityName { get; set; } = string.Empty;

        /// <summary>
        /// 是否审批
        /// </summary>
        public int IsReview { get; set; }

        /// <summary>
        /// 是否审批描述
        /// </summary>
        public string IsReviewName { get; set; } = string.Empty;
    }
}
