namespace SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto
{
    /// <summary>
    /// 时间冲突Dto（请假单为申请人+代理人，代理关系为被代理人+代理人）
    /// </summary>
    public class UserTimeConflictDto
    {
        /// <summary>
        /// 被代理用户Id（请假单为申请人）
        /// </summary>
        public long SubstituteUserId { get; set; }

        /// <summary>
        /// 代理用户Id（请假单可能尚未选择代理人）
        /// </summary>
        public long? AgentUserId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
