using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Service.CustMat.ForecastDetail;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.ForecastDetail
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/ForecastDetail/[controller]/[action]")]
    [ApiController]
    public class FoWeeklyDetail : ControllerBase
    {
        private readonly FoWeeklyDetailService _foWeeklyDetailService;

        public FoWeeklyDetail(FoWeeklyDetailService foWeeklyDetailService)
        {
            _foWeeklyDetailService = foWeeklyDetailService;
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[预测周明细] 查询版本分页")]
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage([FromBody] GetForecastVersionPage getPage)
        {
            return await _foWeeklyDetailService.GetForecastVersionPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[预测周明细] 业务人员下拉")]
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            return await _foWeeklyDetailService.GetSalesUserDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[预测周明细] 查询预测周明细")]
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyDetail([FromForm] string versionId, [FromForm] string? salesUserId)
        {
            return await _foWeeklyDetailService.GetFoWeeklyDetail(versionId, salesUserId);
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[预测周明细] 查询归档预测周明细")]
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyArchiveDetail([FromForm] string versionId, [FromForm] string? salesUserId)
        {
            return await _foWeeklyDetailService.GetFoWeeklyArchiveDetail(versionId, salesUserId);
        }
    }
}
