using Fido2NetLib;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemMgmt.Dto;
using SystemAdmin.Service.SystemBasicMgmt.SystemAuth;
using SystemAdmin.Service.SystemBasicMgmt.SystemBasicData;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.SystemBasicData
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/SystemBasicMgmt/SystemBasicData/[controller]/[action]")]
    [ApiController]
    public class PersonalInfo : ControllerBase
    {
        private readonly PersonalInfoService _personalInfoService;
        private readonly PasskeyService _passkeyService;
        public PersonalInfo(PersonalInfoService personalInfoService, PasskeyService passkeyService)
        {
            _personalInfoService = personalInfoService;
            _passkeyService = passkeyService;
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 职业下拉")]
        public async Task<Result<List<UserLaborDropDto>>> GetLaborDrop()
        {
            return await _personalInfoService.GetLaborDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _personalInfoService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 职级下拉")]
        public async Task<Result<List<PositionDropDto>>> GetPositionDrop()
        {
            return await _personalInfoService.GetPositionDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 角色下拉")]
        public async Task<Result<List<RoleInfoDropDto>>> GetRoleDrop()
        {
            return await _personalInfoService.GetRoleDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 上传用户头像")]
        public async Task<Result<string>> UploadAvatar([FromForm] string userId, IFormFile file)
        {
            return await _personalInfoService.UploadAvatar(userId, file);
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 查询个人信息实体")]
        public async Task<Result<PersonalInfoDto>> GetPersonalInfoEntity()
        {
            return await _personalInfoService.GetPersonalInfoEntity();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] 修改个人信息")]
        public async Task<Result<int>> UpdatePersonalInfo([FromBody] PersonalInfoUpsert upsert)
        {
            return await _personalInfoService.UpdatePersonalInfo(upsert);
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] Passkey 注册-获取选项")]
        public async Task<Result<CredentialCreateOptions>> PasskeyRegisterOptions()
        {
            return await _passkeyService.PasskeyRegisterOptions();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] Passkey 注册-提交凭据")]
        public async Task<Result<int>> PasskeyRegisterComplete([FromBody] PasskeyRegisterComplete complete)
        {
            return await _passkeyService.PasskeyRegisterComplete(complete);
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] Passkey 列表")]
        public async Task<Result<List<PasskeyDto>>> GetPasskeyList()
        {
            return await _passkeyService.GetPasskeyList();
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] Passkey 重命名")]
        public async Task<Result<int>> RenamePasskey([FromBody] PasskeyRename rename)
        {
            return await _passkeyService.RenamePasskey(rename);
        }

        [HttpPost]
        [Tags("系统基础管理-系统管理模块")]
        [EndpointSummary("[个人信息] Passkey 删除")]
        public async Task<Result<int>> DeletePasskey([FromBody] PasskeyDelete delete)
        {
            return await _passkeyService.DeletePasskey(delete);
        }
    }
}
