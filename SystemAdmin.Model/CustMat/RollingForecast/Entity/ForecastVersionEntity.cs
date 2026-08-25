using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Entity
{
    /// <summary>
    /// 预测版本实体类
    /// </summary>
    [SugarTable("[CustMat].[ForecastVersion]")]
    public class ForecastVersionEntity
    {
        /// <summary>
        /// 版本Id
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, ColumnDescription = "Primary Key")]
        public long VersionId { get; set; }

        /// <summary>
        /// 版本编号
        /// </summary>
        public string VersionCode { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 周次
        /// </summary>
        public int Week { get; set; }

        /// <summary>
        /// 是否最新
        /// </summary>
        public int IsLatest { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = string.Empty;

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
