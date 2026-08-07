namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto
{
    /// <summary>
    /// 物料分类下拉Dto
    /// </summary>
    public class PartCategoryDropDto
    {
        /// <summary>
        /// 物料分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
    }
}
