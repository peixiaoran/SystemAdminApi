using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Service.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.SystemAuth
{
    /// <summary>Passkey 登录（匿名）；注册与管理接口见 PersonalInfo</summary>
    [Route("api/SystemBasicMgmt/SystemAuth/[controller]/[action]")]
    [ApiController]
    public class Passkey : ControllerBase
    {
        private readonly PasskeyService _passkeyService;

        public Passkey(PasskeyService passkeyService)
        {
            _passkeyService = passkeyService;
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[Passkey] 登录-获取选项")]
        [AllowAnonymous]
        public async Task<Result<PasskeyLoginOptionsDto>> PasskeyLoginOptions()
        {
            return await _passkeyService.PasskeyLoginOptions();
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[Passkey] 登录-提交断言")]
        [AllowAnonymous]
        public async Task<Result<SysUserLoginReturnDto>> PasskeyLoginComplete([FromBody] PasskeyLoginComplete complete)
        {
            return await _passkeyService.PasskeyLoginComplete(Response, complete);
        }
    }
}
