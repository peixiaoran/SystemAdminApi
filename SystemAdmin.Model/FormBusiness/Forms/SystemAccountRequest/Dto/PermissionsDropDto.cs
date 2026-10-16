namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto
{
    /// <summary>
    /// 系统权限下拉Dto
    /// </summary>
    public class PermissionsDropDto
    {
        /// <summary>
        /// 权限编码
        /// </summary>
        public string Permissions { get; set; } = string.Empty;

        /// <summary>
        /// 权限名称
        /// </summary>
        public string PermissionsName { get; set; } = string.Empty;

        /// <summary>
        /// 权限说明
        /// </summary>
        public string PermissionsNotes { get; set; } = string.Empty;
    }
}
