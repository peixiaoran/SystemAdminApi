namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 本人负责的公司料号Dto
    /// </summary>
    public class SalesNumberDto
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartName { get; set; } = string.Empty;
    }
}
