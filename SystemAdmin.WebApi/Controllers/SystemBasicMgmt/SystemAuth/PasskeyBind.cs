using Fido2NetLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Service.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.SystemAuth
{
    /// <summary>凭邮件链接令牌绑定 Passkey（匿名，身份由令牌证明）</summary>
    [AllowAnonymous]
    [Route("api/SystemBasicMgmt/SystemAuth/[controller]/[action]")]
    [ApiController]
    public class PasskeyBind : ControllerBase
    {
        private readonly PasskeyBindService _passkeyBindService;

        public PasskeyBind(PasskeyBindService passkeyBindService)
        {
            _passkeyBindService = passkeyBindService;
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[Passkey绑定] 凭令牌查询用户信息")]
        public async Task<Result<PasskeyBindUserInfoDto>> GetBindUserInfo([FromBody] PasskeyBindQuery query)
        {
            return await _passkeyBindService.GetBindUserInfo(query);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[Passkey绑定] 获取注册选项")]
        public async Task<Result<CredentialCreateOptions>> PasskeyBindOptions([FromBody] PasskeyBindQuery query)
        {
            return await _passkeyBindService.PasskeyBindOptions(query);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[Passkey绑定] 提交凭据完成绑定")]
        public async Task<Result<int>> PasskeyBindComplete([FromBody] PasskeyBindComplete complete)
        {
            return await _passkeyBindService.PasskeyBindComplete(complete);
        }
    }
}
