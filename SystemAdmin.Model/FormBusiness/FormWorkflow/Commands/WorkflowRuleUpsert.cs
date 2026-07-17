namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Commands
{
    /// <summary>
    /// 流程规则新增/修改类
    /// </summary>
    public class WorkflowRuleUpsert
    {
        /// <summary>
        /// 规则Id
        /// </summary>
        public string RuleId { get; set; } = string.Empty;

        /// <summary>
        /// 表单类别Id
        /// </summary>
        public string FormTypeId { get; set; } = string.Empty;

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
        public string? PositionId { get; set; }

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
