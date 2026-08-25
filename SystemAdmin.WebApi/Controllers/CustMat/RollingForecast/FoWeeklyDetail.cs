using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Service.CustMat.RollingForecast;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.RollingForecast
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/RollingForecast/[controller]/[action]")]
    [ApiController]
    public class FoWeeklyDetail : ControllerBase
    {
        private readonly FoWeeklyDetailService _foWeeklyDetailService;
        public FoWeeklyDetail(FoWeeklyDetailService foWeeklyDetailService)
        {
            _foWeeklyDetailService = foWeeklyDetailService;
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测周明细")]
        [EndpointSummary("[预测周明细] 查询预测版本分页")]
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage([FromBody] GetForecastVersionPage getPage)
        {
            return await _foWeeklyDetailService.GetForecastVersionPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测周明细")]
        [EndpointSummary("[预测周明细] 查询预测周明细")]
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyDetail([FromForm] string versionId)
        {
            return await _foWeeklyDetailService.GetFoWeeklyDetail(versionId);
        }
    }
}
