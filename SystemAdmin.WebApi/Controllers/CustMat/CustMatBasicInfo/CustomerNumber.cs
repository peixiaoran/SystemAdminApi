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
    public class CustomerNumber : ControllerBase
    {
        private readonly CustomerNumberService _customerNumberService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.CustomerNumberExcel_";

        public CustomerNumber(CustomerNumberService customerNumberService, LocalizationService localization)
        {
            _customerNumberService = customerNumberService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 新增客户料号")]
        public async Task<Result<int>> InsertCustomerNumber([FromBody] CustomerNumberUpsert upsert)
        {
            return await _customerNumberService.InsertCustomerNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 删除客户料号")]
        public async Task<Result<int>> DeleteCustomerNumber([FromForm] string partNumberId)
        {
            return await _customerNumberService.DeleteCustomerNumber(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 修改客户料号")]
        public async Task<Result<int>> UpdateCustomerNumber([FromBody] CustomerNumberUpsert upsert)
        {
            return await _customerNumberService.UpdateCustomerNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 查询客户料号实体")]
        public async Task<Result<CustomerNumberDto>> GetCustomerNumberEntity([FromForm] string partNumberId)
        {
            return await _customerNumberService.GetCustomerNumberEntity(partNumberId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 查询客户料号分页")]
        public async Task<ResultPaged<CustomerNumberDto>> GetCustomerNumberPage([FromBody] GetCustomerNumberPage getPage)
        {
            return await _customerNumberService.GetCustomerNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 导出客户料号Excel")]
        public async Task<IActionResult> GetCustomerNumberExcel([FromBody] GetCustomerNumberPage getPage)
        {
            var bytes = await _customerNumberService.GetCustomerNumberExcel(getPage);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}DataSheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}DataSheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 导出客户料号模板")]
        public async Task<IActionResult> GetCustomerNumberTemplate()
        {
            var bytes = await _customerNumberService.GetCustomerNumberTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 导入客户料号")]
        public async Task<Result<int>> ImportCustomerNumber(IFormFile file)
        {
            return await _customerNumberService.ImportCustomerNumber(file);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户料号] 客户下拉")]
        public async Task<Result<List<CustomerDropDto>>> GetCustomerDrop()
        {
            return await _customerNumberService.GetCustomerDrop();
        }
    }
}
