using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Entity
{
    /// <summary>
    /// 流程规则实体类
    /// </summary>
    [SugarTable("[Workflow].[WorkflowRuleStep]")]
    public class WorkflowRuleStepEntity
    {
        /// <summary>
        /// 规则Id
        /// </summary>
        public long RuleId { get; set; }

        /// <summary>
        /// 当前步骤Id
        /// </summary>
        public long CurrentStepId { get; set; }

        /// <summary>
        /// 下一步骤Id
        /// </summary>
        public long? NextStepId { get; set; }

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

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
