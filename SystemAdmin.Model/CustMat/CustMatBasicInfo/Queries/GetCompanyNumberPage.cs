using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries
{
    public class GetCompanyNumberPage : PageModel
    {
        /// <summary>
        /// 料号编码
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名（中英文模糊匹配）
        /// </summary>
        public string PartName { get; set; } = string.Empty;

        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 料号类型（全值匹配）
        /// </summary>
        public string PartType { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类（全值匹配）
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 型号
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 图号
        /// </summary>
        public string DrawingNumber { get; set; } = string.Empty;

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型（全值匹配）
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态（1：启用，0：停用，不传则不筛选）
        /// </summary>
        public int? Status { get; set; }
    }
}
