namespace SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto
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
}
