using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SqlSugar;
using System.Data;
using SystemAdmin.Common.Excel;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Commands;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Repository.CustMat.CustMatBasicInfo;

namespace SystemAdmin.Service.CustMat.CustMatBasicInfo
{
    public class CustomerNumberService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<CustomerNumberService> _logger;
        private readonly SqlSugarScope _db;
        private readonly CustomerNumberRepository _customerNumberRepository;
        private readonly NumberMappingRepository _numberMappingRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.CustMatBasicInfo.CustomerNumberInfo";
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.CustomerNumberExcel_";
        private readonly string _thisImport = "CustMat.CustMatBasicInfo.CustomerNumberImport_";

        // 导入/导出模板列
        private static readonly (string Key, bool Required)[] _templateColumns = new[]
        {
            ("PartNumber", true),
            ("CustomerCode", true),
            ("PartNameCn", true),
            ("PartNameEn", true),
            ("Specification", true),
            ("Unit", true),
            ("Status", true),
        };

        public CustomerNumberService(CurrentUser loginuser, ILogger<CustomerNumberService> logger, SqlSugarScope db, CustomerNumberRepository customerNumberRepository, NumberMappingRepository numberMappingRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _customerNumberRepository = customerNumberRepository;
            _numberMappingRepository = numberMappingRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增客户料号信息
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertCustomerNumber(CustomerNumberUpsert upsert)
        {
            try
            {
                var exists = await _customerNumberRepository.ExistsCustomerNumber(upsert.PartNumber);
                if (exists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}PartNumberDuplicate", (object)upsert.PartNumber));

                await _db.BeginTranAsync();
                var entity = new CustomerNumberEntity()
                {
                    PartNumberId = SnowFlakeSingle.Instance.NextId(),
                    PartNumber = upsert.PartNumber,
                    CustomerCode = upsert.CustomerCode,
                    PartNameCn = upsert.PartNameCn,
                    PartNameEn = upsert.PartNameEn,
                    Specification = upsert.Specification,
                    Unit = upsert.Unit,
                    Status = upsert.Status,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _customerNumberRepository.InsertCustomerNumber(entity);
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
        /// 删除客户料号信息
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteCustomerNumber(string partNumberId)
        {
            try
            {
                var id = long.Parse(partNumberId);
                var entity = await _customerNumberRepository.GetCustomerNumberEntity(id);

                await _db.BeginTranAsync();
                // 联动删除该客户料号下的全部公司料号对照关系
                if (entity != null)
                    await _numberMappingRepository.DeleteMappingsByCustomerPartNumber(entity.PartNumber);
                var delCustomerNumberCount = await _customerNumberRepository.DeleteCustomerNumber(id);
                await _db.CommitTranAsync();

                return delCustomerNumberCount >= 1
                        ? Result<int>.Ok(delCustomerNumberCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
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
        /// 修改客户料号信息
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateCustomerNumber(CustomerNumberUpsert upsert)
        {
            try
            {
                await _db.BeginTranAsync();
                var entity = new CustomerNumberEntity()
                {
                    PartNumberId = long.Parse(upsert.PartNumberId),
                    PartNumber = upsert.PartNumber,
                    CustomerCode = upsert.CustomerCode,
                    PartNameCn = upsert.PartNameCn,
                    PartNameEn = upsert.PartNameEn,
                    Specification = upsert.Specification,
                    Unit = upsert.Unit,
                    Status = upsert.Status,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _customerNumberRepository.UpdateCustomerNumber(entity);

                // 联动失效：客户料号被修改为停用时，其下全部公司料号对照关系一并置为失效
                if (upsert.Status == 0)
                    await _numberMappingRepository.InvalidateMappingsByCustomerPartNumber(upsert.PartNumber, _loginuser.UserId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

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
        /// 查询客户料号信息实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<Result<CustomerNumberDto>> GetCustomerNumberEntity(string partNumberId)
        {
            try
            {
                var entity = await _customerNumberRepository.GetCustomerNumberEntity(long.Parse(partNumberId));
                return Result<CustomerNumberDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CustomerNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询客户料号信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<CustomerNumberDto>> GetCustomerNumberPage(GetCustomerNumberPage getPage)
        {
            try
            {
                return await _customerNumberRepository.GetCustomerNumberPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<CustomerNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出客户料号信息
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<byte[]> GetCustomerNumberExcel(GetCustomerNumberPage getPage)
        {
            try
            {
                var entities = await _customerNumberRepository.GetCustomerNumberList(getPage);

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
                    row["PartNumber"] = entity.PartNumber;
                    row["CustomerCode"] = entity.CustomerCode;
                    row["PartNameCn"] = entity.PartNameCn;
                    row["PartNameEn"] = entity.PartNameEn;
                    row["Specification"] = entity.Specification;
                    row["Unit"] = entity.Unit;
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
        /// 导出客户料号导入模板
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetCustomerNumberTemplate()
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
        /// 导入客户料号信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Result<int>> ImportCustomerNumber(IFormFile file)
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
                    // 中文名、英文名、英文字段名均可，且忽略大小写与空格差异（如 Part Number / PartNumber）
                    var accepted = new[] { zhHeaders[i], enHeaders[i], _templateColumns[i].Key };
                    if (!accepted.Any(header => NormalizeHeader(header) == NormalizeHeader(actualHeaders[i])))
                    {
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", i + 1, $"{zhHeaders[i]}/{enHeaders[i]}", actualHeaders[i]));
                    }
                }

                // 一次性查询文件中所有客户料号在数据库中是否已存在，避免逐行查询
                var partNumberColIndex = Array.FindIndex(_templateColumns, c => c.Key == "PartNumber") + 1;
                var filePartNumbers = new List<string>();
                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var partNumberText = ws.Cells[row, partNumberColIndex].Text?.Trim();
                    if (!string.IsNullOrEmpty(partNumberText))
                        filePartNumbers.Add(partNumberText);
                }
                var existingCustomerNumbers = new HashSet<string>(
                    await _customerNumberRepository.GetExistingCustomerNumbers(filePartNumbers),
                    StringComparer.OrdinalIgnoreCase);

                var entities = new List<CustomerNumberEntity>();
                var seenPartNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

                    // 实体非空验证
                    var emptyField = _templateColumns.FirstOrDefault(c => c.Required && string.IsNullOrEmpty(rowValues[c.Key]));
                    if (emptyField.Key != null)
                    {
                        var fieldLabel = _localization.ReturnMsg($"{_thisExcel}{emptyField.Key}");
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}RequiredFieldEmpty", row, fieldLabel));
                    }

                    // 客户料号是否重复：文件内重复 或 数据库中已存在
                    var partNumber = rowValues["PartNumber"];
                    if (!seenPartNumbers.Add(partNumber) || existingCustomerNumbers.Contains(partNumber))
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}DuplicatePartNumber", row, partNumber));

                    entities.Add(new CustomerNumberEntity
                    {
                        PartNumberId = SnowFlakeSingle.Instance.NextId(),
                        PartNumber = partNumber,
                        CustomerCode = rowValues["CustomerCode"],
                        PartNameCn = rowValues["PartNameCn"],
                        PartNameEn = rowValues["PartNameEn"],
                        Specification = rowValues["Specification"],
                        Unit = rowValues["Unit"],
                        Status = ParseStatusText(rowValues["Status"]),
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                if (entities.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));

                await _db.BeginTranAsync();
                var count = await _customerNumberRepository.InsertCustomerNumberList(entities);
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
        /// 客户下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<CustomerDropDto>>> GetCustomerDrop()
        {
            try
            {
                var drop = await _customerNumberRepository.GetCustomerDrop();
                return Result<List<CustomerDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CustomerDropDto>>.Failure(500, ex.Message);
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
        /// 解析是否类文本为状态值（中文：是/否，英文：Yes/No，对应 1/0）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static int ParseStatusText(string value)
        {
            return value.Trim().ToUpperInvariant() switch
            {
                "是" or "YES" => 1,
                _ => 0,
            };
        }
    }
}
