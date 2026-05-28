namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto
{
    /// <summary>
    /// 表单步骤栏位权限Dto
    /// </summary>
    public class StepFieldPermissionDto
    {
        /// <summary>
        /// 栏位Id
        /// </summary>
        public string FieldKey { get; set; } = string.Empty;

        /// <summary>
        /// 栏位名称
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示
        /// </summary>
        public int IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int IsEditable { get; set; }
    }
}
