using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity
{
    /// <summary>
    /// 系统账号申请单单头实体
    /// </summary>
    [SugarTable("[Forms].[SystemAccountRequest]")]
    public class SystemAccountRequestEntity
    {
        /// <summary>
        /// 系统账号申请单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 需求人部门
        /// </summary>
        public long? RequesterUserDept { get; set; }

        /// <summary>
        /// 需求人工号
        /// </summary>
        public string? RequesterUserNo { get; set; }

        /// <summary>
        /// 需求人姓名（中文）
        /// </summary>
        public string? RequesterUserNameCn { get; set; }

        /// <summary>
        /// 需求人姓名（英文）
        /// </summary>
        public string? RequesterUserNameEn { get; set; }

        /// <summary>
        /// 需求人性别
        /// </summary>
        public int? RequesterGender { get; set; }

        /// <summary>
        /// 需求人国籍
        /// </summary>
        public long? NationalityId { get; set; }

        /// <summary>
        /// 需求人邮件
        /// </summary>
        public string? RequesterEmail { get; set; }

        /// <summary>
        /// 申请原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
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
