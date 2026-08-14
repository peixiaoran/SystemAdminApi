using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.CustMat.Sales.Commands;
using SystemAdmin.Model.CustMat.Sales.Dto;
using SystemAdmin.Model.CustMat.Sales.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Queries;
using SystemAdmin.Service.CustMat.Sales;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.CustMat.Sales
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/CustMat/Sales/[controller]/[action]")]
    [ApiController]
    public class SalesUser : ControllerBase
    {
        private readonly SalesUserService _salesUserService;

        public SalesUser(SalesUserService salesUserService)
        {
            _salesUserService = salesUserService;
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 新增业务人员")]
        public async Task<Result<int>> InsertSalesUser([FromBody] SalesUserUpsert upsert)
        {
            return await _salesUserService.InsertSalesUser(upsert);
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 删除业务人员")]
        public async Task<Result<int>> DeleteSalesUser([FromForm] string salesUserId)
        {
            return await _salesUserService.DeleteSalesUser(salesUserId);
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 修改业务人员")]
        public async Task<Result<int>> UpdateSalesUser([FromBody] SalesUserUpsert upsert)
        {
            return await _salesUserService.UpdateSalesUser(upsert);
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 查询业务人员实体")]
        public async Task<Result<SalesUserDto>> GetSalesUserEntity([FromForm] string salesUserId)
        {
            return await _salesUserService.GetSalesUserEntity(salesUserId);
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 查询业务人员分页")]
        public async Task<ResultPaged<SalesUserDto>> GetSalesUserPage([FromBody] GetSalesUserPage getPage)
        {
            return await _salesUserService.GetSalesUserPage(getPage);
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 业务类型下拉")]
        public async Task<Result<List<SalesTypeDropDto>>> GetSalesTypeDrop()
        {
            return await _salesUserService.GetSalesTypeDrop();
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 部门树下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _salesUserService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("业务人员维护")]
        [EndpointSummary("[业务人员] 查询用户分页（用于选择业务人员）")]
        public async Task<ResultPaged<UserInfoPageDto>> GetUserPage([FromBody] GetUserInfoPage getPage)
        {
            return await _salesUserService.GetUserPage(getPage);
        }
    }
}
