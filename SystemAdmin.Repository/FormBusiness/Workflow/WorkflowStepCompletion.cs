using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
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
    /// 步骤完成后置处理
    /// </summary>
    public class WorkflowStepCompletion
    {
        private readonly SqlSugarScope _db;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly CurrentUser _loginuser;
        private readonly Dictionary<string, Func<long, Task<Result<bool>>>> _registry;
        private readonly string _this = "FormBusiness.Workflow";

        public WorkflowStepCompletion(SqlSugarScope db, Language lang, LocalizationService localization, CurrentUser loginuser)
        {
            _db = db;
            _localization = localization;
            _lang = lang;
            _loginuser = loginuser;
            _registry = new Dictionary<string, Func<long, Task<Result<bool>>>>(StringComparer.OrdinalIgnoreCase)
            {
                // 请假单
                [nameof(ProcessLeaveRequest)] = ProcessLeaveRequest,
                // 销假单
                [nameof(ProcessLeaveCancell)] = ProcessLeaveCancell,
                // 资讯需求单
                [nameof(InforCategoryApply)] = InforCategoryApply,
                [nameof(InforCategoryEstimated)] = InforCategoryEstimated,
                [nameof(InforCategoryProcessStart)] = InforCategoryProcessStart,
                [nameof(InforCategoryProcessEnd)] = InforCategoryProcessEnd,
                [nameof(InforCategoryRating)] = InforCategoryRating,
            };
        }

        public async Task<Result<bool>> Resolve(string guidance, long formId)
        {
            // 未配置 Guidance 时直接放行
            if (string.IsNullOrWhiteSpace(guidance))
            {
                return Result<bool>.Ok(true);
            }

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

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            // 仅 Annual、Sick 占用余额
            var needBalance = leaveRequest.LeaveType == LeaveType.Annual.ToEnumString() || leaveRequest.LeaveType == LeaveType.Sick.ToEnumString();

            var updateBlance = 0;
            if (needBalance)
            {
                var startTime = leaveRequest.StartDateTime!.Value;
                var endTime = leaveRequest.EndDateTime!.Value;

                // 按年度拆分请假工时
                var leaveHoursByYear = CalcHoursByYear(startTime, endTime);

                var userInfo = await _db.Queryable<UserInfoEntity>()
                                        .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

                var isChinese = _lang.Locale == "zh-CN";
                var userName = isChinese ? userInfo.UserNameCn : userInfo.UserNameEn;

                var leaveInfo = await _db.Queryable<DictionaryInfoEntity>()
                                         .Where(dic => dic.DicType == "LeaveType" && dic.DicCode == leaveRequest.LeaveType)
                                         .FirstAsync();
                var leaveName = isChinese ? leaveInfo.DicNameCn : leaveInfo.DicNameEn;

                foreach (var (year, hours) in leaveHoursByYear)
                {
                    var days = Math.Ceiling(hours / 8);
                    var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                               .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveRequest.LeaveType);

                    if (leaveAnnual == null)
                    {
                        return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                        {
                            userName ?? formInstance.ApplicantUserId.ToString(),
                            year,
                            leaveName
                        }));
                    }

                    var newRemainingDays = leaveAnnual.RemainingDays - (decimal)days;
                    if (newRemainingDays < 0)
                    {
                        return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.InsufficientLeaveBalance", args: new object[]
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
            }

            // SubstituteUserId：被代理人（申请人），AgentUserId：代理人
            var userAgent = new UserAgentEntity
            {
                SubstituteUserId = formInstance.ApplicantUserId,
                AgentUserId = leaveRequest.AgentUserId!.Value,
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

            return Result<bool>.Ok((!needBalance || updateBlance >= 1) && insertAgentCount >= 1 && updateAgentCount >= 1);
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

            // 仅 Annual、Sick 占用余额，其余假别无需加回
            if (leaveType != LeaveType.Annual.ToEnumString() && leaveType != LeaveType.Sick.ToEnumString())
            {
                return Result<bool>.Ok(true);
            }

            var startTime = cancell.StartDateTime!.Value;
            var endTime = cancell.EndDateTime!.Value;

            // 按年度拆分销假工时
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
                // 按天数加回，保留两位小数
                var days = Math.Round((decimal)hours / 8, 2, MidpointRounding.AwayFromZero);
                var leaveAnnual = await _db.Queryable<UserLeaveBalanceEntity>()
                                           .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType);

                if (leaveAnnual == null)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", args: new object[]
                    {
                        userName ?? formInstance.ApplicantUserId.ToString(),
                        year,
                        leaveName
                    }));
                }

                // 加回额度，不超过给予天数
                var restoredDays = Math.Min(leaveAnnual.RemainingDays + days, leaveAnnual.RenderDays);
                updateBalance = await _db.Updateable<UserLeaveBalanceEntity>()
                                         .SetColumns(annual => new UserLeaveBalanceEntity
                                         {
                                             RemainingDays = restoredDays,
                                             ModifiedBy = formInstance.CreatedBy,
                                             ModifiedDate = DateTime.Now
                                         }).Where(annual => annual.UserId == formInstance.ApplicantUserId && annual.Year == year && annual.LeaveType == leaveType)
                                         .ExecuteCommandAsync();
            }

            return Result<bool>.Ok(updateBalance >= 1);
        }

        #endregion

        #region 资讯需求单

        /// <summary>
        /// 提交：写入申请人（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforCategoryApply(long formId)
        {
            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var affected = await UpsertInforCategoryResult(
                formId,
                () => new InforCategoryResultEntity
                {
                    FormId = formId,
                    ApplicantUserId = formInstance.ApplicantUserId,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<InforCategoryResultEntity>()
                        .SetColumns(result => new InforCategoryResultEntity
                        {
                            ApplicantUserId = formInstance.ApplicantUserId,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 预估处理时间：写入预计处理天数和处理人（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforCategoryEstimated(long formId)
        {
            var estimatedDays = await _db.Queryable<InformationRequestEntity>()
                                         .Where(info => info.FormId == formId)
                                         .Select(info => info.EstimatedDays)
                                         .FirstAsync();

            var affected = await UpsertInforCategoryResult(
                formId,
                () => new InforCategoryResultEntity
                {
                    FormId = formId,
                    EstimatedDays = estimatedDays,
                    HandlerId = _loginuser.UserId,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<InforCategoryResultEntity>()
                        .SetColumns(result => new InforCategoryResultEntity
                        {
                            EstimatedDays = estimatedDays,
                            HandlerId = _loginuser.UserId,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理开始：写入当前时间（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforCategoryProcessStart(long formId)
        {
            var now = DateTime.Now;

            var affected = await UpsertInforCategoryResult(
                formId,
                () => new InforCategoryResultEntity
                {
                    FormId = formId,
                    ProcessStartTime = now,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                },
                () => _db.Updateable<InforCategoryResultEntity>()
                        .SetColumns(result => new InforCategoryResultEntity
                        {
                            ProcessStartTime = now,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 处理完成：写入当前时间（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforCategoryProcessEnd(long formId)
        {
            var now = DateTime.Now;

            var affected = await UpsertInforCategoryResult(
                formId,
                () => new InforCategoryResultEntity
                {
                    FormId = formId,
                    ProcessEndTime = now,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                },
                () => _db.Updateable<InforCategoryResultEntity>()
                        .SetColumns(result => new InforCategoryResultEntity
                        {
                            ProcessEndTime = now,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// 评分：写入表单评分（已有数据则更新）
        /// </summary>
        public async Task<Result<bool>> InforCategoryRating(long formId)
        {
            var rating = await _db.Queryable<InformationRequestEntity>()
                                  .Where(info => info.FormId == formId)
                                  .Select(info => info.Rating)
                                  .FirstAsync();

            var affected = await UpsertInforCategoryResult(
                formId,
                () => new InforCategoryResultEntity
                {
                    FormId = formId,
                    Rating = rating,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now
                },
                () => _db.Updateable<InforCategoryResultEntity>()
                        .SetColumns(result => new InforCategoryResultEntity
                        {
                            Rating = rating,
                            ModifiedBy = _loginuser.UserId,
                            ModifiedDate = DateTime.Now
                        }).Where(result => result.FormId == formId)
                        .ExecuteCommandAsync());

            return Result<bool>.Ok(affected >= 1);
        }

        /// <summary>
        /// InforCategoryResult 按 FormId 新增或修改
        /// </summary>
        private async Task<int> UpsertInforCategoryResult(long formId, Func<InforCategoryResultEntity> buildInsertEntity, Func<Task<int>> update)
        {
            var exists = await _db.Queryable<InforCategoryResultEntity>()
                                  .With(SqlWith.NoLock)
                                  .AnyAsync(result => result.FormId == formId);

            if (exists)
            {
                return await update();
            }

            return await _db.Insertable(buildInsertEntity()).ExecuteCommandAsync();
        }

        #endregion

        /// <summary>
        /// 按自然年拆分时间段并累计工时（8-12、13-17，午休不计）
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
