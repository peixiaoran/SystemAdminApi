namespace SystemAdmin.Model.CustMat.SalesMgmt.Commands
{
    /// <summary>
    /// 料号分配批量修改类：按客户找出其料号对照中的公司料号，统一绑定到指定业务负责人
    /// </summary>
    public class NumberAssignBatchUpsert
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 更新范围：0-仅补未配置的料号（已配置的保留原负责人）；1-全部覆盖（含已配置的料号）
        /// </summary>
        public int UpdateMode { get; set; }
    }
}
