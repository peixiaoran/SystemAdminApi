namespace SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Queries
{
    /// <summary>
    /// 导出用户信息Excel请求参数
    /// </summary>
    public class GetUserInfoExcel
    {
        /// <summary>
        /// 部门Id
        /// </summary>
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// 用户工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
