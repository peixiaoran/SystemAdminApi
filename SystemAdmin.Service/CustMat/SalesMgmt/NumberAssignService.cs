using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SqlSugar;
using System.Data;
using SystemAdmin.Common.Excel;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Commands;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Repository.CustMat.SalesMgmt;

namespace SystemAdmin.Service.CustMat.SalesMgmt
{
    public class NumberAssignService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<NumberAssignService> _logger;
        private readonly SqlSugarScope _db;
        private readonly NumberAssignRepository _numberAssignRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.Sales.SalesNumber";
        private readonly string _thisExcel = "CustMat.Sales.SalesNumberExcel_";

        public NumberAssignService(CurrentUser loginuser, ILogger<NumberAssignService> logger, SqlSugarScope db, NumberAssignRepository numberAssignRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _numberAssignRepository = numberAssignRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增料号分配
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertNumberAssign(NumberAssignUpsert upsert)
        {
            try
            {
                var exists = await _numberAssignRepository.ExistsNumberAssign(upsert.PartNumber);
                if (exists)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}Duplicate"));

                await _db.BeginTranAsync();
                var entity = new NumberAssignEntity()
                {
                    PartNumber = upsert.PartNumber,
                    SalesUserId = long.Parse(upsert.SalesUserId),
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _numberAssignRepository.InsertNumberAssign(entity);
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
        /// 删除料号分配
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteNumberAssign(string partNumber)
        {
            try
            {
                await _db.BeginTranAsync();
                var count = await _numberAssignRepository.DeleteNumberAssign(partNumber);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}DeleteSuccess"))
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
        /// 修改料号分配
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateNumberAssign(NumberAssignUpsert upsert)
        {
            try
            {
                var originalPartNumber = string.IsNullOrEmpty(upsert.OriginalPartNumber) ? upsert.PartNumber : upsert.OriginalPartNumber;

                // 料号变更时，需校验新选择的料号是否已配置过业务负责人，避免重复配置
                if (upsert.PartNumber != originalPartNumber)
                {
                    var exists = await _numberAssignRepository.ExistsNumberAssign(upsert.PartNumber);
                    if (exists)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}Duplicate"));
                }

                await _db.BeginTranAsync();
                var entity = new NumberAssignEntity()
                {
                    PartNumber = upsert.PartNumber,
                    SalesUserId = long.Parse(upsert.SalesUserId),
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _numberAssignRepository.UpdateNumberAssign(entity, originalPartNumber);
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
        /// 批量修改料号分配
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> BatchUpsertNumberAssign(NumberAssignBatchUpsert upsert)
        {
            try
            {
                var salesUserId = long.Parse(upsert.SalesUserId);
                var salesUserExists = await _numberAssignRepository.ExistsSalesUser(salesUserId);
                if (!salesUserExists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}SalesUserNotFound"));

                // 客户 → 客户料号 → 料号对照 → 公司料号
                var partNumbers = await _numberAssignRepository.GetCompanyPartNumbersByCustomer(long.Parse(upsert.CustomerId));
                if (partNumbers.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}NoCompanyPartNumber"));

                // 已配置过的走更新，未配置过的走新增
                var existingNumbers = await _numberAssignRepository.GetExistingPartNumbers(partNumbers);
                var newNumbers = partNumbers.Except(existingNumbers).ToList();
                var createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 仅补未配置的料号时，已配置的料号一条都动不了，直接提示无需补充
                if (upsert.UpdateMode != 1 && newNumbers.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}AllConfigured"));

                await _db.BeginTranAsync();

                var count = 0;
                // 仅 UpdateMode 为 1 时才覆盖已配置过的料号
                if (upsert.UpdateMode == 1 && existingNumbers.Count > 0)
                {
                    count += await _numberAssignRepository.UpdateSalesUserByPartNumbers(existingNumbers, salesUserId, _loginuser.UserId, createdDate);
                }

                if (newNumbers.Count > 0)
                {
                    var entities = newNumbers.Select(partNumber => new NumberAssignEntity
                    {
                        PartNumber = partNumber,
                        SalesUserId = salesUserId,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = createdDate
                    }).ToList();
                    count += await _numberAssignRepository.InsertNumberAssignList(entities);
                }

                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}BatchUpsertSuccess", count))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}BatchUpsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号分配实体
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<NumberAssignDto>> GetNumberAssignEntity(string partNumber)
        {
            try
            {
                var entity = await _numberAssignRepository.GetNumberAssignEntity(partNumber);
                return Result<NumberAssignDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<NumberAssignDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号分配分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<NumberAssignDto>> GetNumberAssignPage(GetNumberAssignPage getPage)
        {
            try
            {
                return await _numberAssignRepository.GetNumberAssignPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<NumberAssignDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 导出料号分配Excel表格
        /// </summary>
        /// <param name="getExcel"></param>
        /// <returns></returns>
        public async Task<byte[]> GetNumberAssignExcel(GetNumberAssignExcel getExcel)
        {
            try
            {
                DataTable dt = await _numberAssignRepository.GetNumberAssignExcel(getExcel);
                ExcelPackage.License.SetNonCommercialPersonal("Your Name");

                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add(_localization.ReturnMsg($"{_thisExcel}SalesNumber"));

                var columnConfigs = new Dictionary<string, ExcelColumnConfig>
                {
                    { "PartNumber", ExcelColumnConfig.Text },// 料号，防前导零消失
                    { "PartName", ExcelColumnConfig.Text },// 品名
                    { "UserNo", ExcelColumnConfig.Text },// 工号，防前导零消失
                    { "UserName", ExcelColumnConfig.Text },// 姓名
                };
                var headers = new Dictionary<string, string>
                {
                    { "PartNumber", _localization.ReturnMsg($"{_thisExcel}PartNumber") },
                    { "PartName", _localization.ReturnMsg($"{_thisExcel}PartName") },
                    { "UserNo", _localization.ReturnMsg($"{_thisExcel}UserNo") },
                    { "UserName", _localization.ReturnMsg($"{_thisExcel}UserName") },
                };

                ExcelStyleHelper.ApplyStandardStyle(ws, dt, headers, false, columnConfigs);
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
        /// 业务人员下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            try
            {
                var drop = await _numberAssignRepository.GetSalesUserDrop();
                return Result<List<SalesUserDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SalesUserDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 公司料号下拉
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<Result<List<CompanyNumberDropDto>>> GetCompanyNumberDrop(string keyword)
        {
            try
            {
                var drop = await _numberAssignRepository.GetCompanyNumberDrop(keyword);
                return Result<List<CompanyNumberDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CompanyNumberDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 客户信息下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<CustomerDropDto>>> GetCustomerDrop()
        {
            try
            {
                var drop = await _numberAssignRepository.GetCustomerDrop();
                return Result<List<CustomerDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CustomerDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 根据公司料号查询详情
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<CompanyNumberDetailDto>> GetPartNumberDetail(string partNumber)
        {
            try
            {
                var entity = await _numberAssignRepository.GetPartNumberDetail(partNumber);
                return Result<CompanyNumberDetailDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CompanyNumberDetailDto>.Failure(500, ex.Message);
            }
        }
    }
}
