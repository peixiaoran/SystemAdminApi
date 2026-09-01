using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Service.CustMat.SalesMgmt;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.SalesMgmt
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/SalesMgmt/[controller]/[action]")]
    [ApiController]
    public class SalesNumber : ControllerBase
    {
        private readonly SalesNumberService _salesNumberService;

        public SalesNumber(SalesNumberService salesNumberService)
        {
            _salesNumberService = salesNumberService;
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[负责料号] 查询业务料号分页")]
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage([FromBody] GetSalesNumberPage getPage)
        {
            return await _salesNumberService.GetSalesNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[负责料号] 根据料号查询详情")]
        public async Task<Result<CompanyNumberDetailDto>> GetPartNumberDetail([FromForm] string partNumber)
        {
            return await _salesNumberService.GetPartNumberDetail(partNumber);
        }
    }
}
