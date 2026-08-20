using SqlSugar;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Entity
{
    /// <summary>
    /// 人员料号信息实体类
    /// </summary>
    [SugarTable("[CustMat].[SalesNumber]")]
    public class SalesNumberEntity
    {
        /// <summary>
        /// 公司料号（主键，关联公司料号信息）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public long SalesUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedDate { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string? ModifiedDate { get; set; }
    }
}
