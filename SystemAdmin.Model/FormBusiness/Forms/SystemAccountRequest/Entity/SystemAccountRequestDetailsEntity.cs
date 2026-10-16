using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity
{
    /// <summary>
    /// 系统账号申请单明细实体
    /// </summary>
    [SugarTable("[Forms].[SystemAccountRequestDetails]")]
    public class SystemAccountRequestDetailsEntity
    {
        /// <summary>
        /// 系统账号申请单Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long FormId { get; set; }

        /// <summary>
        /// 系统
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// 厂区
        /// </summary>
        public string Sites { get; set; } = string.Empty;

        /// <summary>
        /// 权限
        /// </summary>
        public string Permissions { get; set; } = string.Empty;

        /// <summary>
        /// 评分
        /// </summary>
        public int? Rating { get; set; }

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
