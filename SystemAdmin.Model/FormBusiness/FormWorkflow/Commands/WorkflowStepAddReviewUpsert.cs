namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Commands
{
    /// <summary>
    /// 步骤加审规则新增/修改类
    /// </summary>
    public class WorkflowStepAddReviewUpsert
    {
        /// <summary>
        /// 加审顺序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
