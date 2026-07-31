namespace SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Commands
{
    /// <summary>
    /// 传签单保存类
    /// </summary>
    public class DocumentCirculateSave
    {
        /// <summary>
        /// 传签单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 发文单位
        /// </summary>
        public string? IssueDept { get; set; }

        /// <summary>
        /// 传签目的
        /// </summary>
        public string? CirculationPurpose { get; set; }

        /// <summary>
        /// 内容摘要
        /// </summary>
        public string? ContentSummary { get; set; }
    }
}
