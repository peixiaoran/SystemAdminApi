namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Commands
{
    /// <summary>
    /// 步骤栏位权限新增/修改类
    /// </summary>
    public class StepFieldPermissionUpsert
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        public string StepId { get; set; } = string.Empty;

        /// <summary>
        /// 栏位Id
        /// </summary>
        public string FieldId { get; set; } = string.Empty;

        /// <summary>
        /// 是否显示
        /// </summary>
        public int IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int IsDisabled { get; set; }
    }
}
