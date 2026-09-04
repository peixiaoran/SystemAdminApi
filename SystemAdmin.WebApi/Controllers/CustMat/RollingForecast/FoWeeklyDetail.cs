using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.CommonSetup.Security;
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
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.RollingForecast.FoWeeklyDetailExcel_";

        public FoWeeklyDetail(FoWeeklyDetailService foWeeklyDetailService, LocalizationService localization)
        {
            _foWeeklyDetailService = foWeeklyDetailService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 查询版本分页")]
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage([FromBody] GetForecastVersionPage getPage)
        {
            return await _foWeeklyDetailService.GetForecastVersionPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 查询预测周明细")]
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyDetail([FromForm] string versionId)
        {
            return await _foWeeklyDetailService.GetFoWeeklyDetail(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 查询归档预测周明细")]
        public async Task<Result<FoWeeklyDetailDto>> GetFoWeeklyArchiveDetail([FromForm] string versionId)
        {
            return await _foWeeklyDetailService.GetFoWeeklyArchiveDetail(versionId);
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 导出预测明细模板")]
        public async Task<IActionResult> ExportFoWeeklyDetailTemplate([FromForm] string versionId)
        {
            var bytes = await _foWeeklyDetailService.GetFoWeeklyDetailTemplate(versionId);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}Template", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}Template", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 导出预测周明细")]
        public async Task<IActionResult> ExportFoWeeklyDetail([FromForm] string versionId)
        {
            var bytes = await _foWeeklyDetailService.GetFoWeeklyDetailExcel(versionId);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}Export", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}Export", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-料号滚动预测")]
        [EndpointSummary("[预测周明细] 导入预测周明细")]
        public async Task<Result<int>> ImportFoWeeklyDetail([FromForm] string versionId, IFormFile file)
        {
            return await _foWeeklyDetailService.ImportFoWeeklyDetail(versionId, file);
        }
    }
}
