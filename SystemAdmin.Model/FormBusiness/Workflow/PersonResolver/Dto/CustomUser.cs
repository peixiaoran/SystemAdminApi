namespace SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto
{
    /// <summary>
    /// 自定义用户返回Dto
    /// </summary>
    public class CustomUser
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public long DepartmentId { get; set; }

        /// <summary>
        /// 部门等级Id
        /// </summary>
        public long DepartmentLevelId { get; set; }

        /// <summary>
        /// 职级Id
        /// </summary>
        public long PositionId { get; set; }
    }
}
