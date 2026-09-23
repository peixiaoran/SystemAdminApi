using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity
{
    /// <summary>
    /// 资讯需求单基础信息实体
    /// </summary>
    [SugarTable("[Forms].[InformationRequest]")]
    public class InformationRequestEntity
    {
        /// <summary>
        /// 资讯需求单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 需求类别
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// 现状
        /// </summary>
        public string? CurrentSituation { get; set; }

        /// <summary>
        /// 期望
        /// </summary>
        public string? Expectations { get; set; }

        /// <summary>
        /// 预计处理天数
        /// </summary>
        public decimal? EstimatedDays { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

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
