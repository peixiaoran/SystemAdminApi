namespace SystemAdmin.Model.CustMat.SalesMgmt.Commands
{
    /// <summary>
    /// 人员料号信息新增/修改类
    /// </summary>
    public class SalesNumberUpsert
    {
        /// <summary>
        /// 公司料号（主键，从公司料号下拉中选择）
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 原公司料号（仅编辑时传入，用于定位原记录；未传则默认与PartNumber相同）
        /// </summary>
        public string OriginalPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;
    }
}
