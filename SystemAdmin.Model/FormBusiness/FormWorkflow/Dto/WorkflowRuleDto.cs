using System.Text.Json.Serialization;
using SystemAdmin.Model.ModelHelper.ModelConverter;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Dto
{
    /// <summary>
    /// 流程规则Dto
    /// </summary>
    public class WorkflowRuleDto
    {
        /// <summary>
        /// 规则Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long RuleId { get; set; }

        /// <summary>
        /// 表单类别Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long FormTypeId { get; set; }

        /// <summary>
        /// 规则名称（中文）
        /// </summary>
        public string RuleNameCn { get; set; } = string.Empty;

        /// <summary>
        /// 规则名称（英文）
        /// </summary>
        public string RuleNameEn { get; set; } = string.Empty;

        /// <summary>
        /// 职级Id
        /// </summary>
        [JsonConverter(typeof(LongToStringConverter))]
        public long? PositionId { get; set; }

        /// <summary>
        /// 职级名称
        /// </summary>
        public string? PositionName { get; set; }

        /// <summary>
        /// 导向
        /// </summary>
        public string? Guidance { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 生效开始日期
        /// </summary>
        public DateOnly EffectiveStartDate { get; set; }

        /// <summary>
        /// 生效结束日期
        /// </summary>
        public DateOnly? EffectiveEndDate { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
