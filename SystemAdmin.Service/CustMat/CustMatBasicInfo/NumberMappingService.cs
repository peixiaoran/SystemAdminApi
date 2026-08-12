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
    public class NumberMappingService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<NumberMappingService> _logger;
        private readonly SqlSugarScope _db;
        private readonly NumberMappingRepository _numberMappingRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat.CustMatBasicInfo.NumberMapping";

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
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}MappingOverlap", upsert.CompanyPartNumber));

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
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}MappingOverlap", upsert.CompanyPartNumber));

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
    }
}
