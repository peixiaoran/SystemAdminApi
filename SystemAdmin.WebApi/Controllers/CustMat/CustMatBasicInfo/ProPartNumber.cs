using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Commands;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;
using SystemAdmin.Service.CustMat.CustMatBasicInfo;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.CustMatBasicInfo
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/CustMatBasicInfo/[controller]/[action]")]
    [ApiController]
    public class ProductionNumber : ControllerBase
    {
        private readonly ProductionNumberService _productionNumberService;
        public ProductionNumber(ProductionNumberService productionNumberService)
        {
            _productionNumberService = productionNumberService;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 新增料号信息")]
        public async Task<Result<int>> InsertPartNumberInfo([FromBody] ProductionNumberUpsert upsert)
        {
            return await _productionNumberService.InsertPartNumberInfo(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 删除料号信息")]
        public async Task<Result<int>> DeletePartNumberInfo([FromBody] ProductionNumberUpsert upsert)
        {
            return await _productionNumberService.DeletePartNumberInfo(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 修改料号信息")]
        public async Task<Result<int>> UpdatePartNumberInfo([FromBody] ProductionNumberUpsert upsert)
        {
            return await _productionNumberService.UpdatePartNumberInfo(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息实体")]
        public async Task<Result<ProductionNumberDto>> GetPartNumberInfoEntity([FromBody] GetProductionNumberEntity getEntity)
        {
            return await _productionNumberService.GetPartNumberInfoEntity(getEntity);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息分页")]
        public async Task<ResultPaged<ProductionNumberDto>> GetPartNumberInfoPage([FromBody] GetProductionNumberPage getPage)
        {
            return await _productionNumberService.GetPartNumberInfoPage(getPage);
        }
    }
}
