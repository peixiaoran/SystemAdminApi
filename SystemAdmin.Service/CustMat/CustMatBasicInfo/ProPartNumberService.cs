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
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.CustMat.CustMatBasicInfo;

namespace SystemAdmin.Service.CustMat.CustMatBasicInfo
{
    public class ProPartNumberService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<ProPartNumberService> _logger;
        private readonly SqlSugarScope _db;
        private readonly ProPartNumberRepository _proPartNumberRepository;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly string _this = "CustMat.CustMatBasicInfo.ProPartNumberInfo";
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.ProPartNumberExcel_";
        private readonly string _thisImport = "CustMat.CustMatBasicInfo.ProPartNumberImport_";

        // 导入/导出模板列（顺序即Excel列顺序），不含Id、创建、修改等系统字段
        private static readonly (string Key, bool Required)[] _templateColumns = new[]
        {
            ("PartNumber", true),
            ("PartNameCn", true),
            ("PartNameEn", true),
            ("Specification", true),
            ("PartType", true),
            ("Category", true),
            ("Model", true),
            ("DrawingNumber", true),
            ("Version", true),
            ("Material", true),
            ("BaseUnit", true),
            ("SourceType", true),
            ("Manufacturer", false),
            ("ManufacturerPartNumber", false),
            ("LotControl", true),
            ("Status", true),
            ("Remark", false),
        };

