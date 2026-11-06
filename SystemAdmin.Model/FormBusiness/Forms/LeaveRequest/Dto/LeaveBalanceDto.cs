namespace SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Dto
{
    /// <summary>
    /// 用户请假余额Dto
    /// </summary>
    public class LeaveBalanceDto
    {
        /// <summary>
        /// 年度
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 年假剩余时数
        /// </summary>
        public decimal AnnualRemainingHours { get; set; }

        /// <summary>
        /// 病假剩余时数
        /// </summary>
        public decimal SickRemainingHours { get; set; }
    }
}
