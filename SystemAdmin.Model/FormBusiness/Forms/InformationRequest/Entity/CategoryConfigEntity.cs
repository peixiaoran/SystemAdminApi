using SqlSugar;

namespace SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity
{
    /// <summary>
    /// 资讯需求类别负责人配置实体
    /// </summary>
    [SugarTable("[IT].[CategoryConfig]")]
    public class CategoryConfigEntity
    {
        /// <summary>
        /// 需求类别
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public string System { get; set; } = string.Empty;

        /// <summary>
        /// 负责人
        /// </summary>
        public long PersonChargeId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? SortOrder { get; set; }

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
