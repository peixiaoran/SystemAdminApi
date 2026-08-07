namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Commands
{
    /// <summary>
    /// 料号信息新增/修改类
    /// </summary>
    public class ProPartNumberUpsert
    {
        /// <summary>
        /// 料号Id
        /// </summary>
        public string PartNumberId { get; set; } = string.Empty;

        /// <summary>
        /// 料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        public string PartNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 英文品名
        /// </summary>
        public string PartNameEn { get; set; } = string.Empty;

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
        /// 材质
        /// </summary>
        public string Material { get; set; } = string.Empty;

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
    }
}
