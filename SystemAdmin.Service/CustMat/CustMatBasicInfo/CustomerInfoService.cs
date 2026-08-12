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
    public class CustomerInfoService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<CustomerInfoService> _logger;
        private readonly SqlSugarScope _db;
        private readonly CustomerInfoRepository _customerInfoRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.CustMatBasicInfo.CustomerInfo";
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.CustomerInfoExcel_";
        private readonly string _thisImport = "CustMat.CustMatBasicInfo.CustomerInfoImport_";

        // 导入/导出模板列（顺序即Excel列顺序），不含Id、创建、修改等系统字段
        private static readonly (string Key, bool Required)[] _templateColumns = new[]
        {
            ("CustomerCode", true),
            ("CustomerNameCn", true),
            ("CustomerNameEn", true),
            ("Description", false),
        };

        public CustomerInfoService(CurrentUser loginuser, ILogger<CustomerInfoService> logger, SqlSugarScope db, CustomerInfoRepository customerInfoRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _customerInfoRepository = customerInfoRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增客户信息
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertCustomer(CustomerInfoUpsert upsert)
        {
            try
            {
                var entity = new CustomerInfoEntity()
                {
                    CustomerId = SnowFlakeSingle.Instance.NextId(),
                    CustomerCode = upsert.CustomerCode,
                    CustomerNameCn = upsert.CustomerNameCn,
                    CustomerNameEn = upsert.CustomerNameEn,
                    Description = upsert.Description,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await _db.BeginTranAsync();
                var count = await _customerInfoRepository.InsertCustomer(entity);
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
        /// 删除客户信息
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteCustomer(string customerId)
        {
            try
            {
                await _db.BeginTranAsync();
                var delCustomerCount = await _customerInfoRepository.DeleteCustomer(long.Parse(customerId));
                await _db.CommitTranAsync();

                return delCustomerCount >= 1
                        ? Result<int>.Ok(delCustomerCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
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
        /// 修改客户信息
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateCustomer(CustomerInfoUpsert upsert)
        {
            try
            {
                var entity = new CustomerInfoEntity()
                {
                    CustomerId = long.Parse(upsert.CustomerId),
                    CustomerCode = upsert.CustomerCode,
                    CustomerNameCn = upsert.CustomerNameCn,
                    CustomerNameEn = upsert.CustomerNameEn,
                    Description = upsert.Description,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await _db.BeginTranAsync();
                var count = await _customerInfoRepository.UpdateCustomer(entity);
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
        /// 查询客户信息实体
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Result<CustomerInfoDto>> GetCustomerEntity(string customerId)
        {
            try
            {
                var customerInfoEntity = await _customerInfoRepository.GetCustomerEntity(long.Parse(customerId));
                return Result<CustomerInfoDto>.Ok(customerInfoEntity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CustomerInfoDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询客户信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<CustomerInfoDto>> GetCustomerPage(GetCustomerPage getPage)
        {
            try
            {
                return await _customerInfoRepository.GetCustomerPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<CustomerInfoDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出客户信息
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<byte[]> GetCustomerExcel(GetCustomerPage getPage)
        {
            try
            {
                var entities = await _customerInfoRepository.GetCustomerList(getPage);

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}SheetName"));

                var dt = new DataTable();
                foreach (var col in _templateColumns)
                {
                    dt.Columns.Add(col.Key, typeof(string));
                }

                foreach (var entity in entities)
                {
                    var row = dt.NewRow();
                    row["CustomerCode"] = entity.CustomerCode;
                    row["CustomerNameCn"] = entity.CustomerNameCn;
                    row["CustomerNameEn"] = entity.CustomerNameEn;
                    row["Description"] = entity.Description;
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
        /// 导出客户信息导入模板（不含Id、创建、修改等字段）
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetCustomerTemplate()
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
        /// 导入客户信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Result<int>> ImportCustomer(IFormFile file)
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
                    // 中文名、英文名、英文字段名均可，且忽略大小写与空格差异（如 Customer Code / CustomerCode）
                    var accepted = new[] { zhHeaders[i], enHeaders[i], _templateColumns[i].Key };
                    if (!accepted.Any(header => NormalizeHeader(header) == NormalizeHeader(actualHeaders[i])))
                    {
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", i + 1, $"{zhHeaders[i]}/{enHeaders[i]}", actualHeaders[i]));
                    }
                }

                // 一次性查询文件中所有客户编码在数据库中是否已存在，避免逐行查询
                var customerCodeColIndex = Array.FindIndex(_templateColumns, c => c.Key == "CustomerCode") + 1;
                var fileCustomerCodes = new List<string>();
                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var text = ws.Cells[row, customerCodeColIndex].Text?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        fileCustomerCodes.Add(text);
                }
                var existingCustomerCodes = new HashSet<string>(
                    await _customerInfoRepository.GetExistingCustomerCodes(fileCustomerCodes),
                    StringComparer.OrdinalIgnoreCase);

                var entities = new List<CustomerInfoEntity>();
                var seenCustomerCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

                    // 客户编码是否重复：文件内重复 或 数据库中已存在
                    var customerCode = rowValues["CustomerCode"];
                    if (!seenCustomerCodes.Add(customerCode) || existingCustomerCodes.Contains(customerCode))
                        return Result<int>.Failure(400, _localization.ReturnMsg($"{_thisImport}DuplicateCustomerCode", row, customerCode));

                    entities.Add(new CustomerInfoEntity
                    {
                        CustomerId = SnowFlakeSingle.Instance.NextId(),
                        CustomerCode = customerCode,
                        CustomerNameCn = rowValues["CustomerNameCn"],
                        CustomerNameEn = rowValues["CustomerNameEn"],
                        Description = rowValues["Description"],
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                if (entities.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));

                await _db.BeginTranAsync();
                var count = await _customerInfoRepository.InsertCustomerList(entities);
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
    }
}
