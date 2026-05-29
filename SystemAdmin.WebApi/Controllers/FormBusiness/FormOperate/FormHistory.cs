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
    public class FormHistory : ControllerBase
    {
        private readonly FormHistoryService _formHistoryService;
        public FormHistory(FormHistoryService formHistoryService)
        {
            _formHistoryService = formHistoryService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[表单历史记录] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _formHistoryService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[表单历史记录] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _formHistoryService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[表单历史记录] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _formHistoryService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[表单历史记录] 查询申请记录分页")]
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage([FromBody] GetFormHistoryPage getpage)
        {
            return await _formHistoryService.GetApplyHistoryPage(getpage);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[表单历史记录] 查询审批记录分页")]
        public async Task<ResultPaged<FormHistoryDto>> GetReviewHistoryPage([FromBody] GetFormHistoryPage getpage)
        {
            return await _formHistoryService.GetReviewHistoryPage(getpage);
        }
    }
}
