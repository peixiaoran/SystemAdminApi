using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Entity
{
    /// <summary>
    /// 表单栏位权限实体
    /// </summary>
    [SugarTable("[Form].[StepFieldPermission]")]
    public class StepFieldPermissionEntity
    {
        /// <summary>
        /// 步骤Id
        /// </summary>
        public long StepId { get; set; }

        /// <summary>
        /// 栏位Id
        /// </summary>
        public long FieldId { get; set; }

        /// <summary>
        /// 是否显示
        /// </summary>
        public int IsVisible { get; set; }

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public int IsDisabled { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public long? ModifiedBy { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
