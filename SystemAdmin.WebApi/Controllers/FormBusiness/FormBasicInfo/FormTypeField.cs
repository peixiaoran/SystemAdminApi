using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Commands;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Dto;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Queries;
using SystemAdmin.Service.FormBusiness.FormBasicInfo;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.FormBasicInfo
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/FormBusiness/FormBasicInfo/[controller]/[action]")]
    [ApiController]
    public class FormTypeField : ControllerBase
    {
        private readonly FormTypeFieldService _formTypeFieldService;
        public FormTypeField(FormTypeFieldService formTypeFieldService)
        {
            _formTypeFieldService = formTypeFieldService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 表单组别下拉")]
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            return await _formTypeFieldService.GetFormGroupDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 表单类别下拉")]
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop([FromForm] string formGroupId)
        {
            return await _formTypeFieldService.GetFormTypeDrop(formGroupId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 新增表单栏位")]
        public async Task<Result<int>> InsertFormTypeField([FromBody] FormTypeFieldUpsert upsert)
        {
            return await _formTypeFieldService.InsertFormTypeField(upsert);
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 删除表单栏位")]
        public async Task<Result<int>> DeleteFormTypeField([FromForm] string fieldId)
        {
            return await _formTypeFieldService.DeleteFormTypeField(fieldId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 修改表单栏位")]
        public async Task<Result<int>> UpdateFormTypeField([FromBody] FormTypeFieldUpsert upsert)
        {
            return await _formTypeFieldService.UpdateFormTypeField(upsert);
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 查询表单栏位实体")]
        public async Task<Result<FormTypeFieldDto>> GetFormTypeFieldEntity([FromForm] string fieldId)
        {
            return await _formTypeFieldService.GetFormTypeFieldEntity(fieldId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单基础信息")]
        [EndpointSummary("[表单栏位] 查询表单栏位分页")]
        public async Task<ResultPaged<FormTypeFieldDto>> GetFormTypeFieldPage([FromBody] GetFormTypeFieldPage getPage)
        {
            return await _formTypeFieldService.GetFormTypeFieldPage(getPage);
        }
    }
}
