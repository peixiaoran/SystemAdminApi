using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormBasicInfo.Dto
{
    /// <summary>
    /// 表单类型栏位Dto
    /// </summary>
    public class FormTypeFieldDto
    {
        /// <summary>
        /// 栏位Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FieldId { get; set; }

        /// <summary>
        /// 栏位Key
        /// </summary>
        public string FieldKey { get; set; } = string.Empty;

        /// <summary>
        /// 栏位名称（中文）
        /// </summary>
        public string FieldNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 栏位名称（英文）
        /// </summary>
        public string FieldNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
