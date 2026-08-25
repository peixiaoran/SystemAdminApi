using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.RollingForecast.Commands;
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
    public class ForecastVersion : ControllerBase
    {
        private readonly ForecastVersionService _forecastVersionService;
        public ForecastVersion(ForecastVersionService forecastVersionService)
        {
            _forecastVersionService = forecastVersionService;
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 新增预测版本")]
        public async Task<Result<int>> InsertForecastVersion([FromBody] ForecastVersionUpsert upsert)
        {
            return await _forecastVersionService.InsertForecastVersion(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 删除预测版本")]
        public async Task<Result<int>> DeleteForecastVersion([FromForm] string versionId)
        {
            return await _forecastVersionService.DeleteForecastVersion(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 解锁预测版本")]
        public async Task<Result<int>> UnlockForecastVersion([FromForm] string versionId)
        {
            return await _forecastVersionService.UnlockForecastVersion(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 锁定预测版本")]
        public async Task<Result<int>> LockForecastVersion([FromForm] string versionId)
        {
            return await _forecastVersionService.LockForecastVersion(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 修改预测版本")]
        public async Task<Result<int>> UpdateForecastVersion([FromBody] ForecastVersionUpsert upsert)
        {
            return await _forecastVersionService.UpdateForecastVersion(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 查询预测版本实体")]
        public async Task<Result<ForecastVersionDto>> GetForecastVersionEntity([FromForm] string versionId)
        {
            return await _forecastVersionService.GetForecastVersionEntity(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-滚动预测版本")]
        [EndpointSummary("[预测版本] 查询预测版本分页")]
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage([FromBody] GetForecastVersionPage getPage)
        {
            return await _forecastVersionService.GetForecastVersionPage(getPage);
        }
    }
}
