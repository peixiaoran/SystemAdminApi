using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity
{
    /// <summary>
    /// 客户料号信息实体类
    /// </summary>
    [SugarTable("[CustMat].[CustomerNumber]")]
    public class CustomerNumberEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long PartNumberId { get; set; }

        /// <summary>
        /// 客户料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 品名（中文）
        /// </summary>
        public string PartNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 品名（英文）
        /// </summary>
        public string PartNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 规格型号
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 启用状态
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
