namespace SystemAdmin.Model.FormBusiness.FormOperate.Dto
{
    /// <summary>
    /// 表单打印PDF文件Dto
    /// </summary>
    public class FormPdfDto
    {
        /// <summary>
        /// 文件名（含后缀）
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// PDF文件内容
        /// </summary>
        public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    }
}
