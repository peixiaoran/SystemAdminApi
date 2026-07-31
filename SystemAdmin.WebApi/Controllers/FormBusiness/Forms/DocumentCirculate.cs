using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Commands;
using SystemAdmin.Model.FormBusiness.Forms.DocumentCirculate.Dto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class DocumentCirculate : ControllerBase
    {
        private readonly DocumentCirculateService _documentCirculateService;
        public DocumentCirculate(DocumentCirculateService documentCirculateService)
        {
            _documentCirculateService = documentCirculateService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[传签单] 初始化传签单")]
        public async Task<Result<DocumentCirculateDto>> InitDocumentCirculate([FromForm] string formTypeId)
        {
            return await _documentCirculateService.InitDocumentCirculate(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[传签单] 查询传签单明细")]
        public async Task<Result<DocumentCirculateDto>> GetDocumentCirculate([FromForm] string formId, [FromForm] string type)
        {
            return await _documentCirculateService.GetDocumentCirculate(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[传签单] 保存传签单")]
        public async Task<Result<int>> SaveDocumentCirculate([FromBody] DocumentCirculateSave save)
        {
            return await _documentCirculateService.SaveDocumentCirculate(save);
        }
    }
}
