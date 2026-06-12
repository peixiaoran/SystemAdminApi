namespace SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Commands
{
    /// <summary>
    /// 请假单保存类
    /// </summary>
    public class LeaveFormSave
    {
        /// <summary>
        /// 请假单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 假别
        /// </summary>
        public string? LeaveType { get; set; }

        /// <summary>
        /// 请假事由
        /// </summary>
        public string? LeaveReason { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// 代理人Id
        /// </summary>
        public string? AgentUserId { get; set; }

        /// <summary>
        /// 代理人姓名
        /// </summary>
        public string? AgentUserName { get; set; }
    }
}
