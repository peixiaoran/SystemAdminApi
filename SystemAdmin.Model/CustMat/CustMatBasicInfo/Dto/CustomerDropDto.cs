namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto
{
    /// <summary>
    /// 客户下拉Dto
    /// </summary>
    public class CustomerDropDto
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;
    }
}
