using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Dto;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Dto;
using DepartmentDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.DepartmentDropDto;
using NationalityDropDto = SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto.NationalityDropDto;
using SystemAdmin.Service.FormBusiness.Forms;

namespace SystemAdmin.WebApi.Controllers.FormBusiness.Forms
{
    [JwtAuthorize]
    [Route("api/FormBusiness/Forms/[controller]/[action]")]
    [ApiController]
    public class SystemAccountRequest : ControllerBase
    {
        private readonly SystemAccountRequestService _systemAccountRequestService;
        public SystemAccountRequest(SystemAccountRequestService systemAccountRequestService)
        {
            _systemAccountRequestService = systemAccountRequestService;
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 厂区下拉")]
        public async Task<Result<List<SiteDropDto>>> GetSiteDrop()
        {
            return await _systemAccountRequestService.GetSiteDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _systemAccountRequestService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 国籍下拉")]
        public async Task<Result<List<NationalityDropDto>>> GetNationalityDrop()
        {
            return await _systemAccountRequestService.GetNationalityDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 系统下拉框")]
        public async Task<Result<List<SystemDropDto>>> GetSystemDrop()
        {
            return await _systemAccountRequestService.GetSystemDrop();
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 系统权限下拉框")]
        public async Task<Result<List<PermissionsDropDto>>> GetPermissionsDrop([FromForm] string system, [FromForm] string site)
        {
            return await _systemAccountRequestService.GetPermissionsDrop(system, site);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 初始化系统账号申请单")]
        public async Task<Result<SystemAccountRequestDto>> InitSystemAccountRequest([FromForm] string formTypeId)
        {
            return await _systemAccountRequestService.InitSystemAccountRequest(formTypeId);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 查询系统账号申请单明细")]
        public async Task<Result<SystemAccountRequestDto>> GetSystemAccountRequest([FromForm] string formId, [FromForm] string type)
        {
            return await _systemAccountRequestService.GetSystemAccountRequest(formId, type);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 保存系统账号申请单")]
        public async Task<Result<int>> SaveSystemAccountRequest([FromBody] SystemAccountRequestSave save)
        {
            return await _systemAccountRequestService.SaveSystemAccountRequest(save);
        }

        [HttpPost]
        [Tags("表单业务管理-表单Forms")]
        [EndpointSummary("[系统账号申请单] 发送绑定Passkey邮件")]
        public async Task<Result<int>> SendPasskeyBindMail([FromBody] PasskeyBindMailSend send)
        {
            return await _systemAccountRequestService.SendPasskeyBindMail(send);
        }
    }
}
