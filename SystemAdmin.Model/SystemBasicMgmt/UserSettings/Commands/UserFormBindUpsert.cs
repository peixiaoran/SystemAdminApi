namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Commands
{
    /// <summary>
    /// 用户绑定表单新增/修改类
    /// </summary>
    public class UserFormUpsert
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 表单组别类型Id
        /// </summary>
        public List<string> FormGroupTypeId { get; set; } = new List<string>();
    }
}
