using Microsoft.Extensions.Logging;
using SqlSugar;
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
    }
}
