using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Dto
{
    /// <summary>
    /// 表单步骤栏位权限Dto
    /// </summary>
    public class FormFieIdDto
    {
        /// <summary>
        /// 表单类别Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormTypeId { get; set; }

        /// <summary>
        /// 栏位Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FieldId { get; set; }

        /// <summary>
        /// 栏位名称（中文）
        /// </summary>
        public string FieldNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 栏位名称（英文）
        /// </summary>
        public string FieldNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示
        /// </summary>
        public int IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int IsEditable { get; set; }
    }
}
