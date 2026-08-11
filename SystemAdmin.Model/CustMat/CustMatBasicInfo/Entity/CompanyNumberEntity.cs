using SqlSugar;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity
{
    /// <summary>
    /// 公司料号信息实体类
    /// </summary>
    [SugarTable("[CustMat].[CompanyNumber]")]
    public class CompanyNumberEntity
    {
        /// <summary>
        /// 料号Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long PartNumberId { get; set; }

        /// <summary>
        /// 料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string ProductNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 英文品名
        /// </summary>
        public string ProductNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 规格
        /// </summary>
        public string Specification { get; set; } = string.Empty;

        /// <summary>
        /// 料号类型
        /// </summary>
        public string PartType { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类
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
        /// 基本单位
        /// </summary>
        public string BaseUnit { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// 制造商
        /// </summary>
        public string? Manufacturer { get; set; }

        /// <summary>
        /// 制造商料号
        /// </summary>
        public string? ManufacturerPartNumber { get; set; }

        /// <summary>
        /// 是否批号管制
        /// </summary>
        public bool LotControl { get; set; }

        /// <summary>
        /// 启用状态
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建日期
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
