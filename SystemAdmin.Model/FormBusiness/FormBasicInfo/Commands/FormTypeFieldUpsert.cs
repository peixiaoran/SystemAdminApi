namespace SystemAdmin.Model.FormBusiness.FormBasicInfo.Commands
{
    /// <summary>
    /// 表单类别栏位新增/修改类
    /// </summary>
    public class FormTypeFieldUpsert
    {
        /// <summary>
        /// 栏位Id
        /// </summary>
        public string FieldId { get; set; } = string.Empty;

        /// <summary>
        /// 表单类型Id
        /// </summary>
        public string FormTypeId { get; set; } = string.Empty;

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
