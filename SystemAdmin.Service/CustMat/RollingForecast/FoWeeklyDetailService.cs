using Microsoft.Extensions.Logging;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Repository.CustMat.RollingForecast;

namespace SystemAdmin.Service.CustMat.RollingForecast
{
    public class FoWeeklyDetailService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FoWeeklyDetailService> _logger;
        private readonly FoWeeklyDetailRepository _foWeeklyDetailRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.RollingForecast.FoWeeklyDetail";

        /// <summary>
        /// 按天展开的天数
        /// </summary>
        private const int DayCount = 21;

        /// <summary>
        /// 按周展开的周数
        /// </summary>
        private const int WeekCount = 52;

        public FoWeeklyDetailService(CurrentUser loginuser, ILogger<FoWeeklyDetailService> logger, FoWeeklyDetailRepository foWeeklyDetailRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _foWeeklyDetailRepo = foWeeklyDetailRepo;
            _localization = localization;
        }

        /// <summary>
        /// 查询预测版本分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage(GetForecastVersionPage getPage)
        {
            try
            {
                return await _foWeeklyDetailRepo.GetForecastVersionPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<ForecastVersionDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询预测周明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyDetail(string versionId)
        {
            try
            {
                var version = await _foWeeklyDetailRepo.GetForecastVersion(long.Parse(versionId));
                if (version == null)
                    return Result<FoWeeklyDetailDto>.Failure(400, _localization.ReturnMsg($"{_this}VersionNotFound"));

                var startDate = version.StartDate.Date;
                var periods = BuildPeriods(startDate);

                var result = new FoWeeklyDetailDto
                {
                    VersionId = version.VersionId,
                    VersionCode = version.VersionCode,
                    StartDate = startDate,
                    Periods = periods,
                };

                // 登录用户所负责的公司料号
                var rows = await _foWeeklyDetailRepo.GetSalesPartNumbers(_loginuser.UserId);
                foreach (var row in rows)
                {
                    row.Quantities = periods.ToDictionary(period => period.PeriodKey, period => 0m);
                }
                result.Rows = rows;

                if (rows.Count == 0)
                    return Result<FoWeeklyDetailDto>.Ok(result, "");

                // 每个日期归属的列，用于把明细数量落到对应周期上
                var periodKeyOfDate = BuildPeriodKeyOfDate(periods);
                var rowOfPartNumber = rows.ToDictionary(row => row.PartNumber);

                var details = await _foWeeklyDetailRepo.GetForecastWeeklyDetails(version.VersionId, [.. rows.Select(row => row.PartNumber)]);
                foreach (var detail in details)
                {
                    if (!rowOfPartNumber.TryGetValue(detail.PartNumber, out var row))
                        continue;
                    if (!periodKeyOfDate.TryGetValue(detail.HorizonDays.Date, out var periodKey))
                        continue;
                    row.Quantities[periodKey] += detail.Qty;
                }

                return Result<FoWeeklyDetailDto>.Ok(result, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FoWeeklyDetailDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 生成预测周明细的列，前21列按天，之后52列按周
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private static List<FoWeeklyPeriodDto> BuildPeriods(DateTime startDate)
        {
            var periods = new List<FoWeeklyPeriodDto>(DayCount + WeekCount);

            for (int i = 0; i < DayCount; i++)
            {
                var day = startDate.AddDays(i);
                periods.Add(new FoWeeklyPeriodDto
                {
                    PeriodKey = $"D{i + 1}",
                    PeriodType = ForecastPeriodType.Day.ToEnumString(),
                    StartDate = day,
                    EndDate = day,
                });
            }

            // 按天的部分结束后紧接着按周，版本开始日期为周一，因此每周仍以周一起算
            var weekStartDate = startDate.AddDays(DayCount);
            for (int i = 0; i < WeekCount; i++)
            {
                var monday = weekStartDate.AddDays(i * 7);
                periods.Add(new FoWeeklyPeriodDto
                {
                    PeriodKey = $"W{i + 1}",
                    PeriodType = ForecastPeriodType.Week.ToEnumString(),
                    StartDate = monday,
                    EndDate = monday.AddDays(6),
                });
            }

            return periods;
        }

        /// <summary>
        /// 生成日期到列标识的映射
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        private static Dictionary<DateTime, string> BuildPeriodKeyOfDate(List<FoWeeklyPeriodDto> periods)
        {
            var periodKeyOfDate = new Dictionary<DateTime, string>();
            foreach (var period in periods)
            {
                for (var date = period.StartDate; date <= period.EndDate; date = date.AddDays(1))
                {
                    periodKeyOfDate[date] = period.PeriodKey;
                }
            }
            return periodKeyOfDate;
        }
    }
}
