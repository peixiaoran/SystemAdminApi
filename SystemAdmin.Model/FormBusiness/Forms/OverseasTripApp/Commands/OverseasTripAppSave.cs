namespace SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Commands
{
    /// <summary>
    /// 出差单保存类
    /// </summary>
    public class OverseasTripAppSave
    {
        /// <summary>
        /// 出差单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 目的厂区
        /// </summary>
        public string? DestinationSite { get; set; }

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
    }
}
