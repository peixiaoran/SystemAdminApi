namespace SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Commands
{
    /// <summary>
    /// 销假单保存类
    /// </summary>
    public class LeaveCancellSave
    {
        /// <summary>
        /// 销假单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 请假单Id
        /// </summary>
        public string LeaveRequestId { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDateTime { get; set; }
    }
}
