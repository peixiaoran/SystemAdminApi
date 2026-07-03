using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Service.FormBusiness.FormOperate;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.FormOperate
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/FormOperate/[controller]/[action]")]
    [ApiController]
    public class ApplyHistory : ControllerBase
    {
        private readonly ApplyHistoryService _appHistoryService;
        public ApplyHistory(ApplyHistoryService appHistoryService)
        {
            _appHistoryService = appHistoryService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _appHistoryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _appHistoryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _appHistoryService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 查询申请记录分页")]
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage([FromBody] GetFormHistoryPage getpage)
        {
            return await _appHistoryService.GetApplyHistoryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 查询待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers([FromForm] string formId)
        {
            return await _appHistoryService.GetFormPendingUsers(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单撤回")]
        public async Task<Result<int>> WithdrawForm([FromForm] string formId)
        {
            return await _appHistoryService.WithdrawForm(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 表单作废")]
        public async Task<Result<int>> VoidedForm([FromForm] string formId)
        {
            return await _appHistoryService.VoidedForm(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[申请历史记录] 打印PDF")]
        public async Task<IActionResult> PrintFormPdf([FromForm] string formId, [FromForm] string prefix)
        {
            var result = await _appHistoryService.PrintFormPdf(formId, prefix);
            if (result.Code != 200)
            {
                return Ok(result);
            }
            return File(result.Data!.FileBytes, "application/pdf", result.Data.FileName);
        }
    }
}
