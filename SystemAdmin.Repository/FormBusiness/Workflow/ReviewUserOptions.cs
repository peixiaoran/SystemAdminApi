using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 批量查询审批人结果行
    /// </summary>
    public class BatchUserReview : UserReview
    {
        /// <summary>
        /// 条件组标识
        /// </summary>
        public int ComboKey { get; set; }
    }

    /// <summary>
    /// 审批人过滤方式
    /// </summary>
    internal enum ReviewUserFilter
    {
        /// <summary>组织架构（上级部门链 + 部门级别 + 职级）</summary>
        Org,

        /// <summary>指定部门 + 职级</summary>
        Dept,

        /// <summary>指定人</summary>
        User,
    }

    /// <summary>
    /// 审批人查询投影
    /// </summary>
    /// <param name="WithNames">是否输出姓名、身份名称、排序列</param>
    /// <param name="WithAgent">是否关联代理人</param>
    /// <param name="IsChinese">名称取中文还是英文列（仅 WithNames 时生效）</param>
    internal sealed record ReviewUserProjection(bool WithNames, bool WithAgent, bool IsChinese)
    {
        /// <summary>完整投影</summary>
        internal static ReviewUserProjection Named(bool isChinese) => new(true, true, isChinese);

        /// <summary>身份 + 代理</summary>
        internal static ReviewUserProjection Appointment { get; } = new(false, true, false);

        /// <summary>仅身份</summary>
        internal static ReviewUserProjection AppointmentNoAgent { get; } = new(false, false, false);
    }
}
