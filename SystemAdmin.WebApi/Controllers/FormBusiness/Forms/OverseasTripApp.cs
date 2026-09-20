using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Commands;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class OverseasTripApp : ControllerBase
    {
        private readonly OverseasTripAppService _overseasTripAppService;
        public OverseasTripApp(OverseasTripAppService overseasTripAppService)
        {
            _overseasTripAppService = overseasTripAppService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 交通方式下拉")]
        public async Task<Result<List<TravelModeDropDto>>> GetTravelModeDrop()
        {
            return await _overseasTripAppService.GetTravelModeDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 厂区下拉")]
        public async Task<Result<List<FactoryDropDto>>> GetFactoryDrop()
        {
            return await _overseasTripAppService.GetFactoryDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 初始化出差单")]
        public async Task<Result<OverseasTripAppDto>> InitOverseasTripApp([FromForm] string formTypeId)
        {
            return await _overseasTripAppService.InitOverseasTripApp(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 查询出差单明细")]
        public async Task<Result<OverseasTripAppDto>> GetOverseasTripApp([FromForm] string formId, [FromForm] string type)
        {
            return await _overseasTripAppService.GetOverseasTripApp(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 出差单送审校验")]
        public async Task<Result<bool>> ValidateOverseasTripApp([FromForm] string formId)
        {
            return await _overseasTripAppService.ValidateOverseasTripApp(formId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[出差单] 保存出差单")]
        public async Task<Result<int>> SaveOverseasTripApp([FromBody] OverseasTripAppSave save)
        {
            return await _overseasTripAppService.SaveOverseasTripApp(save);
        }
    }
}
