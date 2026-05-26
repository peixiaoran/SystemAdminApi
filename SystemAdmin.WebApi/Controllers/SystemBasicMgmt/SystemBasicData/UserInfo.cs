using Microsoft.AspNetCore.Mvc;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemMgmt.Dto;
using SystemAdmin.Service.SystemBasicMgmt.SystemBasicData;
using SystemAdmin.WebApi.Attributes;

namespace SystemAdmin.WebApi.Controllers.SystemBasicMgmt.SystemBasicData
{
    [JwtAuthorize]
    [RoutingAuthorize]
    [Route("api/SystemBasicMgmt/SystemBasicData/[controller]/[action]")]
    [ApiController]
    public class UserInfo : ControllerBase
    {
        private readonly UserInfoService _userInfoService;
        private readonly LocalizationService _localization;
        private readonly string _thisExcel = "SystemBasicMgmt.SystemBasicData.UserExcel_";
        public UserInfo(UserInfoService userInfoService, LocalizationService localization)
        {
            _localization = localization;
            _userInfoService = userInfoService;
        }


        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 国籍下拉")]
        public async Task<Result<List<NationalityDropDto>>> GetNationalityDrop()
        {
            return await _userInfoService.GetNationalityDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 部门下拉")]
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            return await _userInfoService.GetDepartmentDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 职级下拉")]
        public async Task<Result<List<PositionDropDto>>> GetPositionDrop()
        {
            return await _userInfoService.GetPositionDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 职业下拉")]
        public async Task<Result<List<UserLaborDropDto>>> GetLaborDrop()
        {
            return await _userInfoService.GetLaborDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 角色下拉")]
        public async Task<Result<List<RoleInfoDropDto>>> GetRoleDrop()
        {
            return await _userInfoService.GetRoleDrop();
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 上传用户头像（新增用户）")]
        public async Task<Result<string>> UploadAvatarInsert([FromForm]IFormFile file)
        {
            return await _userInfoService.UploadAvatar(file);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 上传用户头像（修改用户）")]
        public async Task<Result<string>> UploadAvatarUpdate([FromForm]string userId, IFormFile file)
        {
            return await _userInfoService.UploadAvatar(userId, file);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 新增用户信息")]
        public async Task<Result<int>> InsertUserInfo([FromBody] UserInfoUpsert upsert)
        {
            return await _userInfoService.InsertUserInfo(upsert);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 删除用户信息")]
        public async Task<Result<int>> DeleteUserInfo([FromForm] string userId)
        {
            return await _userInfoService.DeleteUserInfo(userId);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 修改用户信息")]
        public async Task<Result<int>> UpdateUserInfo([FromBody] UserInfoUpsert upsert)
        {
            return await _userInfoService.UpdateUserInfo(upsert);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 查询用户实体")]
        public async Task<Result<UserInfoEntityDto>> GetUserInfoEntity([FromForm] string userId)
        {
            return await _userInfoService.GetUserInfoEntity(userId);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 查询用户分页")]
        public async Task<ResultPaged<UserInfoPageDto>> GetUserInfoPage([FromBody] GetUserInfoPage getPage)
        {
            return await _userInfoService.GetUserInfoPage(getPage);
        }

        [HttpPost]
        [Tags("系统基础管理-基本信息模块")]
        [EndpointSummary("[用户信息] 导出Excel表格")]
        public async Task<IActionResult> ExportUserExcel([FromBody] GetUserInfoExcel getExcel)
        {
            var bytes = await _userInfoService.GetUserInfoExcel(getExcel);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _localization.ReturnMsg($"{_thisExcel}UserInfo") + ".xlsx");
        }
    }
}
