using Microsoft.Extensions.Logging;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Repository.CustMat.ForecastDetail;
using CompanyNumberDetailDto = SystemAdmin.Model.CustMat.SalesMgmt.Dto.CompanyNumberDetailDto;
using SalesUserDropDto = SystemAdmin.Model.CustMat.SalesMgmt.Dto.SalesUserDropDto;

namespace SystemAdmin.Service.CustMat.ForecastDetail
{
    public class NumberTrendService
    {
        private readonly ILogger<NumberTrendService> _logger;
        private readonly NumberTrendRepository _numberTrendRepo;

        public NumberTrendService(ILogger<NumberTrendService> logger, NumberTrendRepository numberTrendRepo)
        {
            _logger = logger;
            _numberTrendRepo = numberTrendRepo;
        }

        /// <summary>
        /// 按业务人员Id查询料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage)
        {
            try
            {
                var salesUserId = long.Parse(getPage.SalesUserId);
                return await _numberTrendRepo.GetSalesNumberPage(getPage, salesUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<SalesNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 业务人员下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            try
            {
                var list = await _numberTrendRepo.GetSalesUserDrop();
                return Result<List<SalesUserDropDto>>.Ok(list, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SalesUserDropDto>>.Failure(500, ex.Message);
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
        /// 按料号统计各版本用量
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<NumberTrendDto>> GetNumberTrend(GetNumberTrend query)
        {
            try
            {
                if (string.IsNullOrEmpty(query.PartNumber) || query.VersionIds == null || query.VersionIds.Count == 0)
                    return Result<NumberTrendDto>.Ok(new NumberTrendDto(), "");

                var versionIds = query.VersionIds.Select(long.Parse).Distinct().ToList();

                var partNumberInfo = await _numberTrendRepo.GetCompanyPartNumberInfo(query.PartNumber);
                var versions = await _numberTrendRepo.GetForecastVersionsByIds(versionIds);
                var details = await _numberTrendRepo.GetForecastWeeklyDetailsByVersions(query.PartNumber, versionIds);

                var dayPeriodType = ForecastPeriodType.Day.ToEnumString();
                var weekPeriodType = ForecastPeriodType.Week.ToEnumString();

                var versionStats = versions
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

                var result = new NumberTrendDto
                {
                    PartInfo = partNumberInfo ?? new CompanyNumberDetailDto { PartNumber = query.PartNumber },
                    Versions = versionStats,
                };

                return Result<NumberTrendDto>.Ok(result, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<NumberTrendDto>.Failure(500, ex.Message);
            }
        }
    }
}
