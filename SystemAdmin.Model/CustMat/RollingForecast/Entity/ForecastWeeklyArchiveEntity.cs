using SqlSugar;

namespace SystemAdmin.Model.CustMat.RollingForecast.Entity
{
    /// <summary>
    /// 预测周明细归档实体类（版本锁定时按业务人员归档的周明细JSON）
    /// </summary>
    [SugarTable("[CustMat].[ForecastWeeklyArchive]")]
    public class ForecastWeeklyArchiveEntity
    {
        /// <summary>
        /// 版本Id
        /// </summary>
        public long VersionId { get; set; }

        /// <summary>
        /// 业务人员Id
        /// </summary>
        public long SalesUserId { get; set; }

        /// <summary>
        /// 周明细JSON（与 GetFoWeeklyDetail 接口返回的 FoWeeklyDetailDto 结构一致）
        /// </summary>
        public string ForecastDetail { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
