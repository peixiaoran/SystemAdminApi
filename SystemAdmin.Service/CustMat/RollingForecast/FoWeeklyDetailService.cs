using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SqlSugar;
using System.Drawing;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Repository.CustMat.RollingForecast;

namespace SystemAdmin.Service.CustMat.RollingForecast
{
    public class FoWeeklyDetailService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FoWeeklyDetailService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FoWeeklyDetailRepository _foWeeklyDetailRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.RollingForecast.FoWeeklyDetail";
        private readonly string _thisExcel = "CustMat.RollingForecast.FoWeeklyDetailExcel_";
        private readonly string _thisImport = "CustMat.RollingForecast.FoWeeklyDetailImport_";

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

        public FoWeeklyDetailService(CurrentUser loginuser, ILogger<FoWeeklyDetailService> logger, SqlSugarScope db, FoWeeklyDetailRepository foWeeklyDetailRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
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
                var (version, periods, rows) = await BuildFoWeeklyDetailRows(long.Parse(versionId), withActualQty: true);
                if (version == null)
                    return Result<FoWeeklyDetailDto>.Failure(400, _localization.ReturnMsg($"{_this}VersionNotFound"));

                await FillQtyChangeRates(version, periods, rows);

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
        /// 导出预测周明细模板
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFoWeeklyDetailTemplate(string versionId)
        {
            try
            {
                var (version, periods, rows) = await BuildFoWeeklyDetailRows(long.Parse(versionId), withActualQty: false);
                if (version == null)
                    return [];

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}Template"));
                WriteFoWeeklyDetailWorksheet(ws, periods, rows);

                package.Workbook.CalcMode = ExcelCalcMode.Manual;
                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return [];
            }
        }

