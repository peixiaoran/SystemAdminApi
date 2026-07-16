using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class LeaveRequest : ControllerBase
    {
        private readonly LeaveRequestService _leaveRequestService;
        public LeaveRequest(LeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 请假类别下拉")]
        public async Task<Result<List<LeaveTypeDropDto>>> GetLeaveTypeDrop()
        {
            return await _leaveRequestService.GetLeaveTypeDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _leaveRequestService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 查询假期余额")]
        public async Task<Result<List<LeaveBalanceDto>>> GetLeaveBalances([FromForm] string formId, [FromForm] string years)
        {
            return await _leaveRequestService.GetLeaveBalances(formId, years);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 请假单送审校验")]
        public async Task<Result<bool>> ValidateLeaveRequest([FromForm] string formId)
        {
            return await _leaveRequestService.ValidateLeaveRequest(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 查询可代理用户")]
        public async Task<ResultPaged<AgentUserInfoDto>> GetUserInfoAgentView([FromBody] GetAgentUserPage getPage)
        {
            return await _leaveRequestService.GetUserInfoAgentView(getPage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 初始化请假单")]
        public async Task<Result<LeaveRequestDto>> InitLeaveRequest([FromForm] string formTypeId)
        {
            return await _leaveRequestService.InitLeaveRequest(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 查询请假单明细")]
        public async Task<Result<LeaveRequestDto>> GetLeaveRequest([FromForm] string formId, [FromForm] string type)
        {
            return await _leaveRequestService.GetLeaveRequest(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 保存请假单")]
        public async Task<Result<int>> SaveLeaveRequest([FromBody] LeaveRequestSave save)
        {
            return await _leaveRequestService.SaveLeaveRequest(save);
        }
    }
}
