using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity
{
    /// <summary>
    /// 表单加审人表
    /// </summary>
    [SugarTable("[Form].[FormAddReview]")]
    public class FormAddReviewEntity
    {
        /// <summary>
        /// 表单Id
        /// </summary>
        public long FormId { get; set; }

        /// <summary>
        /// 加审人部门名称
        /// </summary>
        public string DeptName { get; set; } = string.Empty;

        /// <summary>
        /// 加审人Id
        /// </summary>
        public long UserId { get; set; }

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
        /// 修改时间
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
    }
}
