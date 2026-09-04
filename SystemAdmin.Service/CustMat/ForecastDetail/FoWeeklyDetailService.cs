using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
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
        /// 复用预测周明细模块的Excel文案（列头、导出文件名）
        /// </summary>
        private readonly string _thisExcel = "CustMat.RollingForecast.FoWeeklyDetailExcel_";

        /// <summary>
        /// 固定列数（料号、品名）
        /// </summary>
        private const int FixedColumnCount = 2;

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
        /// 导出预测周明细，可按业务人员Id筛选
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<(byte[] Bytes, string FileName)> GetFoWeeklyDetailExcel(string versionId, string? salesUserId)
        {
            try
            {
                var result = await GetFoWeeklyDetail(versionId, salesUserId);
                if (result.Code != 200 || result.Data == null)
                    return ([], string.Empty);

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}Export"));
                WriteFoWeeklyDetailWorksheet(ws, result.Data.Periods, result.Data.Rows);

                package.Workbook.CalcMode = ExcelCalcMode.Manual;
                return (package.GetAsByteArray(), BuildExcelFileName("Export", result.Data.VersionCode));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ([], string.Empty);
            }
        }

        /// <summary>
        /// 拼接导出文件名，版本编码存在时追加为后缀
        /// </summary>
        /// <param name="key"></param>
        /// <param name="versionCode"></param>
        /// <returns></returns>
        private string BuildExcelFileName(string key, string? versionCode)
        {
            var name = $"{_localization.ReturnMsg($"{_thisExcel}{key}", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}{key}", "en-US")}";
            return string.IsNullOrEmpty(versionCode) ? $"{name}.xlsx" : $"{name}_{versionCode}.xlsx";
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
        /// 按料号、品名 + 天/周日期列的固定格式写入预测周明细工作表
        /// </summary>
        /// <param name="ws"></param>
        /// <param name="periods"></param>
        /// <param name="rows"></param>
        private void WriteFoWeeklyDetailWorksheet(ExcelWorksheet ws, List<FoWeeklyPeriodDto> periods, List<FoWeeklyRowDto> rows)
        {
            ws.Cells[1, 1].Value = _localization.ReturnMsg($"{_thisExcel}PartNumber");
            ws.Cells[1, 2].Value = _localization.ReturnMsg($"{_thisExcel}PartName");

            for (int i = 0; i < periods.Count; i++)
            {
                ws.Cells[1, FixedColumnCount + 1 + i].Value = periods[i].StartDate.ToString("yyyy-MM-dd");
            }

            for (int r = 0; r < rows.Count; r++)
            {
                int rowIndex = r + 2;
                ws.Cells[rowIndex, 1].Value = rows[r].PartNumber;
                ws.Cells[rowIndex, 2].Value = rows[r].PartName;
                for (int c = 0; c < periods.Count; c++)
                {
                    ws.Cells[rowIndex, FixedColumnCount + 1 + c].Value = rows[r].Quantities[periods[c].PeriodKey];
                }
            }

            int totalRows = rows.Count + 1;
            int totalCols = FixedColumnCount + periods.Count;

            ws.Cells[2, 1, totalRows, 1].Style.Numberformat.Format = "@";

            var headerRange = ws.Cells[1, 1, 1, totalCols];
            headerRange.Style.Font.Name = "微软雅黑";
            headerRange.Style.Font.Bold = true;
            headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Row(1).Height = 25;

            // 天、周日期列头分别用深绿、深黄底色区分
            for (int i = 0; i < periods.Count; i++)
            {
                var headerCell = ws.Cells[1, FixedColumnCount + 1 + i];
                headerCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCell.Style.Fill.BackgroundColor.SetColor(
                    periods[i].PeriodType == ForecastPeriodType.Day.ToEnumString()
                        ? ColorTranslator.FromHtml("#67c23a")
                        : ColorTranslator.FromHtml("#e6a23c"));
                headerCell.Style.Font.Color.SetColor(Color.White);
            }

            if (totalRows > 1)
            {
                var dataRange = ws.Cells[2, 1, totalRows, totalCols];
                dataRange.Style.Font.Name = "微软雅黑";
                dataRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                dataRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            var border = ws.Cells[1, 1, totalRows, totalCols].Style.Border;
            border.Top.Style = border.Bottom.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

            ws.View.FreezePanes(2, FixedColumnCount + 1);
            if (ws.Dimension != null)
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
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
