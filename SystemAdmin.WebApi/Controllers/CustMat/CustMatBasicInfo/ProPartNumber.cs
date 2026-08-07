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
    public class ProPartNumber : ControllerBase
    {
        private readonly ProPartNumberService _proPartNumberService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.ProPartNumberExcel_";

        public ProPartNumber(ProPartNumberService proPartNumberService, LocalizationService localization)
        {
            _proPartNumberService = proPartNumberService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 新增料号信息")]
        public async Task<Result<int>> InsertProPartNumber([FromBody] ProPartNumberUpsert upsert)
        {
            return await _proPartNumberService.InsertProPartNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 删除料号信息")]
        public async Task<Result<int>> DeleteProPartNumber([FromForm] string partNumberId)
        {
            return await _proPartNumberService.DeleteProPartNumber(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 修改料号信息")]
        public async Task<Result<int>> UpdateProPartNumber([FromBody] ProPartNumberUpsert upsert)
        {
            return await _proPartNumberService.UpdateProPartNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息实体")]
        public async Task<Result<ProPartNumberDto>> GetProPartNumberEntity([FromForm] string partNumberId)
        {
            return await _proPartNumberService.GetProPartNumberEntity(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息分页")]
        public async Task<ResultPaged<ProPartNumberDto>> GetProPartNumberPage([FromBody] GetProPartNumberPage getPage)
        {
            return await _proPartNumberService.GetProPartNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号类型下拉")]
        public async Task<Result<List<PartTypeDropDto>>> GetPartTypeDrop()
        {
            return await _proPartNumberService.GetPartTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询物料分类下拉")]
        public async Task<Result<List<PartCategoryDropDto>>> GetCategoryDrop()
        {
            return await _proPartNumberService.GetCategoryDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询来源类型下拉")]
        public async Task<Result<List<PartSourceTypeDropDto>>> GetSourceTypeDrop()
        {
            return await _proPartNumberService.GetSourceTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导出料号信息Excel")]
        public async Task<IActionResult> GetProPartNumberExcel([FromBody] GetProPartNumberPage getPage)
        {
            var bytes = await _proPartNumberService.GetProPartNumberExcel(getPage);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localization.ReturnMsg($"{_thisExcel}SheetName") + ".xlsx");
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导出料号导入模板")]
        public async Task<IActionResult> GetProPartNumberTemplate()
        {
            var bytes = await _proPartNumberService.GetProPartNumberTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导入料号信息")]
        public async Task<Result<int>> ImportProPartNumber(IFormFile file)
        {
            return await _proPartNumberService.ImportProPartNumber(file);
        }
    }
}
