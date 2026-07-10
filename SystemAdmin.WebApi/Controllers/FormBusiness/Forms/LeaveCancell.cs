using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Queries;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class LeaveCancell : ControllerBase
    {
        private readonly LeaveCancellService _leaveCancellService;
        public LeaveCancell(LeaveCancellService leaveCancellService)
        {
            _leaveCancellService = leaveCancellService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[销假单] 初始化销假单")]
        public async Task<Result<LeaveCancellDto>> InitLeaveCancell([FromForm] string formTypeId)
        {
            return await _leaveCancellService.InitLeaveCancell(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[销假单] 查询销假单明细")]
        public async Task<Result<LeaveCancellDto>> GetLeaveCancell([FromForm] string formId, [FromForm] string type)
        {
            return await _leaveCancellService.GetLeaveCancell(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[销假单] 保存销假单")]
        public async Task<Result<int>> SaveLeaveCancell([FromBody] LeaveCancellSave save)
        {
            return await _leaveCancellService.SaveLeaveCancell(save);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[销假单] 查询登录用户可销假的请假单")]
        public async Task<ResultPaged<LeaveRequestDto>> GetLeaveRequestView([FromBody] GetLeaveRequestPage getPage)
        {
            return await _leaveCancellService.GetLeaveRequestView(getPage);
        }
    }
}
