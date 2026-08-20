using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto
{
    /// <summary>
    /// 公司料号详情Dto（下拉字典字段中英文均返回）
    /// </summary>
    public class CompanyNumberDetailDto
    {
        /// <summary>
        /// 料号Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long PartNumberId { get; set; }

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
        /// 料号类型名称（中文）
        /// </summary>
        public string PartTypeNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 料号类型名称（英文）
        /// </summary>
        public string PartTypeNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类名称（中文）
        /// </summary>
        public string CategoryNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 物料分类名称（英文）
        /// </summary>
        public string CategoryNameEn { get; set; } = string.Empty;

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
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型
        /// </summary>
        public string SourceType { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型名称（中文）
        /// </summary>
        public string SourceTypeNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 来源类型名称（英文）
        /// </summary>
        public string SourceTypeNameEn { get; set; } = string.Empty;

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
        public int Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }
    }
}
