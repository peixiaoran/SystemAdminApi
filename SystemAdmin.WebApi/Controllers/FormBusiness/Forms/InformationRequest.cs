using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Dto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class InformationRequest : ControllerBase
    {
        private readonly InformationRequestService _informationRequestService;
        public InformationRequest(InformationRequestService informationRequestService)
        {
            _informationRequestService = informationRequestService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[资讯需求单] 需求类别下拉")]
        public async Task<Result<List<ITCategoryDropDto>>> GetITCategoryDrop()
        {
            return await _informationRequestService.GetITCategoryDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[资讯需求单] 初始化资讯需求单")]
        public async Task<Result<InformationRequestDto>> InitInformationRequest([FromForm] string formTypeId)
        {
            return await _informationRequestService.InitInformationRequest(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[资讯需求单] 查询资讯需求单明细")]
        public async Task<Result<InformationRequestDto>> GetInformationRequest([FromForm] string formId, [FromForm] string type)
        {
            return await _informationRequestService.GetInformationRequest(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[资讯需求单] 保存资讯需求单")]
        public async Task<Result<int>> SaveInformationRequest([FromBody] InformationRequestSave save)
        {
            return await _informationRequestService.SaveInformationRequest(save);
        }
    }
}
