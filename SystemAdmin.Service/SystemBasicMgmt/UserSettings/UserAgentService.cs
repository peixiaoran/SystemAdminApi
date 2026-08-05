using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Commands;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries;
using SystemAdmin.Repository.SystemBasicMgmt.UserSettings;

namespace SystemAdmin.Service.SystemBasicMgmt.UserSettings
{
    public class UserAgentService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<UserAgentService> _logger;
        private readonly SqlSugarScope _db;
        private readonly UserAgentRepository _userAgentRepo;
        private readonly LocalizationService _localization;
        private readonly string _this = "SystemBasicMgmt.UserSettings.UserAgent";

        public UserAgentService(CurrentUser loginuser, ILogger<UserAgentService> logger, SqlSugarScope db, UserAgentRepository userAgentRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _userAgentRepo = userAgentRepo;
            _localization = localization;
        }

        /// <summary>
        /// 部门下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            try
            {
                var drop = await _userAgentRepo.GetDepartmentDrop();
                return Result<List<DepartmentDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<DepartmentDropDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询用户分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<UserAgentDto>> GetUserInfoPage(GetUserAgentPage getPage)
        {
            try
            {
                return await _userAgentRepo.GetUserInfoPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<UserAgentDto>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询可代理其他用户分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<UserAgentViewDto>> GetUserInfoAgentView(GetUserAgentViewPage getPage)
        {
            try
            {
                // 筛掉本次代理时间段内已有请假或代理安排的用户
                var occupiedUserIds = await _userAgentRepo.GetOccupiedUserIds(getPage.StartTime, getPage.EndTime, null);

                return await _userAgentRepo.GetUserInfoAgentView(getPage, occupiedUserIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<UserAgentViewDto>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 新增用户代理人
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertUserAgent(UserAgentUpsert upsert)
        {
            try
            {
                // 检查被代理用户是否与代理用户一致
                if (upsert.SubstituteUserId == upsert.AgentUserId)
                {
                    // 被代理用户不能和代理用户相同
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}AgentSameUser"));
                }

                var substituteUserId = long.Parse(upsert.SubstituteUserId);
                var agentUserId = long.Parse(upsert.AgentUserId);

                // 校验被代理人、代理人在本次代理期间是否已有请假单或代理关系
                // 两人中任意一人，以申请人/被代理人或代理人任一角色出现，时间重叠即算冲突
                var involvedUserIds = new List<long?> { substituteUserId, agentUserId };

                // 审批中、已驳回的请假单（已批准的请假单已写入代理关系表，由下方一并覆盖）
                var pendingConflicts = await _userAgentRepo.GetOverlappingPendingLeaves(involvedUserIds, null, upsert.StartTime, upsert.EndTime);

                // 已生效的代理关系
                var agentConflicts = await _userAgentRepo.GetOverlappingUserAgents(involvedUserIds, upsert.StartTime, upsert.EndTime);

                var conflicts = pendingConflicts.Concat(agentConflicts).ToList();

                // 被代理人时间冲突：已有重叠的请假/被代理，或正代理他人（代理职责在身）
                var substituteConflict = conflicts.FirstOrDefault(conflict =>
                    conflict.SubstituteUserId == substituteUserId || conflict.AgentUserId == substituteUserId);

                if (substituteConflict != null)
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg(
                        $"{_this}SubstituteTimeConflict",
                        args: new object[]
                        {
                            substituteConflict.StartTime.ToString("yyyy-MM-dd HH:mm"),
                            substituteConflict.EndTime.ToString("yyyy-MM-dd HH:mm")
                        }
                    ));
                }

                // 代理人时间冲突：本人也在请假/被代理，或已代理他人（一人同时只能代理一个人）
                var agentConflict = conflicts.FirstOrDefault(conflict =>
                    conflict.SubstituteUserId == agentUserId || conflict.AgentUserId == agentUserId);

                if (agentConflict != null)
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg(
                        $"{_this}AgentTimeConflict",
                        args: new object[]
                        {
                            agentConflict.StartTime.ToString("yyyy-MM-dd HH:mm"),
                            agentConflict.EndTime.ToString("yyyy-MM-dd HH:mm")
                        }
                    ));
                }

                var insertUserAgent = new UserAgentEntity
                {
                    SubstituteUserId = substituteUserId,
                    AgentUserId = agentUserId,
                    StartTime = upsert.StartTime,
                    EndTime = upsert.EndTime,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now,
                };

                await _db.BeginTranAsync();
                // 新增用户代理人配置
                int insertUserAgentCount = await _userAgentRepo.InsertUserAgent(insertUserAgent);
                // 更新用户代理状态
                var updateUserAgentCount = await _userAgentRepo.UpdateUserAgent(agentUserId, 1);
                await _db.CommitTranAsync();

                return insertUserAgentCount >= 1 && updateUserAgentCount >= 1
                        ? Result<int>.Ok(insertUserAgentCount, _localization.ReturnMsg($"{_this}InsertSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}InsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 删除用户代理关系
        /// </summary>
        /// <param name="agentUserId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteUserAgent(string agentUserId)
        {
            try
            {
                await _db.BeginTranAsync();
                // 删除用户代理配置
                var delSubAgentCount = await _userAgentRepo.DeleteUserAgent(long.Parse(agentUserId));
                var updateUserAgentCount = await _userAgentRepo.UpdateUserAgent(long.Parse(agentUserId), 0);
                await _db.CommitTranAsync();

                return delSubAgentCount >= 1 && updateUserAgentCount >= 1
                            ? Result<int>.Ok(delSubAgentCount, _localization.ReturnMsg($"{_this}DeleteSuccess"))
                            : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}DeleteFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询此用户代理的用户列表
        /// </summary>
        /// <param name="getList"></param>
        /// <returns></returns>
        public async Task<Result<List<UserAgentProactiveDto>>> GetUserAgentProactiveList(GetUserAgentProactiveList getList)
        {
            try
            {
                return await _userAgentRepo.GetUserAgentProactiveList(getList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<UserAgentProactiveDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询此用户被哪个用户代理列表
        /// </summary>
        /// <param name="substituteUserId"></param>
        /// <returns></returns>
        public async Task<Result<List<UserAgentPassiveDto>>> GetUserAgentPassiveList(string substituteUserId)
        {
            try
            {
                return await _userAgentRepo.GetUserAgentPassiveList(long.Parse(substituteUserId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<UserAgentPassiveDto>>.Failure(500, ex.Message.ToString());
            }
        }
    }
}
