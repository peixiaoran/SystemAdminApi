namespace SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto
{
    /// <summary>
    /// 出差时间冲突Dto
    /// </summary>
    public class TripConflictDto
    {
        /// <summary>
        /// 冲突出差单号
        /// </summary>
        public string FormNo { get; set; } = string.Empty;

        /// <summary>
        /// 冲突出差开始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 冲突出差结束日期
        /// </summary>
        public DateTime EndDate { get; set; }
    }
}
