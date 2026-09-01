using Microsoft.AspNetCore.Mvc;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.SalesMgmt.Commands;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Service.CustMat.SalesMgmt;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.SalesMgmt
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/SalesMgmt/[controller]/[action]")]
    [ApiController]
    public class NumberAssign : ControllerBase
    {
        private readonly NumberAssignService _numberAssignService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.Sales.SalesNumberExcel_";

        public NumberAssign(NumberAssignService numberAssignService, LocalizationService localization)
        {
            _numberAssignService = numberAssignService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 新增料号分配")]
        public async Task<Result<int>> InsertNumberAssign([FromBody] NumberAssignUpsert upsert)
        {
            return await _numberAssignService.InsertNumberAssign(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 删除料号分配")]
        public async Task<Result<int>> DeleteNumberAssign([FromForm] string partNumber)
        {
            return await _numberAssignService.DeleteNumberAssign(partNumber);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 修改料号分配")]
        public async Task<Result<int>> UpdateNumberAssign([FromBody] NumberAssignUpsert upsert)
        {
            return await _numberAssignService.UpdateNumberAssign(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 批量修改料号分配")]
        public async Task<Result<int>> BatchUpsertNumberAssign([FromBody] NumberAssignBatchUpsert upsert)
        {
            return await _numberAssignService.BatchUpsertNumberAssign(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 查询料号分配实体")]
        public async Task<Result<NumberAssignDto>> GetNumberAssignEntity([FromForm] string partNumber)
        {
            return await _numberAssignService.GetNumberAssignEntity(partNumber);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 查询料号分配分页")]
        public async Task<ResultPaged<NumberAssignDto>> GetNumberAssignPage([FromBody] GetNumberAssignPage getPage)
        {
            return await _numberAssignService.GetNumberAssignPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 导出Excel表格")]
        public async Task<IActionResult> ExportNumberAssignExcel([FromBody] GetNumberAssignExcel getExcel)
        {
            var bytes = await _numberAssignService.GetNumberAssignExcel(getExcel);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SalesNumber", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SalesNumber", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 业务人员下拉")]
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            return await _numberAssignService.GetSalesUserDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 公司料号下拉")]
        public async Task<Result<List<CompanyNumberDropDto>>> GetCompanyNumberDrop([FromForm] string keyword)
        {
            return await _numberAssignService.GetCompanyNumberDrop(keyword);
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 客户信息下拉")]
        public async Task<Result<List<CustomerDropDto>>> GetCustomerDrop()
        {
            return await _numberAssignService.GetCustomerDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-业务料号管理")]
        [EndpointSummary("[料号分配] 根据料号查询详情")]
        public async Task<Result<CompanyNumberDetailDto>> GetPartNumberDetail([FromForm] string partNumber)
        {
            return await _numberAssignService.GetPartNumberDetail(partNumber);
        }
    }
}
