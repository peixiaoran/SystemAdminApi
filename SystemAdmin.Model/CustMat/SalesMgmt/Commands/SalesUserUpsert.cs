namespace SystemAdmin.Model.CustMat.SalesMgmt.Commands
{
    /// <summary>
    /// 业务人员信息新增/修改类
    /// </summary>
    public class SalesUserUpsert
    {
        /// <summary>
        /// 用户Id（主键，从用户分页中选择）
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 原用户Id（仅编辑时传入，用于定位原记录；未传则默认与SalesUserId相同）
        /// </summary>
        public string OriginalSalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 部门Id
        /// </summary>
        public string SalesDeptId { get; set; } = string.Empty;

        /// <summary>
        /// 业务类型
        /// </summary>
        public string SalesType { get; set; } = string.Empty;
    }
}
