namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Commands
{
    /// <summary>
    /// 流程规则步骤新增/修改类
    /// </summary>
    public class WorkflowRuleStepUpsert
    {
        /// <summary>
        /// 规则Id
        /// </summary>
        public string RuleId { get; set; } = string.Empty;

        /// <summary>
        /// 当前步骤Id
        /// </summary>
        public string CurrentStepId { get; set; } = string.Empty;

        /// <summary>
        /// 下一步骤Id
        /// </summary>
        public string? NextStepId { get; set; }

        /// <summary>
        /// 过程导向：每有一人完成签核即执行，不等待会签其余人员（如会签场景第一人签核完成即执行）
        /// </summary>
        public string? ProcessGuidance { get; set; }

        /// <summary>
        /// 结论导向：步骤所有人签核完成（或跳过）后执行
        /// </summary>
        public string? ConclusionGuidance { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
