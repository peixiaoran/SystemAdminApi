using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Queries;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    public class LeaveFormRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public LeaveFormRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 请假类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<LeaveTypeDropDto>> GetLeaveTypeDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .Where(dic => dic.DicType == "LeaveType")
                            .Select(dic => new LeaveTypeDropDto()
                            {
                                LeaveType = dic.DicCode,
                                LeaveTypeName = _lang.Locale == "zh-CN"
                                                ? dic.DicNameCn
                                                : dic.DicNameEn,
                            }).ToListAsync();
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
                            }).ToTreeAsync(menu => menu.DepartmentChildList, menu => menu.ParentId, 0);
        }

        /// <summary>
        /// 查询可代理用户
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        public async Task<ResultPaged<AgentUserInfoDto>> GetUserInfoAgentView(GetAgentUserPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DepartmentInfoEntity>((user, dept) => user.DepartmentId == dept.DepartmentId)
                           .InnerJoin<PositionInfoEntity>((user, dept, position) => user.PositionId == position.PositionId)
                           .InnerJoin<UserLaborEntity>((user, dept, position, labor) => user.LaborId == labor.LaborId)
                           .InnerJoin<NationalityInfoEntity>((user, dept, position, labor, nation) => user.Nationality == nation.NationId)
                           .Where((user, dept, position, labor, nation) => user.IsAgent == 0 && user.IsFreeze == 0 && user.UserId != loginUserId);

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
            // 部门Id - 仅当工号和姓名都为空时才应用
            if (string.IsNullOrEmpty(getPage.UserNo) &&
                string.IsNullOrEmpty(getPage.UserName) &&
                !string.IsNullOrEmpty(getPage.DepartmentId) &&
                long.Parse(getPage.DepartmentId) > 0)
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.DepartmentId == long.Parse(getPage.DepartmentId));
            }

            // 排序
            query = query.OrderBy((user, dept, position, labor, nation) => user.UserId);

            var page = await query.Select((user, dept, position, labor, nation) => new AgentUserInfoDto
            {
                UserId = user.UserId,
                UserNo = user.UserNo,
                UserName = _lang.Locale == "zh-CN"
                           ? user.UserNameCn
                           : user.UserNameEn,
                DepartmentName = _lang.Locale == "zh-CN"
                           ? dept.DepartmentNameCn
                           : dept.DepartmentNameEn
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);

            return ResultPaged<AgentUserInfoDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 查询假期余额
        /// </summary>
        public async Task<List<LeaveBalanceDto>> GetLeaveBalances(long loginUserId, string years)
        {
            var yearList = (years ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(yearText => int.Parse(yearText.Trim()))
                .ToList();

            // 1. 查询用户的基础余额
            var balanceList = await _db.Queryable<UserLeaveBalanceEntity>()
                                       .Where(balance => balance.UserId == loginUserId)
                                       .WhereIF(yearList.Count > 0, balance => yearList.Contains(balance.Year))
                                       .ToListAsync();

            // 2. 查询正在审批中的请假单
            var pendingLeaves = await _db.Queryable<LeaveFormEntity>()
                                         .InnerJoin<FormInstanceEntity>((leaveForm, formInstance) => leaveForm.FormId == formInstance.FormId)
                                         .Where((leaveForm, formInstance) => formInstance.ApplicantUserId == loginUserId && (formInstance.FormStatus == FormStatus.UnderReview.ToEnumString() || formInstance.FormStatus == FormStatus.Rejected.ToEnumString()))
                                         .Select((leaveForm, formInstance) => new
                                         {
                                             leaveForm.LeaveType,
                                             leaveForm.StartDateTime,
                                             leaveForm.EndDateTime,
                                             leaveForm.LeaveHours
                                         }).ToListAsync();

            // 3. 计算审批中的请假占用（按年份和假别）
            var pendingUsageByYearAndType = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var leave in pendingLeaves)
            {
                var startDateTime = (DateTime)leave.StartDateTime!;
                var endDateTime = (DateTime)leave.EndDateTime!;

                var startYear = startDateTime.Year;
                var endYear = endDateTime.Year;

                if (startYear == endYear)
                {
                    // 同年请假：直接计算时差 / 8
                    var workingHours = (endDateTime - startDateTime).TotalHours;
                    var leaveDays = (decimal)workingHours / 8;

                    var yearKey = startYear.ToString();
                    if (!pendingUsageByYearAndType.ContainsKey(yearKey))
                        pendingUsageByYearAndType[yearKey] = new Dictionary<string, decimal>();
                    if (!pendingUsageByYearAndType[yearKey].ContainsKey(leave.LeaveType!))
                        pendingUsageByYearAndType[yearKey][leave.LeaveType!] = 0;
                    pendingUsageByYearAndType[yearKey][leave.LeaveType!] += leaveDays;
                }
                else
                {
                    // 跨年请假，分摊到各年

                    // 第一年：从开始时间到该年12月31日 17:00
                    var firstYearEnd = new DateTime(startYear, 12, 31, 17, 0, 0);
                    var firstYearHours = (firstYearEnd - startDateTime).TotalHours;
                    var firstYearDays = (decimal)firstYearHours / 8;

                    var firstYearKey = startYear.ToString();
                    if (!pendingUsageByYearAndType.ContainsKey(firstYearKey))
                        pendingUsageByYearAndType[firstYearKey] = new Dictionary<string, decimal>();
                    if (!pendingUsageByYearAndType[firstYearKey].ContainsKey(leave.LeaveType!))
                        pendingUsageByYearAndType[firstYearKey][leave.LeaveType!] = 0;
                    pendingUsageByYearAndType[firstYearKey][leave.LeaveType!] += firstYearDays;

                    // 中间完整年份（如果有）
                    var currentYear = startYear + 1;
                    while (currentYear < endYear)
                    {
                        var currentYearKey = currentYear.ToString();
                        if (!pendingUsageByYearAndType.ContainsKey(currentYearKey))
                            pendingUsageByYearAndType[currentYearKey] = new Dictionary<string, decimal>();
                        if (!pendingUsageByYearAndType[currentYearKey].ContainsKey(leave.LeaveType!))
                            pendingUsageByYearAndType[currentYearKey][leave.LeaveType!] = 0;
                        // 完整年份：8小时/天 * 8天/天 = 64小时 = 8天
                        pendingUsageByYearAndType[currentYearKey][leave.LeaveType!] += 8;

                        currentYear++;
                    }

                    // 最后一年：从1月1日 8:00 到结束时间
                    var lastYearStart = new DateTime(endYear, 1, 1, 8, 0, 0);
                    var lastYearHours = (endDateTime - lastYearStart).TotalHours;
                    var lastYearDays = (decimal)lastYearHours / 8;

                    var lastYearKey = endYear.ToString();
                    if (!pendingUsageByYearAndType.ContainsKey(lastYearKey))
                        pendingUsageByYearAndType[lastYearKey] = new Dictionary<string, decimal>();
                    if (!pendingUsageByYearAndType[lastYearKey].ContainsKey(leave.LeaveType!))
                        pendingUsageByYearAndType[lastYearKey][leave.LeaveType!] = 0;
                    pendingUsageByYearAndType[lastYearKey][leave.LeaveType!] += lastYearDays;
                }
            }

            // 4. 计算最终余额
            var result = balanceList
                .GroupBy(balance => balance.Year)
                .Select(yearGroup =>
                {
                    var year = yearGroup.Key;
                    var yearKey = year.ToString();
                    var pendingByType = pendingUsageByYearAndType.ContainsKey(yearKey)
                        ? pendingUsageByYearAndType[yearKey]
                        : new Dictionary<string, decimal>();

                    var annualRemaining = (decimal)yearGroup
                        .Where(balance => balance.LeaveType == LeaveType.Annual.ToEnumString())
                        .Sum(balance => balance.RemainingDays);

                    var sickRemaining = (decimal)yearGroup
                        .Where(balance => balance.LeaveType == LeaveType.Sick.ToEnumString())
                        .Sum(balance => balance.RemainingDays);

                    var annualPending = pendingByType.ContainsKey(LeaveType.Annual.ToEnumString())
                        ? pendingByType[LeaveType.Annual.ToEnumString()]
                        : 0;
                    var sickPending = pendingByType.ContainsKey(LeaveType.Sick.ToEnumString())
                        ? pendingByType[LeaveType.Sick.ToEnumString()]
                        : 0;

                    return new LeaveBalanceDto
                    {
                        Year = year,
                        AnnualRemainingDays = (int)(annualRemaining - annualPending),
                        SickRemainingDays = (int)(sickRemaining - sickPending)
                    };
                }).OrderBy(dto => dto.Year)
                .ToList();

            return result;
        }

        /// <summary>
        /// 初始化请假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InitLeaveForm(LeaveFormEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 保存请假表单
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> SaveLeaveForm(LeaveFormEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(leave => new
                            {
                                leave.FormId,
                                leave.CreatedBy,
                                leave.CreatedDate,
                            }).Where(leave => leave.FormId == entity.FormId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询请假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<LeaveFormDto> GetLeaveForm(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<LeaveFormEntity>((form, leave) => form.FormId == leave.FormId)
                            .InnerJoin<UserInfoEntity>((form, leave, user) => form.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((form, leave, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DictionaryInfoEntity>((form, leave, user, dept, dic) => dic.DicType == "FormStatus" && form.FormStatus == dic.DicCode)
                            .Where((form, leave, user, dept, dic) => form.FormId == formId)
                            .Select((form, leave, user, dept, dic) => new LeaveFormDto()
                            {
                                FormTypeId = form.FormTypeId,
                                RuleId = form.RuleId,
                                CurrentStepId = form.CurrentStepId,
                                FormStatus = form.FormStatus,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                                FormId = form.FormId,
                                FormNo = form.FormNo,
                                ApplicantUserNo = user.UserNo,
                                ApplicantUserName = _lang.Locale == "zh-CN"
                                                 ? user.UserNameCn
                                                 : user.UserNameEn,
                                ApplicantDeptName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ApplicantDate = form.ApplicantDate,
                                LeaveType = leave.LeaveType,
                                LeaveReason = leave.LeaveReason,
                                StartDateTime = leave.StartDateTime,
                                EndDateTime = leave.EndDateTime,
                                LeaveHours = leave.LeaveHours,
                                AgentUserId = leave.AgentUserId,
                                AgentUserName = leave.AgentUserName,
                            }).FirstAsync();
        }
    }
}
