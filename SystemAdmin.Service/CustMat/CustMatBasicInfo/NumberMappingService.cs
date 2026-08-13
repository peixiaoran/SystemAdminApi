using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SqlSugar;
using System.Data;
using System.Globalization;
using SystemAdmin.Common.Excel;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Commands;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Repository.CustMat.CustMatBasicInfo;

namespace SystemAdmin.Service.CustMat.CustMatBasicInfo
{
    public class NumberMappingService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<NumberMappingService> _logger;
        private readonly SqlSugarScope _db;
        private readonly NumberMappingRepository _numberMappingRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.CustMatBasicInfo.NumberMapping";
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.NumberMappingExcel_";
        private readonly string _thisImport = "CustMat.CustMatBasicInfo.NumberMappingImport_";

        // 导入/导出模板列
        private static readonly (string Key, bool Required)[] _templateColumns = new[]
        {
            ("CustomerPartNumber", true),
            ("CompanyPartNumber", true),
            ("EffectiveFrom", true),
            ("EffectiveTo", false),
            ("Status", true),
        };

        public NumberMappingService(CurrentUser loginuser, ILogger<NumberMappingService> logger, SqlSugarScope db, NumberMappingRepository numberMappingRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _numberMappingRepository = numberMappingRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增料号对照
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertNumberMapping(NumberMappingUpsert upsert)
        {
            try
            {
                var overlapping = await _numberMappingRepository.HasOverlappingMapping(upsert.CompanyPartNumber, upsert.EffectiveFrom, upsert.EffectiveTo, null);
                if (overlapping)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}MappingOverlap", args: upsert.CompanyPartNumber));

                await _db.BeginTranAsync();
                var entity = new NumberMappingEntity()
                {
                    MappingId = SnowFlakeSingle.Instance.NextId(),
                    CustomerPartNumber = upsert.CustomerPartNumber,
                    CompanyPartNumber = upsert.CompanyPartNumber,
                    EffectiveFrom = upsert.EffectiveFrom,
                    EffectiveTo = upsert.EffectiveTo,
                    Status = upsert.Status,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _numberMappingRepository.InsertNumberMapping(entity);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}InsertSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}InsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 删除料号对照
        /// </summary>
        /// <param name="mappingId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteNumberMapping(string mappingId)
        {
            try
            {
                await _db.BeginTranAsync();
                var delCount = await _numberMappingRepository.DeleteNumberMapping(long.Parse(mappingId));
                await _db.CommitTranAsync();

                return delCount >= 1
                        ? Result<int>.Ok(delCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}DeleteFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 修改料号对照
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateNumberMapping(NumberMappingUpsert upsert)
        {
            try
            {
                var mappingId = long.Parse(upsert.MappingId);
                var overlapping = await _numberMappingRepository.HasOverlappingMapping(upsert.CompanyPartNumber, upsert.EffectiveFrom, upsert.EffectiveTo, mappingId);
                if (overlapping)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}MappingOverlap", (object)upsert.CompanyPartNumber));

                await _db.BeginTranAsync();
                var entity = new NumberMappingEntity()
                {
                    MappingId = mappingId,
                    CustomerPartNumber = upsert.CustomerPartNumber,
                    CompanyPartNumber = upsert.CompanyPartNumber,
                    EffectiveFrom = upsert.EffectiveFrom,
                    EffectiveTo = upsert.EffectiveTo,
                    Status = upsert.Status,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _numberMappingRepository.UpdateNumberMapping(entity);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}UpdateSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UpdateFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号对照实体
        /// </summary>
        /// <param name="mappingId"></param>
        /// <returns></returns>
        public async Task<Result<NumberMappingDto>> GetNumberMappingEntity(string mappingId)
        {
            try
            {
                var entity = await _numberMappingRepository.GetNumberMappingEntity(long.Parse(mappingId));
                return Result<NumberMappingDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<NumberMappingDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号对照分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<NumberMappingDto>> GetNumberMappingPage(GetNumberMappingPage getPage)
        {
            try
            {
                return await _numberMappingRepository.GetNumberMappingPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<NumberMappingDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出料号对照信息
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<byte[]> GetNumberMappingExcel(GetNumberMappingPage getPage)
        {
            try
            {
                var entities = await _numberMappingRepository.GetNumberMappingList(getPage);

                var yesText = _localization.ReturnMsg($"{_thisExcel}Yes");
                var noText = _localization.ReturnMsg($"{_thisExcel}No");

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}DataSheetName"));

                var dt = new DataTable();
                foreach (var col in _templateColumns)
                {
                    dt.Columns.Add(col.Key, typeof(string));
                }

                foreach (var entity in entities)
                {
                    var row = dt.NewRow();
                    row["CustomerPartNumber"] = entity.CustomerPartNumber;
                    row["CompanyPartNumber"] = entity.CompanyPartNumber;
                    row["EffectiveFrom"] = entity.EffectiveFrom.ToString("yyyy/MM/dd");
                    row["EffectiveTo"] = entity.EffectiveTo?.ToString("yyyy/MM/dd");
                    row["Status"] = entity.Status == 1 ? yesText : noText;
                    dt.Rows.Add(row);
                }

                var headers = _templateColumns.ToDictionary(c => c.Key, c => _localization.ReturnMsg($"{_thisExcel}{c.Key}"));
                var columnConfigs = _templateColumns.ToDictionary(c => c.Key, c => ExcelColumnConfig.Text);

                ExcelStyleHelper.ApplyStandardStyle(ws, dt, headers, true, columnConfigs);
                package.Workbook.CalcMode = ExcelCalcMode.Manual;

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// 导出料号对照导入模板
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetNumberMappingTemplate()
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("Your Name");

                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}SheetName"));

                var dt = new DataTable();
                var headers = new Dictionary<string, string>();
                var columnConfigs = new Dictionary<string, ExcelColumnConfig>();
                foreach (var col in _templateColumns)
                {
                    dt.Columns.Add(col.Key, typeof(string));
                    headers[col.Key] = _localization.ReturnMsg($"{_thisExcel}{col.Key}");
                    columnConfigs[col.Key] = ExcelColumnConfig.Text;
                }

                ExcelStyleHelper.ApplyStandardStyle(ws, dt, headers, false, columnConfigs);
                package.Workbook.CalcMode = ExcelCalcMode.Manual;

                return Task.FromResult(package.GetAsByteArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Task.FromResult(Array.Empty<byte>());
            }
        }

        /// <summary>
        /// 导入客户料号与公司料号对照
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Result<int>> ImportNumberMapping(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}FileEmpty"));

                // EPPlus仅支持.xlsx，不支持旧版.xls
                if (!string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidFileFormat"));

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

                // 列数量是否对得上
                var expectedColCount = _templateColumns.Length;
                var actualColCount = ws.Dimension.End.Column;
                if (actualColCount != expectedColCount)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}ColumnCountMismatch"));

                // 列名是否符合中文或英文模板列名（逐列分别判断，允许中英文列名混用）
                var actualHeaders = new List<string>();
                for (var col = 1; col <= actualColCount; col++)
                {
                    actualHeaders.Add(ws.Cells[1, col].Text?.Trim() ?? string.Empty);
                }
                var zhHeaders = _templateColumns.Select(c => _localization.ReturnMsg($"{_thisExcel}{c.Key}", "zh-CN")).ToList();
                var enHeaders = _templateColumns.Select(c => _localization.ReturnMsg($"{_thisExcel}{c.Key}", "en-US")).ToList();
                for (var i = 0; i < actualHeaders.Count; i++)
                {
                    // 中文名、英文名、英文字段名均可，且忽略大小写与空格差异（如 Effective From / EffectiveFrom）
                    var accepted = new[] { zhHeaders[i], enHeaders[i], _templateColumns[i].Key };
                    if (!accepted.Any(header => NormalizeHeader(header) == NormalizeHeader(actualHeaders[i])))
                    {
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", i + 1, $"{zhHeaders[i]}/{enHeaders[i]}", actualHeaders[i]));
                    }
                }

                var effectiveFromColIndex = Array.FindIndex(_templateColumns, c => c.Key == "EffectiveFrom") + 1;
                var effectiveToColIndex = Array.FindIndex(_templateColumns, c => c.Key == "EffectiveTo") + 1;

                // 第一遍：读取原始数据（含日期列的解析结果），并收集需要批量校验的客户料号、公司料号，避免逐行查询
                var rawRows = new List<(int Row, Dictionary<string, string> Values, DateTime EffectiveFrom, bool EffectiveFromValid, DateTime? EffectiveTo, bool EffectiveToValid)>();
                var customerPartNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var companyPartNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var rowValues = new Dictionary<string, string>();
                    var rowIsEmpty = true;
                    for (var col = 1; col <= actualColCount; col++)
                    {
                        var text = ws.Cells[row, col].Text?.Trim() ?? string.Empty;
                        if (!string.IsNullOrEmpty(text))
                            rowIsEmpty = false;
                        rowValues[_templateColumns[col - 1].Key] = text;
                    }

                    // 整行为空则跳过，不当作数据行处理
                    if (rowIsEmpty)
                        continue;

                    // 时间列不论原始格式如何，只要能被解析为日期即可
                    var effectiveFromValid = TryParseExcelDate(ws.Cells[row, effectiveFromColIndex], out var effectiveFrom);

                    DateTime? effectiveTo = null;
                    var effectiveToValid = true;
                    if (!string.IsNullOrEmpty(rowValues["EffectiveTo"]))
                    {
                        effectiveToValid = TryParseExcelDate(ws.Cells[row, effectiveToColIndex], out var parsedEffectiveTo);
                        if (effectiveToValid)
                            effectiveTo = parsedEffectiveTo;
                    }

                    rawRows.Add((row, rowValues, effectiveFrom, effectiveFromValid, effectiveTo, effectiveToValid));

                    if (!string.IsNullOrEmpty(rowValues["CustomerPartNumber"]))
                        customerPartNumbers.Add(rowValues["CustomerPartNumber"]);
                    if (!string.IsNullOrEmpty(rowValues["CompanyPartNumber"]))
                        companyPartNumbers.Add(rowValues["CompanyPartNumber"]);
                }

                if (rawRows.Count == 0)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}NoData"));

                // 一次性批量查询：客户/公司料号是否存在且已启用，以及公司料号在数据库中已存在的对照记录（用于时段冲突校验，避免逐行查询）
                var validCustomerPartNumbers = new HashSet<string>(
                    await _numberMappingRepository.GetValidCustomerPartNumbers(customerPartNumbers.ToList()),
                    StringComparer.OrdinalIgnoreCase);
                var validCompanyPartNumbers = new HashSet<string>(
                    await _numberMappingRepository.GetValidCompanyPartNumbers(companyPartNumbers.ToList()),
                    StringComparer.OrdinalIgnoreCase);
                var existingMappings = await _numberMappingRepository.GetMappingsByCompanyPartNumbers(companyPartNumbers.ToList());

                // 已确认生效区间列表：先放入数据库中已有的记录，随文件逐行校验通过后再追加，
                // 这样既能校验与数据库的冲突，也能校验文件内部多行之间的冲突
                var acceptedRanges = existingMappings
                    .Select(mapping => (mapping.CompanyPartNumber, mapping.EffectiveFrom, mapping.EffectiveTo))
                    .ToList();

                var entities = new List<NumberMappingEntity>();
                foreach (var raw in rawRows)
                {
                    var row = raw.Row;
                    var rowValues = raw.Values;

                    // 实体非空验证（依据数据表结构：客户料号、公司料号、生效日期、状态为必填；失效日期可为空，表示无期限）
                    var emptyField = _templateColumns.FirstOrDefault(c => c.Required && string.IsNullOrEmpty(rowValues[c.Key]));
                    if (emptyField.Key != null)
                    {
                        var fieldLabel = _localization.ReturnMsg($"{_thisExcel}{emptyField.Key}");
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}RequiredFieldEmpty", row, fieldLabel));
                    }

                    // 生效日期必须是有效的日期
                    if (!raw.EffectiveFromValid)
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidDateValue", row, _localization.ReturnMsg($"{_thisExcel}EffectiveFrom"), rowValues["EffectiveFrom"]));

                    // 失效日期若填写，必须是有效的日期
                    if (!raw.EffectiveToValid)
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidDateValue", row, _localization.ReturnMsg($"{_thisExcel}EffectiveTo"), rowValues["EffectiveTo"]));

                    // 是否有效只能填写 是/否/Yes/No
                    if (!TryParseStatusText(rowValues["Status"], out var status))
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidStatusValue", row, rowValues["Status"]));

                    // 客户料号是否存在且已启用
                    var customerPartNumber = rowValues["CustomerPartNumber"];
                    if (!validCustomerPartNumbers.Contains(customerPartNumber))
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidCustomerPartNumber", row, customerPartNumber));

                    // 公司料号是否存在且已启用
                    var companyPartNumber = rowValues["CompanyPartNumber"];
                    if (!validCompanyPartNumbers.Contains(companyPartNumber))
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}InvalidCompanyPartNumber", row, companyPartNumber));

                    // 同一公司料号在相同时间段内是否已存在重叠的对照关系（含数据库已有数据及文件中已校验通过的行，不论客户料号是否相同）
                    var effectiveFromValue = raw.EffectiveFrom;
                    var effectiveToValue = raw.EffectiveTo;
                    var effectiveToOrMax = effectiveToValue ?? DateTime.MaxValue;
                    var overlapping = acceptedRanges.Any(range =>
                        string.Equals(range.CompanyPartNumber, companyPartNumber, StringComparison.OrdinalIgnoreCase) &&
                        range.EffectiveFrom <= effectiveToOrMax &&
                        (range.EffectiveTo == null || range.EffectiveTo >= effectiveFromValue));
                    if (overlapping)
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}MappingOverlap", row, companyPartNumber));

                    acceptedRanges.Add((companyPartNumber, effectiveFromValue, effectiveToValue));

                    entities.Add(new NumberMappingEntity
                    {
                        MappingId = SnowFlakeSingle.Instance.NextId(),
                        CustomerPartNumber = customerPartNumber,
                        CompanyPartNumber = companyPartNumber,
                        EffectiveFrom = effectiveFromValue,
                        EffectiveTo = effectiveToValue,
                        Status = status,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                if (entities.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));

                await _db.BeginTranAsync();
                var count = await _numberMappingRepository.InsertNumberMappingList(entities);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_thisImport}Success", count))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}InsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 客户料号下拉
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<Result<List<CustomerPartNumberDropDto>>> GetCustomerPartNumberDrop(string keyword)
        {
            try
            {
                var partNumbers = await _numberMappingRepository.GetCustomerPartNumberDrop(keyword);
                var drop = partNumbers.Select(partNumber => new CustomerPartNumberDropDto { Value = partNumber }).ToList();
                return Result<List<CustomerPartNumberDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CustomerPartNumberDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 公司料号下拉
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<Result<List<CompanyPartNumberDropDto>>> GetCompanyPartNumberDrop(string keyword)
        {
            try
            {
                var partNumbers = await _numberMappingRepository.GetCompanyPartNumberDrop(keyword);
                var drop = partNumbers.Select(partNumber => new CompanyPartNumberDropDto { Value = partNumber }).ToList();
                return Result<List<CompanyPartNumberDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CompanyPartNumberDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 归一化列名：去除空格等空白字符并转大写，用于宽松比对列名
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
        /// 解析Excel日期单元格：不论原始是日期类型、数值型日期序列还是文本，只要能识别为日期即转换
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private static bool TryParseExcelDate(ExcelRange cell, out DateTime date)
        {
            switch (cell.Value)
            {
                case DateTime dt:
                    date = dt;
                    return true;
                case double serial:
                    date = DateTime.FromOADate(serial);
                    return true;
                default:
                    var text = cell.Text?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        date = default;
                        return false;
                    }
                    return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                        || DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out date);
            }
        }

        /// <summary>
        /// 解析是否有效文本：只能是 是/否/Yes/No 这四种
        /// </summary>
        /// <param name="value"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static bool TryParseStatusText(string value, out int status)
        {
            switch (value.Trim())
            {
                case "是":
                case "Yes":
                    status = 1;
                    return true;
                case "否":
                case "No":
                    status = 0;
                    return true;
                default:
                    status = 0;
                    return false;
            }
        }
    }
}
