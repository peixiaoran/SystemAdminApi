using Microsoft.Extensions.Logging;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Repository.CustMat.RollingForecast;

namespace SystemAdmin.Service.CustMat.RollingForecast
{
    public class NumberTrendService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<NumberTrendService> _logger;
        private readonly NumberTrendRepository _numberTrendRepo;

        public NumberTrendService(CurrentUser loginuser, ILogger<NumberTrendService> logger, NumberTrendRepository numberTrendRepo)
        {
            _loginuser = loginuser;
            _logger = logger;
            _numberTrendRepo = numberTrendRepo;
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage)
        {
            try
            {
                return await _numberTrendRepo.GetSalesNumberPage(getPage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<SalesNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 版本下拉框
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<ForecastVersionDropDto>>> GetForecastVersionDrop()
        {
            try
            {
                var list = await _numberTrendRepo.GetForecastVersionDrop();
                return Result<List<ForecastVersionDropDto>>.Ok(list, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<ForecastVersionDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 按料号统计版本用量（天、周、天+周合计）
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<List<ForecastWeeklyDetailStatDto>>> GetNumberTrend(GetNumberTrend query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.PartNumber) || query.VersionIds == null || query.VersionIds.Count == 0)
                    return Result<List<ForecastWeeklyDetailStatDto>>.Ok([], "");

                var versionIds = query.VersionIds.Select(long.Parse).Distinct().ToList();

                var versions = await _numberTrendRepo.GetForecastVersionsByIds(versionIds);
                var details = await _numberTrendRepo.GetForecastWeeklyDetailsByVersions(query.PartNumber, versionIds);

                var dayPeriodType = ForecastPeriodType.Day.ToEnumString();
                var weekPeriodType = ForecastPeriodType.Week.ToEnumString();

                var result = versions
                    .OrderBy(version => version.StartDate)
                    .Select(version =>
                    {
                        var versionDetails = details.Where(detail => detail.VersionId == version.VersionId).ToList();
                        var dayQty = versionDetails.Where(detail => detail.PeriodType == dayPeriodType).Sum(detail => detail.Qty);
                        var weekQty = versionDetails.Where(detail => detail.PeriodType == weekPeriodType).Sum(detail => detail.Qty);
                        return new ForecastWeeklyDetailStatDto
                        {
                            VersionId = version.VersionId,
                            VersionCode = version.VersionCode,
                            DayQty = dayQty,
                            WeekQty = weekQty,
                            TotalQty = dayQty + weekQty,
                        };
                    }).ToList();

                return Result<List<ForecastWeeklyDetailStatDto>>.Ok(result, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<ForecastWeeklyDetailStatDto>>.Failure(500, ex.Message);
            }
        }
    }
}
