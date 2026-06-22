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
    public class VoidedForm : ControllerBase
    {
        private readonly VoidedFormService _voidedFormService;
        public VoidedForm(VoidedFormService voidedFormService)
        {
            _voidedFormService = voidedFormService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _voidedFormService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _voidedFormService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _voidedFormService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 查询作废表单分页")]
        public async Task<ResultPaged<VoidedFormDto>> GetVoidedFormsPage([FromBody] GetVoidedFormPage getpage)
        {
            return await _voidedFormService.GetVoidedFormsPage(getpage);
        }
    }
}