        /// <summary>
        /// 导出预测周明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<byte[]> GetFoWeeklyDetailExcel(string versionId)
        {
            try
            {
                var (version, periods, rows) = await BuildFoWeeklyDetailRows(long.Parse(versionId), withActualQty: true);
                if (version == null)
                    return [];

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}Export"));
                WriteFoWeeklyDetailWorksheet(ws, periods, rows);

                package.Workbook.CalcMode = ExcelCalcMode.Manual;
                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return [];
            }
        }

        /// <summary>
        /// 导入预测周明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Result<int>> ImportFoWeeklyDetail(string versionId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}FileEmpty"));

                // EPPlus仅支持.xlsx，不支持旧版.xls
                if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidFileFormat"));

                var version = await _foWeeklyDetailRepo.GetForecastVersion(long.Parse(versionId));
                if (version == null)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}VersionNotFound"));

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");

                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);

                // 是否有Sheet表
                if (package.Workbook.Worksheets.Count == 0)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}NoWorksheet"));

                var ws = package.Workbook.Worksheets[0];

                // 是否至少有一条数据
                if (ws.Dimension == null || ws.Dimension.End.Row < 2)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}NoData"));

                // 版本对应的完整周期列（21天+13周）
                var periods = BuildPeriods(version.StartDate.Date);
                var expectedColCount = FixedColumnCount + periods.Count;
                var actualColCount = ws.Dimension.End.Column;
                if (actualColCount != expectedColCount)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}ColumnCountMismatch", expectedColCount, actualColCount));

                // 公司料号、品名列头校验，中英文均可
                var zhPartNumber = _localization.ReturnMsg($"{_thisExcel}PartNumber", "zh-CN");
                var enPartNumber = _localization.ReturnMsg($"{_thisExcel}PartNumber", "en-US");
                var partNumberHeader = ws.Cells[1, 1].Text?.Trim() ?? string.Empty;
                if (!IsHeaderMatch(partNumberHeader, zhPartNumber, enPartNumber))
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", 1, $"{zhPartNumber}/{enPartNumber}", partNumberHeader));

                var zhPartName = _localization.ReturnMsg($"{_thisExcel}PartName", "zh-CN");
                var enPartName = _localization.ReturnMsg($"{_thisExcel}PartName", "en-US");
                var partNameHeader = ws.Cells[1, 2].Text?.Trim() ?? string.Empty;
                if (!IsHeaderMatch(partNameHeader, zhPartName, enPartName))
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", 2, $"{zhPartName}/{enPartName}", partNameHeader));

                // 不通过原因逐条累积，最后一次性返回
                var errors = new List<string>();

                // 日期列头必须与版本周期一一对应
                for (int i = 0; i < periods.Count; i++)
                {
                    var col = FixedColumnCount + 1 + i;
                    var headerText = ws.Cells[1, col].Text?.Trim();
                    if (!DateTime.TryParse(headerText, out var headerDate) || headerDate.Date != periods[i].StartDate.Date)
                        errors.Add(_localization.ReturnMsg($"{_thisImport}DateHeaderMismatch", col, periods[i].StartDate.ToString("yyyy-MM-dd"), headerText ?? string.Empty));
                }

                // 一次性查询文件中所有公司料号的有效性、以及是否已配置为当前登录人负责
                var filePartNumbers = new List<string>();
                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var partNumber = ws.Cells[row, 1].Text?.Trim();
                    if (!string.IsNullOrEmpty(partNumber))
                        filePartNumbers.Add(partNumber);
                }
                var distinctPartNumbers = filePartNumbers.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                var validPartNumbers = new HashSet<string>(
                    await _foWeeklyDetailRepo.GetExistingCompanyNumbers(distinctPartNumbers),
                    StringComparer.OrdinalIgnoreCase);
                var assignedPartNumbers = new HashSet<string>(
                    await _foWeeklyDetailRepo.GetAssignedPartNumbers(distinctPartNumbers, _loginuser.UserId),
                    StringComparer.OrdinalIgnoreCase);

                var entities = new List<ForecastWeeklyDetailEntity>();
                var now = DateTime.Now;

                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var partNumber = ws.Cells[row, 1].Text?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(partNumber))
                    {
                        errors.Add(_localization.ReturnMsg($"{_thisImport}PartNumberEmpty", row));
                        continue;
                    }

                    if (!validPartNumbers.Contains(partNumber))
                        errors.Add(_localization.ReturnMsg($"{_thisImport}PartNumberInvalid", row, partNumber));

                    if (!assignedPartNumbers.Contains(partNumber))
                        errors.Add(_localization.ReturnMsg($"{_thisImport}PartNumberNotAssigned", row, partNumber));

                    for (int i = 0; i < periods.Count; i++)
                    {
                        var col = FixedColumnCount + 1 + i;
                        var text = ws.Cells[row, col].Text?.Trim();
                        if (string.IsNullOrEmpty(text))
                            continue;

                        if (!decimal.TryParse(text, out var qty))
                        {
                            errors.Add(_localization.ReturnMsg($"{_thisImport}QtyNotNumeric", row, col, text));
                            continue;
                        }

                        entities.Add(new ForecastWeeklyDetailEntity
                        {
                            VersionId = version.VersionId,
                            PartNumber = partNumber,
                            HorizonDays = periods[i].StartDate,
                            PeriodType = periods[i].PeriodType,
                            Qty = qty,
                            SalesUserId = _loginuser.UserId,
                            CreatedDate = now,
                        });
                    }
                }

                if (errors.Count > 0)
                    return Result<int>.Failure(400, string.Join("\n", errors));

                if (entities.Count == 0)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}NoData"));

                await _db.BeginTranAsync();
                await _foWeeklyDetailRepo.DeleteForecastWeeklyDetails(version.VersionId);
                var count = await _foWeeklyDetailRepo.InsertForecastWeeklyDetailList(entities);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_thisImport}Success", count))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询指定版本的周期列与料号行，可选择是否填充实际预测数量
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="withActualQty">true时按ForecastWeeklyDetail填充实际数量，false时数量固定为0</param>
        /// <returns></returns>
        private async Task<(ForecastVersionEntity? version, List<FoWeeklyPeriodDto> periods, List<FoWeeklyRowDto> rows)> BuildFoWeeklyDetailRows(long versionId, bool withActualQty)
        {
            var version = await _foWeeklyDetailRepo.GetForecastVersion(versionId);
            if (version == null)
                return (null, [], []);

            var periods = BuildPeriods(version.StartDate.Date);

            // 最新版本按人员料号对照查询，非最新版本直接取当时导入数据中的公司料号
            var rows = version.IsLatest == 1
                ? await _foWeeklyDetailRepo.GetSalesPartNumbers(_loginuser.UserId)
                : await _foWeeklyDetailRepo.GetImportedPartNumbers(version.VersionId, _loginuser.UserId);
            foreach (var row in rows)
            {
                row.Quantities = periods.ToDictionary(period => period.PeriodKey, period => 0m);
            }

            if (withActualQty && rows.Count > 0)
            {
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
            }

            return (version, periods, rows);
        }

        /// <summary>
        /// 按料号填充天、周数量环比上周的变化百分比（保留2位小数，上周数量为0时为空）
        /// </summary>
        /// <param name="version"></param>
        /// <param name="periods"></param>
        /// <param name="rows"></param>
        private async Task FillQtyChangeRates(ForecastVersionEntity version, List<FoWeeklyPeriodDto> periods, List<FoWeeklyRowDto> rows)
        {
            if (rows.Count == 0)
                return;

            var previousVersion = await _foWeeklyDetailRepo.GetPreviousVersion(version.StartDate);
            if (previousVersion == null)
                return;

            var dayType = ForecastPeriodType.Day.ToEnumString();
            var weekType = ForecastPeriodType.Week.ToEnumString();
            var dayKeys = periods.Where(period => period.PeriodType == dayType).Select(period => period.PeriodKey).ToHashSet();
            var weekKeys = periods.Where(period => period.PeriodType == weekType).Select(period => period.PeriodKey).ToHashSet();

            var previousDetails = await _foWeeklyDetailRepo.GetForecastWeeklyDetails(previousVersion.VersionId, [.. rows.Select(row => row.PartNumber)]);
            var previousQtyOfPartNumber = previousDetails
                .GroupBy(detail => (detail.PartNumber, detail.PeriodType))
                .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Qty));

            foreach (var row in rows)
            {
                var currentDayQty = dayKeys.Sum(key => row.Quantities[key]);
                var currentWeekQty = weekKeys.Sum(key => row.Quantities[key]);
                var previousDayQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, dayType), 0m);
                var previousWeekQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, weekType), 0m);

                row.DayQtyChangeRate = previousDayQty == 0 ? null : Math.Round((currentDayQty - previousDayQty) / previousDayQty * 100, 2);
                row.WeekQtyChangeRate = previousWeekQty == 0 ? null : Math.Round((currentWeekQty - previousWeekQty) / previousWeekQty * 100, 2);
            }
        }

        /// <summary>
        /// 按料号、品名 + 天/周日期列的固定格式写入预测周明细工作表（模板导出与数据导出共用）
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
        /// 判断列头是否匹配中文或英文名称（忽略大小写与空格差异）
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="zh"></param>
        /// <param name="en"></param>
        /// <returns></returns>
        private static bool IsHeaderMatch(string actual, string zh, string en)
        {
            var normalized = NormalizeHeader(actual);
            return normalized == NormalizeHeader(zh) || normalized == NormalizeHeader(en);
        }

        /// <summary>
        /// 去除空白并转大写，用于列头比对
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string NormalizeHeader(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return new string(value.Where(ch => !char.IsWhiteSpace(ch)).ToArray()).ToUpperInvariant();
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

        /// <summary>
        /// 生成日期到列标识的映射
        /// </summary>
        /// <param name="periods"></param>
        /// <returns></returns>
        private static Dictionary<DateTime, string> BuildPeriodKeyOfDate(List<FoWeeklyPeriodDto> periods)
        {
            return periods.ToDictionary(period => period.StartDate, period => period.PeriodKey);
        }
    }
}
