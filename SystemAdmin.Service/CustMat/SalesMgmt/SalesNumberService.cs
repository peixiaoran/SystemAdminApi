using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Commands;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Repository.CustMat.SalesMgmt;

namespace SystemAdmin.Service.CustMat.SalesMgmt
{
    public class SalesNumberService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<SalesNumberService> _logger;
        private readonly SqlSugarScope _db;
        private readonly SalesNumberRepository _salesNumberRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.Sales.SalesNumber";

        public SalesNumberService(CurrentUser loginuser, ILogger<SalesNumberService> logger, SqlSugarScope db, SalesNumberRepository salesNumberRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _salesNumberRepository = salesNumberRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增人员料号
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertSalesNumber(SalesNumberUpsert upsert)
        {
            try
            {
                var exists = await _salesNumberRepository.ExistsSalesNumber(upsert.PartNumber);
                if (exists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}Duplicate"));

                await _db.BeginTranAsync();
                var entity = new SalesNumberEntity()
                {
                    PartNumber = upsert.PartNumber,
                    SalesUserId = long.Parse(upsert.SalesUserId),
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _salesNumberRepository.InsertSalesNumber(entity);
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
        /// 删除人员料号
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteSalesNumber(string partNumber)
        {
            try
            {
                await _db.BeginTranAsync();
                var count = await _salesNumberRepository.DeleteSalesNumber(partNumber);
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
        /// 修改人员料号
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateSalesNumber(SalesNumberUpsert upsert)
        {
            try
            {
                var originalPartNumber = string.IsNullOrEmpty(upsert.OriginalPartNumber) ? upsert.PartNumber : upsert.OriginalPartNumber;

                // 料号变更时，需校验新选择的料号是否已配置过业务负责人，避免重复配置
                if (upsert.PartNumber != originalPartNumber)
                {
                    var exists = await _salesNumberRepository.ExistsSalesNumber(upsert.PartNumber);
                    if (exists)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}Duplicate"));
                }

                await _db.BeginTranAsync();
                var entity = new SalesNumberEntity()
                {
                    PartNumber = upsert.PartNumber,
                    SalesUserId = long.Parse(upsert.SalesUserId),
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _salesNumberRepository.UpdateSalesNumber(entity, originalPartNumber);
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
        /// 批量修改人员料号：按客户找出料号对照中的公司料号，统一绑定到指定业务负责人
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> BatchUpsertSalesNumber(SalesNumberBatchUpsert upsert)
        {
            try
            {
                var salesUserId = long.Parse(upsert.SalesUserId);
                var salesUserExists = await _salesNumberRepository.ExistsSalesUser(salesUserId);
                if (!salesUserExists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}SalesUserNotFound"));

                // 客户 → 客户料号 → 料号对照 → 公司料号
                var partNumbers = await _salesNumberRepository.GetCompanyPartNumbersByCustomer(long.Parse(upsert.CustomerId));
                if (partNumbers.Count == 0)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}NoCompanyPartNumber"));

                // 已配置过的走更新，未配置过的走新增
                var existingNumbers = await _salesNumberRepository.GetExistingPartNumbers(partNumbers);
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
                    count += await _salesNumberRepository.UpdateSalesUserByPartNumbers(existingNumbers, salesUserId, _loginuser.UserId, createdDate);
                }

                if (newNumbers.Count > 0)
                {
                    var entities = newNumbers.Select(partNumber => new SalesNumberEntity
                    {
                        PartNumber = partNumber,
                        SalesUserId = salesUserId,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = createdDate
                    }).ToList();
                    count += await _salesNumberRepository.InsertSalesNumberList(entities);
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
        /// 查询人员料号实体
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<Result<SalesNumberDto>> GetSalesNumberEntity(string partNumber)
        {
            try
            {
                var entity = await _salesNumberRepository.GetSalesNumberEntity(partNumber);
                return Result<SalesNumberDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<SalesNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询人员料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage)
        {
            try
            {
                return await _salesNumberRepository.GetSalesNumberPage(getPage);
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
                var drop = await _salesNumberRepository.GetSalesUserDrop();
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
        public async Task<Result<List<CompanyNumberDropDto>>> GetCompanyPartNumberDrop(string keyword)
        {
            try
            {
                var drop = await _salesNumberRepository.GetCompanyPartNumberDrop(keyword);
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
                var drop = await _salesNumberRepository.GetCustomerDrop();
                return Result<List<CustomerDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<CustomerDropDto>>.Failure(500, ex.Message);
            }
        }
    }
}
