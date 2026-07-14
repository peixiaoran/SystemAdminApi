using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤结束执行😇
    /// </summary>
    public class WorkflowStepCompletion
    {
        private readonly SqlSugarScope _db;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly Dictionary<string, Func<long, Task<Result<bool>>>> _registry;
        private readonly string _this = "FormBusiness.Workflow";

        public WorkflowStepCompletion(SqlSugarScope db, Language lang, LocalizationService localization)
        {
            _db = db;
            _localization = localization;
            _lang = lang;
            _registry = new Dictionary<string, Func<long, Task<Result<bool>>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(ProcessLeaveRequest)] = ProcessLeaveRequest,
                [nameof(ProcessLeaveCancell)] = ProcessLeaveCancell,
            };
        }

        public async Task<Result<bool>> Resolve(string guidance, long formId)
        {
            // 没有配置 Guidance:不算失败,正常放行
            if (string.IsNullOrWhiteSpace(guidance))
            {
                return Result<bool>.Ok(true);
            }

            // Resolve 内
            if (!_registry.TryGetValue(guidance, out var handler))
            {
                return Result<bool>.Failure(500, _localization.ReturnMsg($"{_this}.GuidanceHandlerNotFound", _lang.Locale, guidance));
            }

            return await handler(formId);
        }

        #region 请假单

        /// <summary>
        /// 请假单审批完成后处理
        /// </summary>
        public async Task<Result<bool>> ProcessLeaveRequest(long formId)
        {
            var leaveRequest = await _db.Queryable<FormInstanceEntity>()
                                        .With(SqlWith.NoLock)
                                        .InnerJoin<LeaveRequestEntity>((instance, leave) => instance.FormId == leave.FormId)
                                        .Where((instance, leave) => instance.FormId == formId)
                                        .Select((instance, leave) => leave)
                                        .FirstAsync();

            var startTime = leaveRequest.StartDateTime!.Value;
            var endTime = leaveRequest.EndDateTime!.Value;

            // 按年度拆分请假工时（上午 8-12、下午 13-17，跨年按自然年归集）
            var leaveHoursByYear = CalcHoursByYear(startTime, endTime);

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var userInfo = await _db.Queryable<UserInfoEntity>()
                                    .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

            var isChinese = _lang.Locale == "zh-CN";
            var userName = isChinese ? userInfo.UserNameCn : userInfo.UserNameEn;

            var leaveInfo = await _db.Queryable<DictionaryInfoEntity>()
                                     .Where(dic => dic.DicType == "LeaveType" && dic.DicCode == leaveRequest.LeaveType)
                                     .FirstAsync();
            var leaveName = isChinese ? leaveInfo.DicNameCn : leaveInfo.DicNameEn;

            var updateBlance = 0;
            foreach (var (year, hours) in leaveHoursByYear)
            {
                var days = Math.Ceiling(hours / 8);
                var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                           .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveRequest.LeaveType);

                if (leaveAnnual == null)
                {
                    return Result<bool>.Failure(402, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                    {
                        userName ?? formInstance.ApplicantUserId.ToString(),
                        year,
                        leaveName
                    }));
                }

                var newRemainingDays = leaveAnnual.RemainingDays - (decimal)days;
                if (newRemainingDays < 0)
                {
                    return Result<bool>.Failure(402, _localization.ReturnMsg($"{_this}.InsufficientLeaveBalance", args: new object[]
                    {
                        userName ?? formInstance.ApplicantUserId.ToString(),
                        year,
                        leaveName,
                        leaveAnnual.RemainingDays,
                        days
                    }));
                }

                updateBlance = await _db.Updateable<UserLeaveBalanceEntity>()
                                        .SetColumns(annual => new UserLeaveBalanceEntity
                                        {
                                            RemainingDays = newRemainingDays,
                                            ModifiedBy = formInstance.CreatedBy,
                                            ModifiedDate = DateTime.Now
                                        }).Where(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveRequest.LeaveType)
                                        .ExecuteCommandAsync();
            }

            // 记录代理人信息
            var userAgent = new UserAgentEntity
            {
                SubstituteUserId = formInstance.ApplicantUserId, // 被代理人（申请人）
                AgentUserId = leaveRequest.AgentUserId!.Value, // 代理人
                StartTime = leaveRequest.StartDateTime!.Value,
                EndTime = leaveRequest.EndDateTime!.Value,
                CreatedBy = formInstance.CreatedBy,
                CreatedDate = DateTime.Now
            };

            var insertAgentCount = await _db.Insertable(userAgent).ExecuteCommandAsync();

            var updateAgentCount = await _db.Updateable<UserInfoEntity>()
                                            .SetColumns(user => new UserInfoEntity
                                            {
                                                IsAgent = 1
                                            }).Where(user => user.UserId == leaveRequest.AgentUserId)
                                            .ExecuteCommandAsync();

            return Result<bool>.Ok(updateBlance>=1 && insertAgentCount >= 1 && updateAgentCount >= 1);
        }

        #endregion

        #region 销假单

        /// <summary>
        /// 销假单审批完成后处理：将销假时数按年度加回对应假别的剩余额度
        /// </summary>
        public async Task<Result<bool>> ProcessLeaveCancell(long formId)
        {
            var cancell = await _db.Queryable<LeaveCancellEntity>()
                                   .With(SqlWith.NoLock)
                                   .Where(item => item.FormId == formId)
                                   .FirstAsync();

            // 假别取自绑定的请假单
            var leaveType = await _db.Queryable<LeaveRequestEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(leave => leave.FormId == cancell.LeaveRequestId)
                                     .Select(leave => leave.LeaveType)
                                     .FirstAsync();

            var startTime = cancell.StartDateTime!.Value;
            var endTime = cancell.EndDateTime!.Value;

            // 按年度拆分销假工时（与请假单一致：上午 8-12、下午 13-17，跨年按自然年归集）
            var cancellHoursByYear = CalcHoursByYear(startTime, endTime);

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var userInfo = await _db.Queryable<UserInfoEntity>()
                                    .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

            var isChinese = _lang.Locale == "zh-CN";
            var userName = isChinese ? userInfo.UserNameCn : userInfo.UserNameEn;

            var leaveInfo = await _db.Queryable<DictionaryInfoEntity>()
                                     .Where(dic => dic.DicType == "LeaveType" && dic.DicCode == leaveType)
                                     .FirstAsync();
            var leaveName = isChinese ? leaveInfo.DicNameCn : leaveInfo.DicNameEn;

            var updateBalance = 0;
            foreach (var (year, hours) in cancellHoursByYear)
            {
                var days = Math.Ceiling(hours / 8);
                var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                           .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType);

                if (leaveAnnual == null)
                {
                    return Result<bool>.Failure(402, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                    {
                        userName ?? formInstance.ApplicantUserId.ToString(),
                        year,
                        leaveName
                    }));
                }

                // 销假：把之前扣掉的额度加回去
                updateBalance = await _db.Updateable<UserLeaveBalanceEntity>()
                                         .SetColumns(annual => new UserLeaveBalanceEntity
                                         {
                                             RemainingDays = leaveAnnual.RemainingDays + (decimal)days,
                                             ModifiedBy = formInstance.CreatedBy,
                                             ModifiedDate = DateTime.Now
                                         }).Where(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType)
                                         .ExecuteCommandAsync();
            }

            return Result<bool>.Ok(updateBalance >= 1);
        }

        #endregion

        /// <summary>
        /// 将一段时间按自然年拆分并累计工作时数（上午 8:00-12:00、下午 13:00-17:00，午休不计）
        /// </summary>
        private static Dictionary<int, double> CalcHoursByYear(DateTime startTime, DateTime endTime)
        {
            var hoursByYear = new Dictionary<int, double>();

            var currentDate = startTime.Date;
            var endDate = endTime.Date;

            while (currentDate <= endDate)
            {
                var dayStart = currentDate;
                var dayEnd = currentDate.AddDays(1).AddTicks(-1);
                var effectiveStart = dayStart < startTime ? startTime : dayStart;
                var effectiveEnd = dayEnd > endTime ? endTime : dayEnd;

                if (effectiveStart <= effectiveEnd)
                {
                    double dayHours = 0;

                    var morningStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 8, 0, 0);
                    var morningEnd = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 12, 0, 0);
                    var morningCalculated = (morningStart > effectiveStart ? morningStart : effectiveStart);
                    var morningEnd2 = (morningEnd < effectiveEnd ? morningEnd : effectiveEnd);
                    if (morningCalculated < morningEnd2)
                    {
                        dayHours += (morningEnd2 - morningCalculated).TotalHours;
                    }

                    var afternoonStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 13, 0, 0);
                    var afternoonEnd = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 17, 0, 0);
                    var afternoonCalculated = (afternoonStart > effectiveStart ? afternoonStart : effectiveStart);
                    var afternoonEnd2 = (afternoonEnd < effectiveEnd ? afternoonEnd : effectiveEnd);
                    if (afternoonCalculated < afternoonEnd2)
                    {
                        dayHours += (afternoonEnd2 - afternoonCalculated).TotalHours;
                    }

                    if (dayHours > 0)
                    {
                        var year = currentDate.Year;
                        if (hoursByYear.ContainsKey(year))
                        {
                            hoursByYear[year] += dayHours;
                        }
                        else
                        {
                            hoursByYear[year] = dayHours;
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return hoursByYear;
        }
    }
}
