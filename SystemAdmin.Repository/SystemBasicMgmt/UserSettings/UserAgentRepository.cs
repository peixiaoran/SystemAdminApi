using Mapster;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Dto;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Queries;
using SystemAdmin.CommonSetup.Security;

namespace SystemAdmin.Repository.SystemBasicMgmt.UserSettings
{
    public class UserAgentRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public UserAgentRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 部门树下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<DepartmentDropDto>> GetDepartmentDrop()
        {
            return await _db.Queryable<DepartmentInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DepartmentLevelEntity>((dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                            .OrderBy((dept, deptlevel) => deptlevel.SortOrder)
                            .Select((dept, deptlevel) => new DepartmentDropDto
                            {
                                DepartmentId = dept.DepartmentId,
                                DepartmentName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ParentId = dept.ParentId,
                            }).ToTreeAsync(menu => menu.DepartmentChildList, menu => menu.ParentId, null);
        }

        /// <summary>
        /// 查询用户分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<UserAgentDto>> GetUserInfoPage(GetUserAgentPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DepartmentInfoEntity>((user, dept) => user.DepartmentId == dept.DepartmentId)
                           .InnerJoin<PositionInfoEntity>((user, dept, position) => user.PositionId == position.PositionId)
                           .InnerJoin<UserLaborEntity>((user, dept, position, labor) => user.LaborId == labor.LaborId)
                           .InnerJoin<NationalityInfoEntity>((user, dept, position, labor, nation) =>
                            user.Nationality == nation.NationId)
                           .Where((user, dept, position, labor, nation) => user.IsEmployed == 1 && user.IsFreeze == 0);

            // 用户工号
            if (!string.IsNullOrEmpty(getPage.UserNo))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNo.Contains(getPage.UserNo));
            }
            // 用户姓名
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNameCn.Contains(getPage.UserName) ||
                    user.UserNameEn.Contains(getPage.UserName));
            }
            // 部门Id
            if (!string.IsNullOrEmpty(getPage.DepartmentId) && long.Parse(getPage.DepartmentId) > 0)
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.DepartmentId == long.Parse(getPage.DepartmentId));
            }

            //排序
            query = query.OrderBy((user, dept, position, labor, nation) => new { position.SortOrder, user.HireDate });

            var page = await query.Select((user, dept, position, labor, nation) => new UserAgentDto
            {
                UserId = user.UserId,
                UserNo = user.UserNo,
                UserName = _lang.Locale == "zh-CN"
                                                 ? user.UserNameCn
                                                 : user.UserNameEn,
                DepartmentName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                PositionName = _lang.Locale == "zh-CN"
                                                 ? position.PositionNameCn
                                                 : position.PositionNameEn,
                LaborName = _lang.Locale == "zh-CN"
                                                 ? labor.LaborNameCn
                                                 : labor.LaborNameEn,
                NationalityName = _lang.Locale == "zh-CN"
                                                 ? nation.NationNameCn
                                                 : nation.NationNameEn,
                IsAgent = user.IsAgent,
                IsReview = user.IsReview,
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<UserAgentDto>.Ok(page.Adapt<List<UserAgentDto>>(), totalCount, "");
        }

        /// <summary>
        /// 查询可代理其他用户分页（occupiedUserIds 为代理时间段内已有请假或代理安排的用户，需排除）
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="occupiedUserIds"></param>
        /// <returns></returns>
        public async Task<ResultPaged<UserAgentViewDto>> GetUserInfoAgentView(GetUserAgentViewPage getPage, List<long> occupiedUserIds)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DepartmentInfoEntity>((user, dept) => user.DepartmentId == dept.DepartmentId)
                           .InnerJoin<PositionInfoEntity>((user, dept, position) => user.PositionId == position.PositionId)
                           .InnerJoin<UserLaborEntity>((user, dept, position, labor) => user.LaborId == labor.LaborId)
                           .InnerJoin<NationalityInfoEntity>((user, dept, position, labor, nation) => user.Nationality == nation.NationId)
                           .Where((user, dept, position, labor, nation) => user.UserId != long.Parse(getPage.SubstituteUserId) && user.IsFreeze == 0)
                           .WhereIF(occupiedUserIds.Count > 0, (user, dept, position, labor, nation) => !occupiedUserIds.Contains(user.UserId));

            // 用户工号
            if (!string.IsNullOrEmpty(getPage.UserNo))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNo == getPage.UserNo);
            }
            // 用户姓名
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNameCn.Contains(getPage.UserName) ||
                    user.UserNameEn.Contains(getPage.UserName));
            }
            // 部门Id
            if (!string.IsNullOrEmpty(getPage.DepartmentId) && long.Parse(getPage.DepartmentId) > 0)
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.DepartmentId == long.Parse(getPage.DepartmentId));
            }

            // 排序
            query = query.OrderBy((user, dept, position, labor, nation) => user.UserId);

            var page = await query.Select((user, dept, position, labor, nation) => new UserAgentViewDto
            {
                UserId = user.UserId,
                UserNo = user.UserNo,
                UserName = _lang.Locale == "zh-CN"
                           ? user.UserNameCn
                           : user.UserNameEn,
                DepartmentName = _lang.Locale == "zh-CN"
                           ? dept.DepartmentNameCn
                           : dept.DepartmentNameEn,
                PositionName = _lang.Locale == "zh-CN"
                           ? position.PositionNameCn
                           : position.PositionNameEn,
                LaborName = _lang.Locale == "zh-CN"
                           ? labor.LaborNameCn
                           : labor.LaborNameEn,
                NationalityName = _lang.Locale == "zh-CN"
                           ? nation.NationNameCn
                           : nation.NationNameEn,
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<UserAgentViewDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 新增用户代理人
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertUserAgent(UserAgentEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除用户代理人
        /// </summary>
        /// <param name="agentUserId"></param>
        /// <returns></returns>
        public async Task<int> DeleteUserAgent(long agentUserId)
        {
            return await _db.Deleteable<UserAgentEntity>()
                            .Where(useragent => useragent.AgentUserId == agentUserId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改用户代理状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isAgent"></param>
        /// <returns></returns>
        public async Task<int> UpdateUserAgent(long userId, int isAgent)
        {
            return await _db.Updateable<UserInfoEntity>()
                            .SetColumns(user => user.IsAgent == isAgent)
                            .Where(user => user.UserId == userId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询用户代理了哪些人列表
        /// </summary>
        /// <param name="getList"></param>
        /// <returns></returns>
        public async Task<Result<List<UserAgentProactiveDto>>> GetUserAgentProactiveList(GetUserAgentProactiveList getList)
        {
            var list = await _db.Queryable<UserAgentEntity>()
                                .With(SqlWith.NoLock)
                                .LeftJoin<UserInfoEntity>((useragent, agentuser) => useragent.AgentUserId == agentuser.UserId)
                                .LeftJoin<UserInfoEntity>((useragent, agentuser, substituteuser) => useragent.SubstituteUserId == substituteuser.UserId)
                                .Where((useragent, agentuser, substituteuser) => useragent.AgentUserId == long.Parse(getList.UserId))
                                .Select((useragent, agentuser, substituteuser) => new UserAgentProactiveDto
                                {
                                    AgentUserId = useragent.AgentUserId,
                                    SubstituteUserId = substituteuser.UserId,
                                    SubstituteUserNo = substituteuser.UserNo,
                                    SubstituteUserName = _lang.Locale == "zh-CN"
                                                         ? substituteuser.UserNameCn
                                                         : substituteuser.UserNameEn,
                                    StartTime = useragent.StartTime,
                                    EndTime = useragent.EndTime
                                }).ToListAsync();
            return Result<List<UserAgentProactiveDto>>.Ok(list.Adapt<List<UserAgentProactiveDto>>(), "");
        }

        /// <summary>
        /// 查询此用户被哪些人代理列表
        /// </summary>
        /// <param name="substituteUserId"></param>
        /// <returns></returns>
        public async Task<Result<List<UserAgentPassiveDto>>> GetUserAgentPassiveList(long substituteUserId)
        {
            var list = await _db.Queryable<UserAgentEntity>()
                                .With(SqlWith.NoLock)
                                .LeftJoin<UserInfoEntity>((useragent, substituteuser) => useragent.SubstituteUserId == substituteuser.UserId)
                                .LeftJoin<UserInfoEntity>((useragent, substituteuser, agentuser) => useragent.AgentUserId == agentuser.UserId)
                                .Where((useragent, substituteuser, agentuser) => useragent.SubstituteUserId == substituteUserId)
                                .Select((useragent, substituteuser, agentuser) => new UserAgentPassiveDto
                                {
                                    SubstituteUserId = useragent.SubstituteUserId,
                                    AgentUserId = agentuser.UserId,
                                    AgentUserNo = agentuser.UserNo,
                                    AgentUserName = _lang.Locale == "zh-CN"
                                                    ? agentuser.UserNameCn
                                                    : agentuser.UserNameEn,
                                    StartTime = useragent.StartTime,
                                    EndTime = useragent.EndTime
                                }).ToListAsync();
            return Result<List<UserAgentPassiveDto>>.Ok(list.Adapt<List<UserAgentPassiveDto>>(), "");
        }

        /// <summary>
        /// 查询与指定时间段重叠、且涉及指定用户（申请人或代理人任一角色）的审批中、已驳回请假单
        /// 已批准的请假单在审批完成时已写入代理关系表，由 GetOverlappingUserAgents 覆盖
        /// userIds 传 null 表示不限用户，用于取该时间段内全部被占用的人
        /// </summary>
        public async Task<List<UserTimeConflictDto>> GetOverlappingPendingLeaves(List<long?>? userIds, long? excludeFormId, DateTime startTime, DateTime endTime)
        {
            var list = await _db.Queryable<LeaveRequestEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<FormInstanceEntity>((leave, instance) => leave.FormId == instance.FormId)
                                .Where((leave, instance) => (instance.FormStatus == FormStatus.UnderReview.ToEnumString() || instance.FormStatus == FormStatus.Rejected.ToEnumString())
                                                         && leave.StartDateTime < endTime
                                                         && startTime < leave.EndDateTime)
                                .WhereIF(userIds != null, (leave, instance) => userIds!.Contains(instance.ApplicantUserId) || userIds!.Contains(leave.AgentUserId))
                                .WhereIF(excludeFormId.HasValue, (leave, instance) => instance.FormId != excludeFormId)
                                .Select((leave, instance) => new UserTimeConflictDto
                                {
                                    SubstituteUserId = instance.ApplicantUserId,
                                    AgentUserId = leave.AgentUserId,
                                    StartTime = leave.StartDateTime!.Value,
                                    EndTime = leave.EndDateTime!.Value
                                }).ToListAsync();
            return list;
        }

        /// <summary>
        /// 查询与指定时间段重叠、且涉及指定用户（被代理人或代理人任一角色）的代理关系
        /// userIds 传 null 表示不限用户，用于取该时间段内全部被占用的人
        /// </summary>
        public async Task<List<UserTimeConflictDto>> GetOverlappingUserAgents(List<long?>? userIds, DateTime startTime, DateTime endTime)
        {
            var list = await _db.Queryable<UserAgentEntity>()
                                .With(SqlWith.NoLock)
                                .Where(useragent => useragent.StartTime < endTime && startTime < useragent.EndTime)
                                .WhereIF(userIds != null, useragent => userIds!.Contains(useragent.SubstituteUserId) || userIds!.Contains(useragent.AgentUserId))
                                .Select(useragent => new UserTimeConflictDto
                                {
                                    SubstituteUserId = useragent.SubstituteUserId,
                                    AgentUserId = useragent.AgentUserId,
                                    StartTime = useragent.StartTime,
                                    EndTime = useragent.EndTime
                                }).ToListAsync();
            return list;
        }

        /// <summary>
        /// 查询指定时间段内已被占用（有请假单或代理安排）的用户Id，用于筛掉不可选的代理人
        /// </summary>
        public async Task<List<long>> GetOccupiedUserIds(DateTime startTime, DateTime endTime, long? excludeFormId)
        {
            var pendingConflicts = await GetOverlappingPendingLeaves(null, excludeFormId, startTime, endTime);
            var agentConflicts = await GetOverlappingUserAgents(null, startTime, endTime);

            return pendingConflicts.Concat(agentConflicts)
                                   .SelectMany(conflict => conflict.AgentUserId.HasValue
                                       ? new[] { conflict.SubstituteUserId, conflict.AgentUserId.Value }
                                       : new[] { conflict.SubstituteUserId })
                                   .Distinct()
                                   .ToList();
        }
    }
}
