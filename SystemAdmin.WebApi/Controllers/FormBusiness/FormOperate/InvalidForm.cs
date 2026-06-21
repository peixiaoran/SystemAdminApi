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
    public class InvalidForm : ControllerBase
    {
        private readonly InvalidFormService _invalidFormService;
        public InvalidForm(InvalidFormService invalidFormService)
        {
            _invalidFormService = invalidFormService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _invalidFormService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _invalidFormService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 表单状态下拉")]
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            return await _invalidFormService.GetFormStatusDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单作业模块")]
        [EndpointSummary("[作废表单列表] 查询作废表单分页")]
        public async Task<ResultPaged<InvalidFormDto>> GetInvalidFormsPage([FromBody] GetInvalidFormsPage getpage)
        {
            return await _invalidFormService.GetInvalidFormsPage(getpage);
        }
    }
}
