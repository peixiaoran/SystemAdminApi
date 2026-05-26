namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Commands
{
    /// <summary>
    /// 步骤指定用户新增/修改类
    /// </summary>
    public class WorkflowStepUserUpsert
    {
        /// <summary>
        /// 部门Id
        /// </summary>
        public string DepartmentId { get; set; } = string.Empty;

        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}
