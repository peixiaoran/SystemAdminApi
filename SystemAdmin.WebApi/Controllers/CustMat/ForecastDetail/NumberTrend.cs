using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Service.CustMat.ForecastDetail;
using SystemAdmin.WebApi.Attributes;
using SalesUserDropDto = SystemAdmin.Model.CustMat.SalesMgmt.Dto.SalesUserDropDto;

namespace SystemAdmin.WebApi.Controllers.CustMat.ForecastDetail
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/ForecastDetail/[controller]/[action]")]
    [ApiController]
    public class NumberTrend : ControllerBase
    {
        private readonly NumberTrendService _numberTrendService;

        public NumberTrend(NumberTrendService numberTrendService)
        {
            _numberTrendService = numberTrendService;
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[料号趋势] 查询料号分页")]
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage([FromBody] GetSalesNumberPage getPage)
        {
            return await _numberTrendService.GetSalesNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[料号趋势] 业务人员下拉")]
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            return await _numberTrendService.GetSalesUserDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[料号趋势] 版本下拉框")]
        public async Task<Result<List<ForecastVersionDropDto>>> GetForecastVersionDrop()
        {
            return await _numberTrendService.GetForecastVersionDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-料号预测明细")]
        [EndpointSummary("[料号趋势] 料号版本用量")]
        public async Task<Result<NumberTrendDto>> GetNumberTrend([FromBody] GetNumberTrend query)
        {
            return await _numberTrendService.GetNumberTrend(query);
        }
    }
}
