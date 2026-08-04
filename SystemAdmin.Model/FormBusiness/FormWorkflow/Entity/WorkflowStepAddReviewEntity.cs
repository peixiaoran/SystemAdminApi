using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Entity
{
    /// <summary>
    /// 步骤加审规则表
    /// </summary>
    [SugarTable("[Workflow].[WorkflowStepAddReview]")]
    public class WorkflowStepAddReviewEntity
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long StepId { get; set; }

        /// <summary>
        /// 加审顺序
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
    }
}
