using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Dto
{
    /// <summary>
    /// 流程规则步骤Dto类
    /// </summary>
    public class WorkflowRuleStepDto
    {
        /// <summary>
        /// 规则Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long RuleId { get; set; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// 当前步骤Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long CurrentStepId { get; set; }

        /// <summary>
        /// 当前步骤名称
        /// </summary>
        public string CurrentStepName { get; set; } = string.Empty;

        /// <summary>
        /// 下一步骤Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long NextStepId { get; set; }

        /// <summary>
        /// 下一步骤名称
        /// </summary>
        public string NextStepName { get; set; } = string.Empty;

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
