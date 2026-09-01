namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 业务负责料号信息Dto
    /// </summary>
    public class SalesNumberDto
    {
        /// <summary>
        /// 料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 英文品名
        /// </summary>
        public string PartNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态
        /// </summary>
        public int Status { get; set; }
    }
}
