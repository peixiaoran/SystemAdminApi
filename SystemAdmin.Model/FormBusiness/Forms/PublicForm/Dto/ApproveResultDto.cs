namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto
{
    /// <summary>
    /// 表单核准返回 Dto
    /// </summary>
    public class ApproveResultDto
    {
        /// <summary>
        /// 是否核准成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否自动签核：0 核准后停在含当前操作人且未自动签核的步骤，需提示再次手动签核；1 无需提示
        /// </summary>
        public int Automatic { get; set; } = 1;
    }
}
