using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity
{
    /// <summary>
    /// 表单检索表
    /// </summary>
    [SugarTable("[Forms].[FormSearch]")]
    public class FormSearchEntity
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 标准表检索文本（单号、申请日期、申请人工号/姓名、部门）
        /// </summary>
        public string FormText { get; set; } = string.Empty;

        /// <summary>
        /// 明细表检索文本（各业务表单字段）
        /// </summary>
        public string DetailText { get; set; } = string.Empty;

        /// <summary>
        /// 附件表检索文本（附件文件名）
        /// </summary>
        public string AttachmentText { get; set; } = string.Empty;

        /// <summary>
        /// 加审人检索文本（部门、工号、姓名）
        /// </summary>
        public string AddReviewText { get; set; } = string.Empty;

        /// <summary>
        /// 修改人
        /// </summary>
        public long ModifiedBy { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifiedDate { get; set; }
    }
}
