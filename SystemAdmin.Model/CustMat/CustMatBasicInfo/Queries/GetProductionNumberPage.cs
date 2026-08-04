using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries
{
    public class GetProductionNumberPage : PageModel
    {
        /// <summary>
        /// 料号编码
        /// </summary>
        public string PartNumberNo { get; set; } = string.Empty;
    }
}
