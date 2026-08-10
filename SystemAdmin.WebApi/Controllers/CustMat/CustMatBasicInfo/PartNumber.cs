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
    public class PartNumber : ControllerBase
    {
        private readonly PartNumberService _partNumberService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.PartNumberExcel_";

        public PartNumber(PartNumberService partNumberService, LocalizationService localization)
        {
            _partNumberService = partNumberService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 新增料号信息")]
        public async Task<Result<int>> InsertPartNumber([FromBody] PartNumberUpsert upsert)
        {
            return await _partNumberService.InsertPartNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 删除料号信息")]
        public async Task<Result<int>> DeletePartNumber([FromForm] string partNumberId)
        {
            return await _partNumberService.DeletePartNumber(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 修改料号信息")]
        public async Task<Result<int>> UpdatePartNumber([FromBody] PartNumberUpsert upsert)
        {
            return await _partNumberService.UpdatePartNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息实体")]
        public async Task<Result<PartNumberDto>> GetPartNumberEntity([FromForm] string partNumberId)
        {
            return await _partNumberService.GetPartNumberEntity(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号信息分页")]
        public async Task<ResultPaged<PartNumberDto>> GetPartNumberPage([FromBody] GetPartNumberPage getPage)
        {
            return await _partNumberService.GetPartNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询料号类型下拉")]
        public async Task<Result<List<PartTypeDropDto>>> GetPartTypeDrop()
        {
            return await _partNumberService.GetPartTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询物料分类下拉")]
        public async Task<Result<List<PartCategoryDropDto>>> GetCategoryDrop()
        {
            return await _partNumberService.GetCategoryDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 查询来源类型下拉")]
        public async Task<Result<List<PartSourceTypeDropDto>>> GetSourceTypeDrop()
        {
            return await _partNumberService.GetSourceTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导出料号信息Excel")]
        public async Task<IActionResult> GetPartNumberExcel([FromBody] GetPartNumberPage getPage)
        {
            var bytes = await _partNumberService.GetPartNumberExcel(getPage);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localization.ReturnMsg($"{_thisExcel}SheetName") + ".xlsx");
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导出料号导入模板")]
        public async Task<IActionResult> GetPartNumberTemplate()
        {
            var bytes = await _partNumberService.GetPartNumberTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[料号信息] 导入料号信息")]
        public async Task<Result<int>> ImportPartNumber(IFormFile file)
        {
            return await _partNumberService.ImportPartNumber(file);
        }
    }
}
