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
    public class SalesNumber : ControllerBase
    {
        private readonly SalesNumberService _salesNumberService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "CustMat.Sales.SalesNumberExcel_";

        public SalesNumber(SalesNumberService salesNumberService, LocalizationService localization)
        {
            _salesNumberService = salesNumberService;
            _localization = localization;
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 新增人员料号")]
        public async Task<Result<int>> InsertSalesNumber([FromBody] SalesNumberUpsert upsert)
        {
            return await _salesNumberService.InsertSalesNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 删除人员料号")]
        public async Task<Result<int>> DeleteSalesNumber([FromForm] string partNumber)
        {
            return await _salesNumberService.DeleteSalesNumber(partNumber);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 修改人员料号")]
        public async Task<Result<int>> UpdateSalesNumber([FromBody] SalesNumberUpsert upsert)
        {
            return await _salesNumberService.UpdateSalesNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 批量修改人员料号")]
        public async Task<Result<int>> BatchUpsertSalesNumber([FromBody] SalesNumberBatchUpsert upsert)
        {
            return await _salesNumberService.BatchUpsertSalesNumber(upsert);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 查询人员料号实体")]
        public async Task<Result<SalesNumberDto>> GetSalesNumberEntity([FromForm] string partNumber)
        {
            return await _salesNumberService.GetSalesNumberEntity(partNumber);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 查询人员料号分页")]
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage([FromBody] GetSalesNumberPage getPage)
        {
            return await _salesNumberService.GetSalesNumberPage(getPage);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 导出Excel表格")]
        public async Task<IActionResult> ExportSalesNumberExcel([FromBody] GetSalesNumberExcel getExcel)
        {
            var bytes = await _salesNumberService.GetSalesNumberExcel(getExcel);
            var fileName = $"{_localization.ReturnMsg($"{_thisExcel}SalesNumber", "zh-CN")} {_localization.ReturnMsg($"{_thisExcel}SalesNumber", "en-US")}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 业务人员下拉")]
        public async Task<Result<List<SalesUserDropDto>>> GetSalesUserDrop()
        {
            return await _salesNumberService.GetSalesUserDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 公司料号下拉")]
        public async Task<Result<List<CompanyNumberDropDto>>> GetCompanyNumberDrop([FromForm] string keyword)
        {
            return await _salesNumberService.GetCompanyNumberDrop(keyword);
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 客户信息下拉")]
        public async Task<Result<List<CustomerDropDto>>> GetCustomerDrop()
        {
            return await _salesNumberService.GetCustomerDrop();
        }

        [HttpPost]
        [Tags("客户生产订单-业务人员料号")]
        [EndpointSummary("[人员料号] 根据料号查询详情")]
        public async Task<Result<CompanyNumberDetailDto>> GetPartNumberDetail([FromForm] string partNumber)
        {
            return await _salesNumberService.GetPartNumberDetail(partNumber);
        }
    }
}
