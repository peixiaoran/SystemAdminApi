using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity
{
    /// <summary>
    /// 出差单基础信息实体
    /// </summary>
    [SugarTable("[Forms].[OverseasTripApp]")]
    public class OverseasTripAppEntity
    {
        /// <summary>
        /// 出差单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 出发厂区
        /// </summary>
        public string? DepartureFactory { get; set; }

        /// <summary>
        /// 目的厂区
        /// </summary>
        public string? DestinationFactory { get; set; }

        /// <summary>
        /// 出差事由
        /// </summary>
        public string? TripReason { get; set; }

        /// <summary>
        /// 出差开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 出差结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 出差天数
        /// </summary>
        public decimal? Days { get; set; }

        /// <summary>
        /// 去程交通方式
        /// </summary>
        public string? OutboundTravel { get; set; }

        /// <summary>
        /// 返程交通方式
        /// </summary>
        public string? ReturnTravel { get; set; }

        /// <summary>
        /// 工作内容
        /// </summary>
        public string? JobDescription { get; set; }

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
