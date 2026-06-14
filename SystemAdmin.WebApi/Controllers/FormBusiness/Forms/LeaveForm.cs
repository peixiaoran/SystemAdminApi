using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class LeaveForm : ControllerBase
    {
        private readonly LeaveFormService _leaveFormService;
        public LeaveForm(LeaveFormService leaveFormService)
        {
            _leaveFormService = leaveFormService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 请假类别下拉")]
        public async Task<Result<List<LeaveTypeDropDto>>> GetLeaveTypeDrop()
        {
            return await _leaveFormService.GetLeaveTypeDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _leaveFormService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 查询可代理用户分页")]
        public async Task<ResultPaged<AgentUserInfoDto>> GetUserInfoAgentView([FromBody] GetAgentUserPage getPage)
        {
            return await _leaveFormService.GetUserInfoAgentView(getPage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 初始化请假单")]
        public async Task<Result<LeaveFormDto>> InitializeLevel([FromForm] string formTypeId)
        {
            return await _leaveFormService.InitializeLevel(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 保存请假单")]
        public async Task<Result<int>> SaveLeaveForm([FromBody] LeaveFormSave save)
        {
            return await _leaveFormService.SaveLeaveForm(save);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[请假单] 查询请假单明细")]
        public async Task<Result<LeaveFormDto>> GetLeaveForm([FromForm] string formId, [FromForm] string type)
        {
            return await _leaveFormService.GetLeaveForm(formId, type);
        }
    }
}
