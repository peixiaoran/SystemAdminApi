using Microsoft.Extensions.Logging;
using System.Text.Json;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Repository.CustMat.ForecastDetail;

namespace SystemAdmin.Service.CustMat.ForecastDetail
{
    public class FoWeeklyDetailService
    {
        private readonly ILogger<FoWeeklyDetailService> _logger;
        private readonly FoWeeklyDetailRepository _foWeeklyDetailRepo;
        private readonly LocalizationService _localization;

        /// <summary>
        /// 复用预测周明细模块的多语言文案（版本不存在提示）
        /// </summary>
        private readonly string _this = "CustMat.RollingForecast.FoWeeklyDetail";

        /// <summary>
        /// 按天展开的天数
        /// </summary>
        private const int DayCount = 21;

        /// <summary>
        /// 按周展开的周数
        /// </summary>
        private const int WeekCount = 13;

        public FoWeeklyDetailService(ILogger<FoWeeklyDetailService> logger, FoWeeklyDetailRepository foWeeklyDetailRepo, LocalizationService localization)
        {
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
        /// 业务人员下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            try
            {
                var list = await _foWeeklyDetailRepo.GetSalesUserDrop();
                return Result<List<SalesUserDropDto>>.Ok(list, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SalesUserDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询预测周明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyDetail(string versionId, string? salesUserId)
        {
            try
            {
                var version = await _foWeeklyDetailRepo.GetForecastVersion(long.Parse(versionId));
                if (version == null)
                    return Result<FoWeeklyDetailDto>.Failure(400, _localization.ReturnMsg($"{_this}VersionNotFound"));

                long? salesUserIdValue = string.IsNullOrEmpty(salesUserId) ? null : long.Parse(salesUserId);

                var periods = BuildPeriods(version.StartDate.Date);

                var rows = version.IsLatest == 1
                    ? await _foWeeklyDetailRepo.GetAllSalesPartNumbers(salesUserIdValue)
                    : await _foWeeklyDetailRepo.GetAllImportedPartNumbers(version.VersionId, salesUserIdValue);
                foreach (var row in rows)
                {
                    row.Quantities = periods.ToDictionary(period => period.PeriodKey, period => 0m);
                }

                if (rows.Count > 0)
                {
                    var periodKeyOfDate = periods.ToDictionary(period => period.StartDate, period => period.PeriodKey);
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
                }

                await FillTotalsAndChangeRates(version, periods, rows);

                var result = new FoWeeklyDetailDto
                {
                    VersionId = version.VersionId,
                    VersionCode = version.VersionCode,
                    StartDate = version.StartDate.Date,
                    Periods = periods,
                    Rows = rows,
                };

                return Result<FoWeeklyDetailDto>.Ok(result, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FoWeeklyDetailDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询指定版本锁定时归档的预测周明细，可按业务人员Id筛选；不筛选时合并所有业务人员的归档行（返回结构与 GetFoWeeklyDetail 一致）
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId">为空时查询全部业务人员</param>
        /// <returns></returns>
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyArchiveDetail(string versionId, string? salesUserId)
        {
            try
            {
                long? salesUserIdValue = string.IsNullOrEmpty(salesUserId) ? null : long.Parse(salesUserId);

                var archives = await _foWeeklyDetailRepo.GetForecastWeeklyArchives(long.Parse(versionId), salesUserIdValue);

                FoWeeklyDetailDto? result = null;
                foreach (var archive in archives)
                {
                    if (string.IsNullOrEmpty(archive.ForecastDetail))
                        continue;

                    var detail = JsonSerializer.Deserialize<FoWeeklyDetailDto>(archive.ForecastDetail);
                    if (detail == null)
                        continue;

                    // 同一版本各业务人员的列定义相同，取第一份归档作为基础，其余只合并数据行
                    if (result == null)
                    {
                        result = detail;
                        continue;
                    }
                    result.Rows.AddRange(detail.Rows);
                }

                if (result == null)
                    return Result<FoWeeklyDetailDto>.Failure(400, _localization.ReturnMsg($"{_this}ArchiveNotFound"));

                return Result<FoWeeklyDetailDto>.Ok(result, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FoWeeklyDetailDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 按料号填充天/周数量合计，以及环比上周的变化百分比（保留2位小数，上周数量为0时为空）
        /// </summary>
        /// <param name="version"></param>
        /// <param name="periods"></param>
        /// <param name="rows"></param>
        private async Task FillTotalsAndChangeRates(ForecastVersionEntity version, List<FoWeeklyPeriodDto> periods, List<FoWeeklyRowDto> rows)
        {
            if (rows.Count == 0)
                return;

            var dayType = ForecastPeriodType.Day.ToEnumString();
            var weekType = ForecastPeriodType.Week.ToEnumString();
            var dayKeys = periods.Where(period => period.PeriodType == dayType).Select(period => period.PeriodKey).ToHashSet();
            var weekKeys = periods.Where(period => period.PeriodType == weekType).Select(period => period.PeriodKey).ToHashSet();

            foreach (var row in rows)
            {
                row.DayTotal = dayKeys.Sum(key => row.Quantities[key]);
                row.WeekTotal = weekKeys.Sum(key => row.Quantities[key]);
            }

            var previousVersion = await _foWeeklyDetailRepo.GetPreviousVersion(version.StartDate);
            if (previousVersion == null)
                return;

            var previousDetails = await _foWeeklyDetailRepo.GetForecastWeeklyDetails(previousVersion.VersionId, [.. rows.Select(row => row.PartNumber)]);
            var previousQtyOfPartNumber = previousDetails
                .GroupBy(detail => (detail.PartNumber, detail.PeriodType))
                .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Qty));

            foreach (var row in rows)
            {
                var previousDayQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, dayType), 0m);
                var previousWeekQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, weekType), 0m);

                row.DayQtyChangeRate = previousDayQty == 0 ? null : Math.Round((row.DayTotal - previousDayQty) / previousDayQty * 100, 2);
                row.WeekQtyChangeRate = previousWeekQty == 0 ? null : Math.Round((row.WeekTotal - previousWeekQty) / previousWeekQty * 100, 2);
            }
        }

        /// <summary>
        /// 生成预测周明细的列，前21列按天，之后13列按周
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
                });
            }

            return periods;
        }
    }
}
