using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.FormWorkflow.Entity
{
    /// <summary>
    /// 表单栏位实体
    /// </summary>
    [SugarTable("[Form].[FormStepFieId]")]
    public class FormFieIdEntity
    {
        /// <summary>
        /// 表单类别Id
        /// </summary>
        public long FormTypeId { get; set; }

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
        public int IsEditable { get; set; }

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
