using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity
{
    /// <summary>
    /// 客户料号与公司料号对照表实体类
    /// </summary>
    [SugarTable("[CustMat].[NumberMapping]")]
    public class NumberMappingEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long MappingId { get; set; }

        /// <summary>
        /// 客户料号
        /// </summary>
        public string CustomerPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 公司料号
        /// </summary>
        public string CompanyPartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

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
