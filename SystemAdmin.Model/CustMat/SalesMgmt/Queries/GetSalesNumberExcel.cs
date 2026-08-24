namespace SystemAdmin.Model.CustMat.SalesMgmt.Queries
{
    /// <summary>
    /// 导出人员料号Excel请求参数
    /// </summary>
    public class GetSalesNumberExcel
    {
        /// <summary>
        /// 公司料号（模糊匹配）
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public string SalesUserId { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;
    }
}
