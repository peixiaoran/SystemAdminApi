using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.CommonSetup.Security;
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
    public class NumberMapping : ControllerBase
    {
        private readonly NumberMappingService _numberMappingService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.NumberMappingExcel_";

        public NumberMapping(NumberMappingService numberMappingService, LocalizationService localization)
        {
            _numberMappingService = numberMappingService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 新增对照信息")]
        public async Task<Result<int>> InsertNumberMapping([FromBody] NumberMappingUpsert upsert)
        {
            return await _numberMappingService.InsertNumberMapping(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 删除对照信息")]
        public async Task<Result<int>> DeleteNumberMapping([FromForm] string mappingId)
        {
            return await _numberMappingService.DeleteNumberMapping(mappingId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 修改对照信息")]
        public async Task<Result<int>> UpdateNumberMapping([FromBody] NumberMappingUpsert upsert)
        {
            return await _numberMappingService.UpdateNumberMapping(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 查询对照实体")]
        public async Task<Result<NumberMappingDto>> GetNumberMappingEntity([FromForm] string mappingId)
        {
            return await _numberMappingService.GetNumberMappingEntity(mappingId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 查询对照分页")]
        public async Task<ResultPaged<NumberMappingDto>> GetNumberMappingPage([FromBody] GetNumberMappingPage getPage)
        {
            return await _numberMappingService.GetNumberMappingPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 导出对照Excel")]
        public async Task<IActionResult> GetNumberMappingExcel([FromBody] GetNumberMappingPage getPage)
        {
            var bytes = await _numberMappingService.GetNumberMappingExcel(getPage);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}DataSheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}DataSheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 导出对照导入模板")]
        public async Task<IActionResult> GetNumberMappingTemplate()
        {
            var bytes = await _numberMappingService.GetNumberMappingTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 导入对照信息")]
        public async Task<Result<int>> ImportNumberMapping(IFormFile file)
        {
            return await _numberMappingService.ImportNumberMapping(file);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 客户料号下拉")]
        public async Task<Result<List<CustomerPartNumberDropDto>>> GetCustomerPartNumberDrop([FromForm] string keyword)
        {
            return await _numberMappingService.GetCustomerPartNumberDrop(keyword);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号对照] 公司料号下拉")]
        public async Task<Result<List<CompanyPartNumberDropDto>>> GetCompanyPartNumberDrop([FromForm] string keyword)
        {
            return await _numberMappingService.GetCompanyPartNumberDrop(keyword);
        }
    }
}
