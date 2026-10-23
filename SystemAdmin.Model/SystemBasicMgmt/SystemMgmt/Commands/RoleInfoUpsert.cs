namespace SystemAdmin.Model.SystemBasicMgmt.SystemMgmt.Commands
{
    /// <summary>
    /// 角色新增/修改类
    /// </summary>
    public class RoleInfoUpsert
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// 角色编码
        /// </summary>
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称（中文）
        /// </summary>
        public string RoleNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称（英文）
        /// </summary>
        public string RoleNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 角色描述
        /// </summary>
        public string? Description { get; set; }
    }
}
