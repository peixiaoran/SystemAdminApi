namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Upsert
{
    /// <summary>
    /// 表单加审人新增/修改类
    /// </summary>
    public class FormAddReviewUpsert
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        public string FormId { get; set; } = string.Empty;

        /// <summary>
        /// 加审人部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 加审人Id
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 加审人工号
        /// </summary>
        public string UserNo { get; set; } = string.Empty;

        /// <summary>
        /// 加审人姓名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 加审顺序
        /// </summary>
        public int SortOrder { get; set; }
    }
}
