namespace SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto
{
    /// <summary>
    /// 步骤审批信息 + 步骤排序（查询流程 / 查询可驳回步骤共用）
    /// </summary>
    public class StepFlowItem
    {
        /// <summary>
        /// 步骤审批信息
        /// </summary>
        public StepReview Review { get; set; } = new StepReview();

        /// <summary>
        /// 步骤排序
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// 是否起始步骤
        /// </summary>
        public int IsStartStep { get; set; }
    }
}
