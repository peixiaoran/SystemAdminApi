using SqlSugar;

namespace SystemAdmin.Model.CustMat.SalesMgmt.Entity
{
    /// <summary>
    /// 业务人员信息实体类
    /// </summary>
    [SugarTable("[CustMat].[SalesUser]")]
    public class SalesUserEntity
    {
        /// <summary>
        /// 用户Id（主键，关联用户信息）
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long SalesUserId { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public long SalesDeptId { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string SalesType { get; set; } = string.Empty;

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
