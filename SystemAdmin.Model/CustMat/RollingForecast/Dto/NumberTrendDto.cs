using SystemAdmin.Model.CustMat.SalesMgmt.Dto;

namespace SystemAdmin.Model.CustMat.RollingForecast.Dto
{
    /// <summary>
    /// 料号版本用量趋势Dto（料号基础信息 + 各版本用量统计）
    /// </summary>
    public class NumberTrendDto
    {
        /// <summary>
        /// 料号基础信息
        /// </summary>
        public CompanyNumberDetailDto PartInfo { get; set; } = new();

        /// <summary>
        /// 各版本用量统计
        /// </summary>
        public List<ForecastWeeklyDetailStatDto> Versions { get; set; } = new();
    }
}
