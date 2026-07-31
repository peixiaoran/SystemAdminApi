using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Entity
{
    /// <summary>
    /// 传签单基础信息实体
    /// </summary>
    [SugarTable("[Form].[DocumentCirculate]")]
    public class DocumentCirculateEntity
    {
        /// <summary>
        /// 传签单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 发文单位
        /// </summary>
        public string? IssueDept { get; set; }

        /// <summary>
        /// 传签目的
        /// </summary>
        public string? CirculationPurpose { get; set; }

        /// <summary>
        /// 内容摘要
        /// </summary>
        public string? ContentSummary { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
