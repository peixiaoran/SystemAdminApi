using Microsoft.AspNetCore.Http;
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
    public class ProductionNumberService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<ProductionNumberService> _logger;
        private readonly SqlSugarScope _db;
        private readonly ProductionNumberRepository _productionNumberRepository;
        private readonly LocalizationService _localization;
        private readonly string _this = "CustMat_CustMatBasicInfo_PartNumberInfo_";

        public ProductionNumberService(CurrentUser loginuser, ILogger<ProductionNumberService> logger, SqlSugarScope db, ProductionNumberRepository productionNumberRepository, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _productionNumberRepository = productionNumberRepository;
            _localization = localization;
        }

        /// <summary>
        /// 新增料号信息
        /// </summary>
        /// <param name="productionNumberUpsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertPartNumberInfo(ProductionNumberUpsert productionNumberUpsert)
        {
            try
            {
                await _db.BeginTranAsync();
                var entity = new ProductionNumberEntity()
                {
                    PartNumberId = SnowFlakeSingle.Instance.NextId(),
                    PartNumberNo = productionNumberUpsert.PartNumberNo,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _productionNumberRepository.InsertPartNumberInfo(entity);
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
        /// <param name="productionNumberUpsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeletePartNumberInfo(ProductionNumberUpsert productionNumberUpsert)
        {
            try
            {
                await _db.BeginTranAsync();
                var delPartNumberCount = await _productionNumberRepository.DeletePartNumberInfo(long.Parse(productionNumberUpsert.PartNumberId));
                await _db.CommitTranAsync();

                return delPartNumberCount >= 1
                        ? Result<int>.Ok(delPartNumberCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
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
        /// <param name="productionNumberUpsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdatePartNumberInfo(ProductionNumberUpsert productionNumberUpsert)
        {
            try
            {
                await _db.BeginTranAsync();
                var entity = new ProductionNumberEntity()
                {
                    PartNumberId = long.Parse(productionNumberUpsert.PartNumberId),
                    PartNumberNo = productionNumberUpsert.PartNumberNo,
                    ProductName = productionNumberUpsert.ProductName,
                    Specifications = productionNumberUpsert.Specifications,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var count = await _productionNumberRepository.UpdatePartNumberInfo(entity);
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
        /// <param name="getEntity"></param>
        /// <returns></returns>
        public async Task<Result<ProductionNumberDto>> GetPartNumberInfoEntity(GetProductionNumberEntity getEntity)
        {
            try
            {
                var productionNumberEntity = await _productionNumberRepository.GetPartNumberInfoEntity(long.Parse(getEntity.PartNumberId));
                return Result<ProductionNumberDto>.Ok(productionNumberEntity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<ProductionNumberDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询料号信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ProductionNumberDto>> GetPartNumberInfoPage(GetProductionNumberPage getPage)
        {
            try
            {
                return await _productionNumberRepository.GetPartNumberInfoPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<ProductionNumberDto>.Failure(500, ex.Message);
            }
        }
    }
}
