namespace SystemAdmin.Model.CustMat.SalesMgmt.Dto
{
    /// <summary>
    /// 公司料号下拉Dto（配合 el-select 远程搜索使用）
    /// </summary>
    public class CompanyNumberDropDto
    {
        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;
    }
}
