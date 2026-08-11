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
    public class CompanyNumber : ControllerBase
    {
        private readonly CompanyNumberService _companyNumberService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.CompanyNumberExcel_";

        public CompanyNumber(CompanyNumberService companyNumberService, LocalizationService localization)
        {
            _companyNumberService = companyNumberService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 新增公司料号信息")]
        public async Task<Result<int>> InsertCompanyNumber([FromBody] CompanyNumberUpsert upsert)
        {
            return await _companyNumberService.InsertCompanyNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 删除公司料号信息")]
        public async Task<Result<int>> DeleteCompanyNumber([FromForm] string partNumberId)
        {
            return await _companyNumberService.DeleteCompanyNumber(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 修改公司料号信息")]
        public async Task<Result<int>> UpdateCompanyNumber([FromBody] CompanyNumberUpsert upsert)
        {
            return await _companyNumberService.UpdateCompanyNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 查询公司料号信息实体")]
        public async Task<Result<CompanyNumberDto>> GetCompanyNumberEntity([FromForm] string partNumberIdss)
        {
            return await _companyNumberService.GetCompanyNumberEntity(partNumberIdss);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 查询公司料号信息分页")]
        public async Task<ResultPaged<CompanyNumberDto>> GetCompanyNumberPage([FromBody] GetCompanyNumberPage getPage)
        {
            return await _companyNumberService.GetCompanyNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 查询料号类型下拉")]
        public async Task<Result<List<PartTypeDropDto>>> GetPartTypeDrop()
        {
            return await _companyNumberService.GetPartTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 查询物料分类下拉")]
        public async Task<Result<List<PartCategoryDropDto>>> GetCategoryDrop()
        {
            return await _companyNumberService.GetCategoryDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 查询来源类型下拉")]
        public async Task<Result<List<PartSourceTypeDropDto>>> GetSourceTypeDrop()
        {
            return await _companyNumberService.GetSourceTypeDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 导出公司料号信息Excel")]
        public async Task<IActionResult> GetCompanyNumberExcel([FromBody] GetCompanyNumberPage getPage)
        {
            var bytes = await _companyNumberService.GetCompanyNumberExcel(getPage);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localization.ReturnMsg($"{_thisExcel}SheetName") + ".xlsx");
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 导出公司料号导入模板")]
        public async Task<IActionResult> GetCompanyNumberTemplate()
        {
            var bytes = await _companyNumberService.GetCompanyNumberTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[公司料号信息] 导入公司料号信息")]
        public async Task<Result<int>> ImportCompanyNumber(IFormFile file)
        {
            return await _companyNumberService.ImportCompanyNumber(file);
        }
    }
}
