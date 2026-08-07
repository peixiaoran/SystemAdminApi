using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries
{
    public class GetProPartNumberPage : PageModel
    {
        /// <summary>
        /// 料号编码
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 料号类型
        /// </summary>
        public string PartType { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态（1：启用，0：停用，不传则不筛选）
        /// </summary>
        public int? Status { get; set; }
    }
}
