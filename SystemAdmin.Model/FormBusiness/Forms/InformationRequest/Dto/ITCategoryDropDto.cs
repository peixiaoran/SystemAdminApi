namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Dto
{
    /// <summary>
    /// 需求类别下拉Dto
    /// </summary>
    public class ITCategoryDropDto
    {
        /// <summary>
        /// 需求类别编码
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 需求类别名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
