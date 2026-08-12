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
    public class CustomerInfo : ControllerBase
    {
        private readonly CustomerInfoService _customerInfoService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.CustMatBasicInfo.CustomerInfoExcel_";

        public CustomerInfo(CustomerInfoService customerInfoService, LocalizationService localization)
        {
            _customerInfoService = customerInfoService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 新增客户信息")]
        public async Task<Result<int>> InsertCustomer([FromBody] CustomerInfoUpsert upsert)
        {
            return await _customerInfoService.InsertCustomer(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 删除客户信息")]
        public async Task<Result<int>> DeleteCustomer([FromForm] string customerId)
        {
            return await _customerInfoService.DeleteCustomer(customerId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 修改客户信息")]
        public async Task<Result<int>> UpdateCustomer([FromBody] CustomerInfoUpsert upsert)
        {
            return await _customerInfoService.UpdateCustomer(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 查询客户信息实体")]
        public async Task<Result<CustomerInfoDto>> GetCustomerEntity([FromForm] string customerId)
        {
            return await _customerInfoService.GetCustomerEntity(customerId);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 查询客户信息分页")]
        public async Task<ResultPaged<CustomerInfoDto>> GetCustomerPage([FromBody] GetCustomerPage getPage)
        {
            return await _customerInfoService.GetCustomerPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 导出客户信息Excel")]
        public async Task<IActionResult> GetCustomerExcel([FromBody] GetCustomerPage getPage)
        {
            var bytes = await _customerInfoService.GetCustomerExcel(getPage);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localization.ReturnMsg($"{_thisExcel}SheetName") + ".xlsx");
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 导出客户信息导入模板")]
        public async Task<IActionResult> GetCustomerTemplate()
        {
            var bytes = await _customerInfoService.GetCustomerTemplate();
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SheetName", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SheetName", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-相关基础信息")]
        [EndpointSummary("[客户信息] 导入客户信息")]
        public async Task<Result<int>> ImportCustomer(IFormFile file)
        {
            return await _customerInfoService.ImportCustomer(file);
        }
    }
}