        public ProPartNumberService(CurrentUser loginuser, ILogger<ProPartNumberService> logger, SqlSugarScope db, ProPartNumberRepository proPartNumberRepository, LocalizationService localization, Language lang)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _proPartNumberRepository = proPartNumberRepository;
            _localization = localization;
            _lang = lang;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="proPartNumberUpsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertProPartNumber(ProPartNumberUpsert proPartNumberUpsert)
        {
            try
            {
                var exists = await _proPartNumberRepository.ExistsPartNumber(proPartNumberUpsert.PartNumber);
                if (exists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}PartNumberDuplicate", proPartNumberUpsert.PartNumber));

                await _db.BeginTranAsync();
                var entity = new ProPartNumberEntity()
                {
                    PartNumberId = SnowFlakeSingle.Instance.NextId(),
                    PartNumber = proPartNumberUpsert.PartNumber,
                    PartNameCn = proPartNumberUpsert.PartNameCn,
                    PartNameEn = proPartNumberUpsert.PartNameEn,
                    Specification = proPartNumberUpsert.Specification,
                    PartType = proPartNumberUpsert.PartType,
                    Category = proPartNumberUpsert.Category,
                    Model = proPartNumberUpsert.Model,
                    DrawingNumber = proPartNumberUpsert.DrawingNumber,
                    Version = proPartNumberUpsert.Version,
                    Material = proPartNumberUpsert.Material,
                    BaseUnit = proPartNumberUpsert.BaseUnit,
                    SourceType = proPartNumberUpsert.SourceType,
                    Manufacturer = proPartNumberUpsert.Manufacturer,
                    ManufacturerPartNumber = proPartNumberUpsert.ManufacturerPartNumber,
                    LotControl = proPartNumberUpsert.LotControl,
                    Status = proPartNumberUpsert.Status,
                    Remark = proPartNumberUpsert.Remark,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _proPartNumberRepository.InsertProPartNumber(entity);
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
        /// 删除料号信息
        /// </summary>
        /// <param name="proPartNumberId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteProPartNumber(string proPartNumberId)
        {
            try
            {
                await _db.BeginTranAsync();
                var delProPartNumberCount = await _proPartNumberRepository.DeleteProPartNumber(long.Parse(proPartNumberId));
                await _db.CommitTranAsync();

                return delProPartNumberCount >= 1
                        ? Result<int>.Ok(delProPartNumberCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
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
        /// 修改料号信息
        /// </summary>
        /// <param name="proPartNumberUpsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateProPartNumber(ProPartNumberUpsert proPartNumberUpsert)
        {
            try
            {
                await _db.BeginTranAsync();
                var entity = new ProPartNumberEntity()
                {
                    PartNumberId = long.Parse(proPartNumberUpsert.PartNumberId),
                    PartNumber = proPartNumberUpsert.PartNumber,
                    PartNameCn = proPartNumberUpsert.PartNameCn,
                    PartNameEn = proPartNumberUpsert.PartNameEn,
                    Specification = proPartNumberUpsert.Specification,
                    PartType = proPartNumberUpsert.PartType,
                    Category = proPartNumberUpsert.Category,
                    Model = proPartNumberUpsert.Model,
                    DrawingNumber = proPartNumberUpsert.DrawingNumber,
                    Version = proPartNumberUpsert.Version,
                    Material = proPartNumberUpsert.Material,
                    BaseUnit = proPartNumberUpsert.BaseUnit,
                    SourceType = proPartNumberUpsert.SourceType,
                    Manufacturer = proPartNumberUpsert.Manufacturer,
                    ManufacturerPartNumber = proPartNumberUpsert.ManufacturerPartNumber,
                    LotControl = proPartNumberUpsert.LotControl,
                    Status = proPartNumberUpsert.Status,
                    Remark = proPartNumberUpsert.Remark,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _proPartNumberRepository.UpdateProPartNumber(entity);
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
        /// 查询料号信息实体
        /// </summary>
        /// <param name="partNumberId"></param>
        /// <returns></returns>
        public async Task<Result<ProPartNumberDto>> GetProPartNumberEntity(string partNumberId)
        {
            try
            {
                var proPartNumberEntity = await _proPartNumberRepository.GetProPartNumberEntity(long.Parse(partNumberId));
                return Result<ProPartNumberDto>.Ok(proPartNumberEntity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<ProPartNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ProPartNumberDto>> GetProPartNumberPage(GetProPartNumberPage getPage)
        {
            try
            {
                return await _proPartNumberRepository.GetProPartNumberPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<ProPartNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出料号信息
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<byte[]> GetProPartNumberExcel(GetProPartNumberPage getPage)
        {
            try
            {
                var entities = await _proPartNumberRepository.GetProPartNumberList(getPage);

                var allDict = await _proPartNumberRepository.GetDictionaryByTypes(new List<string> { "PartType", "Category", "SourceType" });
                var partTypeDict = allDict.Where(dic => dic.DicType == "PartType")
                                           .ToDictionary(dic => dic.DicCode, dic => _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn);
                var categoryDict = allDict.Where(dic => dic.DicType == "Category")
                                           .ToDictionary(dic => dic.DicCode, dic => _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn);
                var sourceTypeDict = allDict.Where(dic => dic.DicType == "SourceType")
                                           .ToDictionary(dic => dic.DicCode, dic => _lang.Locale == "zh-CN" ? dic.DicNameCn : dic.DicNameEn);

                var yesText = _localization.ReturnMsg($"{_thisExcel}Yes");
                var noText = _localization.ReturnMsg($"{_thisExcel}No");

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
                    row["PartNumber"] = entity.PartNumber;
                    row["PartNameCn"] = entity.PartNameCn;
                    row["PartNameEn"] = entity.PartNameEn;
                    row["Specification"] = entity.Specification;
                    row["PartType"] = partTypeDict.GetValueOrDefault(entity.PartType, entity.PartType);
                    row["Category"] = categoryDict.GetValueOrDefault(entity.Category, entity.Category);
                    row["Model"] = entity.Model;
                    row["DrawingNumber"] = entity.DrawingNumber;
                    row["Version"] = entity.Version;
                    row["Material"] = entity.Material;
                    row["BaseUnit"] = entity.BaseUnit;
                    row["SourceType"] = sourceTypeDict.GetValueOrDefault(entity.SourceType, entity.SourceType);
                    row["Manufacturer"] = entity.Manufacturer;
                    row["ManufacturerPartNumber"] = entity.ManufacturerPartNumber;
                    row["LotControl"] = entity.LotControl ? yesText : noText;
                    row["Status"] = entity.Status ? yesText : noText;
                    row["Remark"] = entity.Remark;
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
        /// 查询料号类型下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<PartTypeDropDto>>> GetPartTypeDrop()
        {
            try
            {
                var drop = await _proPartNumberRepository.GetPartTypeDrop();
                return Result<List<PartTypeDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<PartTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询物料分类下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<PartCategoryDropDto>>> GetCategoryDrop()
        {
            try
            {
                var drop = await _proPartNumberRepository.GetCategoryDrop();
                return Result<List<PartCategoryDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<PartCategoryDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询来源类型下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<PartSourceTypeDropDto>>> GetSourceTypeDrop()
        {
            try
            {
                var drop = await _proPartNumberRepository.GetSourceTypeDrop();
                return Result<List<PartSourceTypeDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<PartSourceTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出料号导入模板（不含Id、创建、修改等字段）
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetProPartNumberTemplate()
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
        /// 导入料号信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Result<int>> ImportProPartNumber(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}FileEmpty"));

                ExcelPackage.License.SetNonCommercialPersonal("Your Name");

                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);

                // 是否有Sheet表
                if (package.Workbook.Worksheets.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoWorksheet"));

                var ws = package.Workbook.Worksheets[0];

                // 是否至少有一条数据
                if (ws.Dimension == null || ws.Dimension.End.Row < 2)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));

                // 列数量是否对得上
                var expectedColCount = _templateColumns.Length;
                var actualColCount = ws.Dimension.End.Column;
                if (actualColCount != expectedColCount)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}ColumnCountMismatch"));

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
                    if (actualHeaders[i] != zhHeaders[i] && actualHeaders[i] != enHeaders[i])
                    {
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}HeaderMismatch", i + 1, $"{zhHeaders[i]}/{enHeaders[i]}", actualHeaders[i]));
                    }
                }

                // 三个下拉字段对应的字典数据一次性查询出来（编码、中文名称、英文名称均可匹配）
                var allDict = await _proPartNumberRepository.GetDictionaryByTypes(new List<string> { "PartType", "Category", "SourceType" });
                var partTypeDict = allDict.Where(dic => dic.DicType == "PartType").ToList();
                var categoryDict = allDict.Where(dic => dic.DicType == "Category").ToList();
                var sourceTypeDict = allDict.Where(dic => dic.DicType == "SourceType").ToList();

                // 一次性查询文件中所有料号在数据库中是否已存在，避免逐行查询
                var partNumberColIndex = Array.FindIndex(_templateColumns, c => c.Key == "PartNumber") + 1;
                var filePartNumbers = new List<string>();
                for (var row = 2; row <= ws.Dimension.End.Row; row++)
                {
                    var text = ws.Cells[row, partNumberColIndex].Text?.Trim();
                    if (!string.IsNullOrEmpty(text))
                        filePartNumbers.Add(text);
                }
                var existingPartNumbers = new HashSet<string>(
                    await _proPartNumberRepository.GetExistingPartNumbers(filePartNumbers),
                    StringComparer.OrdinalIgnoreCase);

                var entities = new List<ProPartNumberEntity>();
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
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}RequiredFieldEmpty", row, fieldLabel));
                    }

                    // 三个下拉框字段是否有匹配的字典值
                    var partTypeCode = MatchDictCode(partTypeDict, rowValues["PartType"]);
                    if (partTypeCode == null)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}InvalidDictValue", row, _localization.ReturnMsg($"{_thisExcel}PartType"), rowValues["PartType"]));

                    var categoryCode = MatchDictCode(categoryDict, rowValues["Category"]);
                    if (categoryCode == null)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}InvalidDictValue", row, _localization.ReturnMsg($"{_thisExcel}Category"), rowValues["Category"]));

                    var sourceTypeCode = MatchDictCode(sourceTypeDict, rowValues["SourceType"]);
                    if (sourceTypeCode == null)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}InvalidDictValue", row, _localization.ReturnMsg($"{_thisExcel}SourceType"), rowValues["SourceType"]));

                    // 料号是否重复：文件内重复 或 数据库中已存在
                    var partNumber = rowValues["PartNumber"];
                    if (!seenPartNumbers.Add(partNumber) || existingPartNumbers.Contains(partNumber))
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}DuplicatePartNumber", row, partNumber));

                    entities.Add(new ProPartNumberEntity
                    {
                        PartNumberId = SnowFlakeSingle.Instance.NextId(),
                        PartNumber = rowValues["PartNumber"],
                        PartNameCn = rowValues["PartNameCn"],
                        PartNameEn = rowValues["PartNameEn"],
                        Specification = rowValues["Specification"],
                        PartType = partTypeCode,
                        Category = categoryCode,
                        Model = rowValues["Model"],
                        DrawingNumber = rowValues["DrawingNumber"],
                        Version = rowValues["Version"],
                        Material = rowValues["Material"],
                        BaseUnit = rowValues["BaseUnit"],
                        SourceType = sourceTypeCode,
                        Manufacturer = string.IsNullOrEmpty(rowValues["Manufacturer"]) ? null : rowValues["Manufacturer"],
                        ManufacturerPartNumber = string.IsNullOrEmpty(rowValues["ManufacturerPartNumber"]) ? null : rowValues["ManufacturerPartNumber"],
                        LotControl = ParseBoolText(rowValues["LotControl"]),
                        Status = ParseBoolText(rowValues["Status"]),
                        Remark = string.IsNullOrEmpty(rowValues["Remark"]) ? null : rowValues["Remark"],
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                if (entities.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_thisImport}NoData"));

                await _db.BeginTranAsync();
                var count = await _proPartNumberRepository.InsertProPartNumberList(entities);
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
        /// 按编码/中文名称/英文名称匹配字典编码
        /// </summary>
        /// <param name="dictList"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string? MatchDictCode(List<DictionaryInfoEntity> dictList, string value)
        {
            var matched = dictList.FirstOrDefault(dic =>
                string.Equals(dic.DicCode, value, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dic.DicNameCn, value, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dic.DicNameEn, value, StringComparison.OrdinalIgnoreCase));
            return matched?.DicCode;
        }

        /// <summary>
        /// 解析是否类文本为bool（中文：是/否，英文：Yes/No）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool ParseBoolText(string value)
        {
            return value.Trim().ToUpperInvariant() switch
            {
                "是" or "YES" => true,
                _ => false,
            };
        }
    }
}
