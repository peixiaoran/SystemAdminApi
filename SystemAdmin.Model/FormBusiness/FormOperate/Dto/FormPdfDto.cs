namespace SystemAdmin.Model.FormBusiness.FormOperate.Dto
{
    /// <summary>
    /// 表单打印PDF文件Dto
    /// </summary>
    public class FormPdfDto
    {
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = null!;
    }
}
