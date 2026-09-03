namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 业务负责客户料号明细Dto（用于统计客户分布占比）
    /// </summary>
    public class SalesCustomerNumberDto
    {
        /// <summary>
        /// 客户Id
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustomerPartNumber { get; set; } = string.Empty;
    }
}
