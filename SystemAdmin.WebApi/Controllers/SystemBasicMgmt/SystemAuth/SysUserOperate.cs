using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Queries;
using SystemAdmin.Service.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.SystemAuth
{
    [Route("api/SystemBasicMgmt/SystemAuth/[controller]/[action]")]
    [ApiController]
    public class SysUserOperate : ControllerBase
    {
        private readonly SysUserOperateService _sysUserOperateService;

        public SysUserOperate(SysUserOperateService sysUserOperateService)
        {
            _sysUserOperateService = sysUserOperateService;
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 登录")]
        [AllowAnonymous]
        public async Task<Result<SysUserLoginReturnDto>> UserLogin([FromBody] UserLogin login)
        {
            return await _sysUserOperateService.UserLogin(Response, login);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 解锁-发送验证码")]
        [AllowAnonymous]
        public async Task<Result<string>> UnLockSendCode([FromForm] string userNo)
        {
            return await _sysUserOperateService.UnLockSendCode(userNo);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 账号解锁")]
        [AllowAnonymous]
        public async Task<Result<int>> UserUnlock([FromBody] UserUnlock unlock)
        {
            return await _sysUserOperateService.UserUnLock(unlock);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 过期-发送验证码")]
        [AllowAnonymous]
        public async Task<Result<string>> UnExpirationSendCode([FromForm] string userNo)
        {
            return await _sysUserOperateService.UnExpirationSendCode(userNo);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 密码重置")]
        [AllowAnonymous]
        public async Task<Result<int>> UserPwdExpiration([FromBody] PwdExpiration expiration)
        {
            return await _sysUserOperateService.UserPwdExpiration(expiration);
        }

        [HttpPost]
        [Tags("系统基础管理-Auth")]
        [EndpointSummary("[身份验证] 登出")]
        public async Task<Result<int>> UserLogOut()
        {
            return await _sysUserOperateService.UserLogOut();
        }
    }
}
