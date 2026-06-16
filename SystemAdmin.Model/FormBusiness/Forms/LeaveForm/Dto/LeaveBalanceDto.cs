namespace SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto
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
        /// 年假剩余天数
        /// </summary>
        public int AnnualRemainingDays { get; set; }

        /// <summary>
        /// 病假剩余天数
        /// </summary>
        public int SickRemainingDays { get; set; }
    }
}
