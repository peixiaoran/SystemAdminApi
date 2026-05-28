using SqlSugar;
namespace SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity
{
    /// <summary>
    /// 表单类型栏位实体
    /// </summary>
    [SugarTable("[Form].[FormTypeField]")]
    public class FormTypeFieldEntity
    {
        /// <summary>
        /// 栏位Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FieldId { get; set; }

        /// <summary>
        /// 表单类型Id
        /// </summary>
        public long FormTypeId { get; set; }

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

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
