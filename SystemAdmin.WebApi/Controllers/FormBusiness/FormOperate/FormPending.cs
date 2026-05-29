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
    public class FormPending : ControllerBase
    {
        private readonly FormPendingService _formPendingService;
        public FormPending(FormPendingService formPendingService)
        {
            _formPendingService = formPendingService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _formPendingService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _formPendingService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _formPendingService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 查询待送审分页")]
        public async Task<ResultPaged<FormPendingDto>> GetPendingSubmitPage([FromBody] GetFormPendingPage getpage)
        {
            return await _formPendingService.GetPendingSubmitPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 查询待审批分页")]
        public async Task<ResultPaged<FormPendingDto>> GetPendingReviewPage([FromBody] GetFormPendingPage getpage)
        {
            return await _formPendingService.GetPendingReviewPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 查询表单待审批人")]
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUser([FromForm] string formId)
        {
            return await _formPendingService.GetFormPendingUser(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[待审表单列表] 作废表单")]
        public async Task<Result<int>> VoidedForm([FromForm] string formId)
        {
            return await _formPendingService.VoidedForm(formId);
        }
    }
}
