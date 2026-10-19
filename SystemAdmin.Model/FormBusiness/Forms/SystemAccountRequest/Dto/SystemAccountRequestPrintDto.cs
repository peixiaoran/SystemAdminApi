namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto
{
    /// <summary>
    /// 系统账号申请单打印名称Dto（编码、Id 转换为当前语言名称）
    /// </summary>
    public class SystemAccountRequestPrintDto
    {
        /// <summary>
        /// 需求人部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 需求人国籍名称
        /// </summary>
        public string NationalityName { get; set; } = string.Empty;

        /// <summary>
        /// 系统明细名称列表，与单据 Systems 顺序一致
        /// </summary>
        public List<SystemAccountSystemPrintDto> Systems { get; set; } = new List<SystemAccountSystemPrintDto>();
    }

    /// <summary>
    /// 系统账号申请单系统明细打印Dto
    /// </summary>
    public class SystemAccountSystemPrintDto
    {
        /// <summary>
        /// 系统名称
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 厂区名称
        /// </summary>
        public List<string> SiteNames { get; set; } = new List<string>();

        /// <summary>
        /// 权限名称
        /// </summary>
        public List<string> PermissionNames { get; set; } = new List<string>();

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }
    }
}
