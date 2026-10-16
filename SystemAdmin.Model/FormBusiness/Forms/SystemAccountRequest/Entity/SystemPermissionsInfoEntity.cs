using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity
{
    /// <summary>
    /// 系统权限实体
    /// </summary>
    [SugarTable("[IT].[SystemParmissionsInfo]")]
    public class SystemPermissionsInfoEntity
    {
        /// <summary>
        /// 系统
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// 厂区
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string Site { get; set; } = string.Empty;

        /// <summary>
        /// 权限编码
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string PermissionsCode { get; set; } = string.Empty;

        /// <summary>
        /// 权限名（中文）
        /// </summary>
        [SugarColumn(ColumnName = "ParmissionsNameCn")]
        public string PermissionsNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 权限名（英文）
        /// </summary>
        [SugarColumn(ColumnName = "ParmissionsNameEn")]
        public string PermissionsNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 权限说明（中文）
        /// </summary>
        [SugarColumn(ColumnName = "ParmissionsNotesCn")]
        public string PermissionsNotesCn { get; set; } = string.Empty;

        /// <summary>
        /// 权限说明（英文）
        /// </summary>
        [SugarColumn(ColumnName = "ParmissionsNotesEn")]
        public string PermissionsNotesEn { get; set; } = string.Empty;

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
