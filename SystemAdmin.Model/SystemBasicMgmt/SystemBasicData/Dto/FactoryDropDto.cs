namespace SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto
{
    /// <summary>
    /// 厂区下拉Dto
    /// </summary>
    public class FactoryDropDto
    {
        /// <summary>
        /// 厂区编码
        /// </summary>
        public string Factory { get; set; } = string.Empty;

        /// <summary>
        /// 厂区名称
        /// </summary>
        public string FactoryName { get; set; } = string.Empty;
    }
}
