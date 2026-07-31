using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto
{
    /// <summary>
    /// 表单加审人Dto
    /// </summary>
    public class FormAddReviewDto
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormId { get; set; }

        /// <summary>
        /// 加审人部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 加审人Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long UserId { get; set; }

        /// <summary>
        /// 加审人工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 加审人姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 加审顺序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
