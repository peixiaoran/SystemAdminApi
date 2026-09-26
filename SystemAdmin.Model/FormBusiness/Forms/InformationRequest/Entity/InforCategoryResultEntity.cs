using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity
{
    /// <summary>
    /// 资讯需求单处理结果实体
    /// </summary>
    [SugarTable("[IT].[InforCategoryResult]")]
    public class InforCategoryResultEntity
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 申请人Id
        /// </summary>
        public long? ApplicantUserId { get; set; }

        /// <summary>
        /// 处理人Id
        /// </summary>
        public long? HandlerId { get; set; }

        /// <summary>
        /// 预计时长（天数）
        /// </summary>
        public decimal? EstimatedDays { get; set; }

        /// <summary>
        /// 处理开始时间
        /// </summary>
        public DateTime? ProcessStartTime { get; set; }

        /// <summary>
        /// 处理完成时间
        /// </summary>
        public DateTime? ProcessEndTime { get; set; }

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
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
