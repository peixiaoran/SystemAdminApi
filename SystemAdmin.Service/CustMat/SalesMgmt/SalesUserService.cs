using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Commands;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Queries;
using SystemAdmin.Repository.CustMat.SalesMgmt;

namespace SystemAdmin.Service.CustMat.SalesMgmt
{
    public class SalesUserService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<SalesUserService> _logger;
        private readonly SqlSugarScope _db;
        private readonly SalesUserRepository _salesUserRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.Sales.SalesUser";

        public SalesUserService(CurrentUser loginuser, ILogger<SalesUserService> logger, SqlSugarScope db, SalesUserRepository salesUserRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _salesUserRepository = salesUserRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增业务人员
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertSalesUser(SalesUserUpsert upsert)
        {
            try
            {
                var salesUserId = long.Parse(upsert.SalesUserId);
                var exists = await _salesUserRepository.ExistsSalesUser(salesUserId);
                if (exists)
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}Duplicate"));

                await _db.BeginTranAsync();
                var entity = new SalesUserEntity()
                {
                    SalesUserId = salesUserId,
                    SalesDeptId = long.Parse(upsert.SalesDeptId),
                    SalesType = upsert.SalesType,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _salesUserRepository.InsertSalesUser(entity);
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
        /// 删除业务人员
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteSalesUser(string salesUserId)
        {
            try
            {
                await _db.BeginTranAsync();
                var count = await _salesUserRepository.DeleteSalesUser(long.Parse(salesUserId));
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
        /// 修改业务人员
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateSalesUser(SalesUserUpsert upsert)
        {
            try
            {
                var salesUserId = long.Parse(upsert.SalesUserId);
                var originalSalesUserId = string.IsNullOrEmpty(upsert.OriginalSalesUserId) ? salesUserId : long.Parse(upsert.OriginalSalesUserId);

                // 人员变更时，需校验新选择的人员是否已被配置为业务人员，避免重复配置
                if (salesUserId != originalSalesUserId)
                {
                    var exists = await _salesUserRepository.ExistsSalesUser(salesUserId);
                    if (exists)
                        return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}Duplicate"));
                }

                await _db.BeginTranAsync();
                var entity = new SalesUserEntity()
                {
                    SalesUserId = salesUserId,
                    SalesDeptId = long.Parse(upsert.SalesDeptId),
                    SalesType = upsert.SalesType,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _salesUserRepository.UpdateSalesUser(entity, originalSalesUserId);
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
        /// 查询业务人员实体
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<Result<SalesUserDto>> GetSalesUserEntity(string salesUserId)
        {
            try
            {
                var entity = await _salesUserRepository.GetSalesUserEntity(long.Parse(salesUserId));
                return Result<SalesUserDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<SalesUserDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询业务人员分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesUserDto>> GetSalesUserPage(GetSalesUserPage getPage)
        {
            try
            {
                return await _salesUserRepository.GetSalesUserPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<SalesUserDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 业务类型下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<SalesTypeDropDto>>> GetSalesTypeDrop()
        {
            try
            {
                var drop = await _salesUserRepository.GetSalesTypeDrop();
                return Result<List<SalesTypeDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<SalesTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 业务人员部门下拉分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<DepartmentDropDto>> GetSalesUserDepartmentPage()
        {
            try
            {
                return await _salesUserRepository.GetSalesUserDepartmentPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<DepartmentDropDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询用户分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesUserPageDto>> GetUserPage(GetUserInfoPage getPage)
        {
            try
            {
                return await _salesUserRepository.GetUserPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<SalesUserPageDto>.Failure(500, ex.Message);
            }
        }
    }
}
