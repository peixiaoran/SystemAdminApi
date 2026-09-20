namespace SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto
{
    /// <summary>
    /// 自定义取人结果Dto：定位到具体的「部门 + 职级」这一角色，
    /// 该角色下实职/兼任/代理人员的解析与优先级（实 &gt; 代 &gt; 兼 &gt; 兼代）统一交由既有的按部门职级取人逻辑处理
    /// </summary>
    public class CustomUser
    {
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
