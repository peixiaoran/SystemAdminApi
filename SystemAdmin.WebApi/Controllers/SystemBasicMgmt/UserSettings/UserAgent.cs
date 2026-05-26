using Microsoft.AspNetCore.Mvc;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Commands;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries;
using SystemAdmin.Service.SystemBasicMgmt.UserSettings;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.UserSettings
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/SystemBasicMgmt/UserSettings/[controller]/[action]")]
    [ApiController]
    public class UserAgent : ControllerBase
    {
        private readonly UserAgentService _userAgentService;
        public UserAgent(UserAgentService userAgentService)
        {
            _userAgentService = userAgentService;
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户信息] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _userAgentService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 查询用户分页")]
        public async Task<ResultPaged<UserAgentDto>> GetUserInfoPage([FromBody] GetUserAgentPage getPage)
        {
            return await _userAgentService.GetUserInfoPage(getPage);
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 查询可代理用户")]
        public async Task<ResultPaged<UserAgentViewDto>> GetUserInfoAgentView([FromBody] GetUserAgentViewPage getPage)
        {
            return await _userAgentService.GetUserInfoAgentView(getPage);
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 新增用户代理人")]
        public async Task<Result<int>> InsertUserAgent([FromBody] UserAgentUpsert upsert)
        {
            return await _userAgentService.InsertUserAgent(upsert);
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 删除用户代理人")]
        public async Task<Result<int>> DeleteUserAgent([FromForm] string agentUserId)
        {
            return await _userAgentService.DeleteUserAgent(agentUserId);
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 查询用户代理了谁")]
        public async Task<Result<List<UserAgentProactiveDto>>> GetUserAgentProactiveList([FromBody] GetUserAgentProactiveList getList)
        {
            return await _userAgentService.GetUserAgentProactiveList(getList);
        }

        [HttpPost]
        [Tags("系统基础管理-用户相关配置")]
        [EndpointSummary("[用户代理] 查询用户被谁代理")]
        public async Task<Result<List<UserAgentPassiveDto>>> GetUserAgentList([FromForm] string substituteUserId)
        {
            return await _userAgentService.GetUserAgentPassiveList(substituteUserId);
        }
    }
}
