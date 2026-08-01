using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 批量查询审批人的结果行：ComboKey 标示所属条件组（实体映射需公开类型）
    /// </summary>
    public class BatchUserReview : UserReview
    {
        /// <summary>
        /// 条件组标识
        /// </summary>
        public int ComboKey { get; set; }
    }

    /// <summary>
    /// 审批人查询过滤方式
    /// </summary>
    internal enum ReviewUserFilter
    {
        /// <summary>按组织架构（上级部门链 + 部门级别 + 职级）</summary>
        Org,

        /// <summary>按指定部门 + 职级</summary>
        Dept,

        /// <summary>按指定人</summary>
        User,
    }

    /// <summary>
    /// 审批人查询投影
    /// </summary>
    /// <param name="WithNames">是否输出姓名、身份名称、排序列（完整投影）</param>
    /// <param name="WithAgent">是否关联代理人</param>
    /// <param name="IsChinese">姓名/字典名称取中文列还是英文列（仅 WithNames 时生效）</param>
    internal sealed record ReviewUserProjection(bool WithNames, bool WithAgent, bool IsChinese)
    {
        /// <summary>完整投影：姓名 + 代理 + 身份名称</summary>
        internal static ReviewUserProjection Named(bool isChinese) => new(true, true, isChinese);

        /// <summary>精简投影：身份 + 代理</summary>
        internal static ReviewUserProjection Appointment { get; } = new(false, true, false);

        /// <summary>精简投影：仅身份，不关联代理</summary>
        internal static ReviewUserProjection AppointmentNoAgent { get; } = new(false, false, false);
    }
}
