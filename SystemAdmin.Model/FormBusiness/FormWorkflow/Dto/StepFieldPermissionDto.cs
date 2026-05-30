using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Dto
{
    /// <summary>
    /// 表单步骤栏位权限Dto
    /// </summary>
    public class StepFieldPermissionDto
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long StepId { get; set; }

        /// <summary>
        /// 栏位Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FieldId { get; set; }

        /// <summary>
        /// 栏位名称
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示
        /// </summary>
        public int IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int IsDisabled { get; set; }
    }
}
