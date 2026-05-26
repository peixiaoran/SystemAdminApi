using SqlSugar;

namespace SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    [SugarTable("[Basic].[UserInfo]")]
    public class SysUserInfoEntity
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long UserId { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 职级Id
        /// </summary>
        public long PositionId { get; set; }

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
    }
}
