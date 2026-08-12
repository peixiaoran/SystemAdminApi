namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Commands
{
    /// <summary>
    /// 客户料号信息新增/修改类
    /// </summary>
    public class CustomerNumberUpsert
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string PartNumberId { get; set; } = string.Empty;

        /// <summary>
        /// 客户料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名（中文）
        /// </summary>
        public string PartNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 品名（英文）
        /// </summary>
        public string PartNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态
        /// </summary>
        public int Status { get; set; }
    }
}
