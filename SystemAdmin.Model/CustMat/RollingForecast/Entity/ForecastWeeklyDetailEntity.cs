using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Entity
{
    /// <summary>
    /// 预测周明细实体类
    /// </summary>
    [SugarTable("[CustMat].[ForecastWeeklyDetail]")]
    public class ForecastWeeklyDetailEntity
    {
        /// <summary>
        /// 版本Id
        /// </summary>
        public long VersionId { get; set; }

        /// <summary>
        /// 公司料号
        /// </summary>
        public string PartNumber { get; set; } = string.Empty;

        /// <summary>
        /// 预测日期
        /// </summary>
        public DateTime HorizonDays { get; set; }

        /// <summary>
        /// 周期类型
        /// </summary>
        public string PeriodType { get; set; } = string.Empty;

        /// <summary>
        /// 预测数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 业务负责人Id
        /// </summary>
        public long SalesUserId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
