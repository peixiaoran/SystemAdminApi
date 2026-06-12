using SqlSugar;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤结束执行
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
                [nameof(DeductLeaveBalance)] = DeductLeaveBalance,
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
        /// 扣除假期余额
        /// </summary>
        public async Task<Result<bool>> DeductLeaveBalance(long formId)
        {
            var leaveForm = await _db.Queryable<FormInstanceEntity>()
                                     .InnerJoin<LeaveFormEntity>((instance, leave) => instance.FormId == leave.FormId)
                                     .Where((instance, leave) => instance.FormId == formId)
                                     .Select<LeaveFormEntity>()
                                     .FirstAsync();

            var startTime = leaveForm.StartDateTime!.Value;
            var endTime = leaveForm.EndDateTime!.Value;
            var leaveHoursByYear = new Dictionary<int, double>();

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
                        if (leaveHoursByYear.ContainsKey(year))
                        {
                            leaveHoursByYear[year] += dayHours;
                        }
                        else
                        {
                            leaveHoursByYear[year] = dayHours;
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .FirstAsync(instance => instance.FormId == formId);

            var userInfo = await _db.Queryable<UserInfoEntity>()
                                    .FirstAsync(user => user.UserId == formInstance.ApplicantUserId);

            var isChinese = _lang.Locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase);
            var userName = isChinese ? userInfo?.UserNameCn : userInfo?.UserNameEn;

            foreach (var (year, hours) in leaveHoursByYear)
            {
                var days = Math.Ceiling(hours / 8);
                var leaveAnnual = await _db.Queryable<UserLeaveAnnualEntity>()
                                           .FirstAsync(annual => annual.UserId == formInstance.ApplicantUserId
                                                              && annual.Year == year
                                                              && annual.LeaveType == leaveForm.LeaveType);

                if (leaveAnnual == null)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.LeaveAnnualNotFound", _lang.Locale,
                        userName ?? formInstance.ApplicantUserId.ToString(), year, leaveForm.LeaveType));
                }

                var newRemainingDays = leaveAnnual.RemainingDays - (decimal)days;
                if (newRemainingDays < 0)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_this}.InsufficientLeaveBalance",
                        userName ?? formInstance.ApplicantUserId.ToString(), year, leaveForm.LeaveType,
                        leaveAnnual.RemainingDays, days));
                }

                await _db.Updateable<UserLeaveAnnualEntity>()
                    .SetColumns(annual => new UserLeaveAnnualEntity
                    {
                        RemainingDays = newRemainingDays,
                        ModifiedBy = formInstance.CreatedBy,
                        ModifiedDate = DateTime.Now
                    })
                    .Where(annual => annual.UserId == formInstance.ApplicantUserId
                                  && annual.Year == year
                                  && annual.LeaveType == leaveForm.LeaveType)
                    .ExecuteCommandAsync();
            }

            return Result<bool>.Ok(true);
        }

        #endregion
    }
}
