namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto
{
    /// <summary>
    /// 表单核准/驳回返回 Dto
    /// </summary>
    public class ReviewResultDto
    {
        /// <summary>
        /// 是否操作成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 0 流程停在包含当前操作人且需手动处理的步骤，前端需提示；1 无需提示
        /// </summary>
        public int Automatic { get; set; } = 1;
    }
}
