using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class LeaveFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<LeaveFormService> _logger;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveFormRepository _leaveForm;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveFormService(CurrentUser loginuser, ILogger<LeaveFormService> logger, SqlSugarScope db, Language lang, FormPermissionChecker formchecker, LeaveFormRepository leaveForm, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _lang = lang;
            _leaveForm = leaveForm;
            _formChecker = formchecker;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 请假类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<LeaveTypeDropDto>>> GetLeaveTypeDrop()
        {
            var drop = await _leaveForm.GetLeaveTypeDrop();
            return Result<List<LeaveTypeDropDto>>.Ok(drop);
        }

        /// <summary>
        /// 部门下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<DepartmentDropDto>>> GetDepartmentDrop()
        {
            try
            {
                var drop = await _leaveForm.GetDepartmentDrop();
                return Result<List<DepartmentDropDto>>.Ok(drop, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<DepartmentDropDto>>.Failure(500, ex.Message.ToString());
            }
        }

        /// <summary>
        /// 查询可代理用户
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<AgentUserInfoDto>> GetUserInfoAgentView(GetAgentUserPage getPage)
        {
            try
            {
                return await _leaveForm.GetUserInfoAgentView(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<AgentUserInfoDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询假期余额
        /// </summary>
        public async Task<Result<List<LeaveBalanceDto>>> GetLeaveBalances(string formId, string years)
        {
            var yearList = (years ?? string.Empty)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(yearText => int.Parse(yearText.Trim()))
                .ToList();

            // 1. 查询申请人基础余额
            var balanceList = await _leaveForm.GetApplicantLeaveBalances(long.Parse(formId), yearList);

            // 2. 查询申请人审批中的请假单
            var pendingLeaves = await _leaveForm.GetApplicantPendingLeaves(long.Parse(formId));

            // 3. 计算审批中请假的占用工时（按年份和假别），逐日累计
            var pendingHoursByYearAndType = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var leave in pendingLeaves)
            {
                var leaveStart = (DateTime)leave.StartDateTime!;
                var leaveEnd = (DateTime)leave.EndDateTime!;

                var cursor = leaveStart.Date;
                var lastDate = leaveEnd.Date;
                while (cursor <= lastDate)
                {
                    var morningStart = cursor.AddHours(8);
                    var morningEnd = cursor.AddHours(12);
                    var afternoonStart = cursor.AddHours(13);
                    var afternoonEnd = cursor.AddHours(17);

                    decimal dayHours;
                    if (cursor == leaveStart.Date && cursor == leaveEnd.Date)
                    {
                        if (leaveEnd <= morningEnd)
                            dayHours = (decimal)(leaveEnd - leaveStart).TotalHours;
                        else if (leaveStart >= afternoonStart)
                            dayHours = (decimal)(leaveEnd - leaveStart).TotalHours;
                        else
                            dayHours = (decimal)(morningEnd - leaveStart).TotalHours + (decimal)(leaveEnd - afternoonStart).TotalHours;
                    }
                    else if (cursor == leaveStart.Date)
                    {
                        if (leaveStart < morningEnd)
                            dayHours = (decimal)(morningEnd - leaveStart).TotalHours + 4;
                        else
                            dayHours = (decimal)(afternoonEnd - leaveStart).TotalHours;
                    }
                    else if (cursor == leaveEnd.Date)
                    {
                        if (leaveEnd <= morningEnd)
                            dayHours = (decimal)(leaveEnd - morningStart).TotalHours;
                        else
                            dayHours = 4 + (decimal)(leaveEnd - afternoonStart).TotalHours;
                    }
                    else
                    {
                        dayHours = 8;
                    }

                    var yearKey = cursor.Year.ToString();
                    if (!pendingHoursByYearAndType.ContainsKey(yearKey))
                        pendingHoursByYearAndType[yearKey] = new Dictionary<string, decimal>();
                    if (!pendingHoursByYearAndType[yearKey].ContainsKey(leave.LeaveType!))
                        pendingHoursByYearAndType[yearKey][leave.LeaveType!] = 0;
                    pendingHoursByYearAndType[yearKey][leave.LeaveType!] += dayHours;

                    cursor = cursor.AddDays(1);
                }
            }

            // 4. 计算最终余额（天数，1天=8小时）
            var result = balanceList
                .GroupBy(balance => balance.Year)
                .Select(yearGroup =>
                {
                    var year = yearGroup.Key;
                    var yearKey = year.ToString();
                    var pendingByType = pendingHoursByYearAndType.ContainsKey(yearKey)
                        ? pendingHoursByYearAndType[yearKey]
                        : new Dictionary<string, decimal>();

                    var annualRemaining = yearGroup
                        .Where(b => b.LeaveType == LeaveType.Annual.ToEnumString())
                        .Sum(b => b.RemainingDays);

                    var sickRemaining = yearGroup
                        .Where(b => b.LeaveType == LeaveType.Sick.ToEnumString())
                        .Sum(b => b.RemainingDays);

                    var annualPendingDays = Math.Round(pendingByType.GetValueOrDefault(LeaveType.Annual.ToEnumString(), 0) / 8, 2);
                    var sickPendingDays = Math.Round(pendingByType.GetValueOrDefault(LeaveType.Sick.ToEnumString(), 0) / 8, 2);

                    return new LeaveBalanceDto
                    {
                        Year = year,
                        AnnualRemainingDays = Math.Round(annualRemaining - annualPendingDays, 2),
                        SickRemainingDays = Math.Round(sickRemaining - sickPendingDays, 2)
                    };
                }).OrderBy(dto => dto.Year)
                .ToList();

            return Result<List<LeaveBalanceDto>>.Ok(result);
        }

        /// <summary>
        /// 验证假期余额
        /// </summary>
        public async Task<Result<bool>> ValidateLeaveBalance(string formId)
        {
            // 1. 查询当前表单请假信息
            var currentLeave = await _leaveForm.GetCurrentLeaveForm(long.Parse(formId));

            // 2. 查询假别字典
            var leaveTypeDics = await _leaveForm.GetLeaveTypeDictionary();

            // 3. 计算当前表单涉及的年份
            var currentStart = (DateTime)currentLeave.StartDateTime!;
            var currentEnd = (DateTime)currentLeave.EndDateTime!;
            var involvedYears = Enumerable.Range(currentStart.Year, currentEnd.Year - currentStart.Year + 1).ToList();

            // 4. 查询申请人基础余额
            var balanceList = await _leaveForm.GetApplicantLeaveBalances(long.Parse(formId), involvedYears);

            // 5. 查询其余审批中的请假单
            var pendingLeaves = await _leaveForm.GetApplicantPendingLeaves(long.Parse(formId));

            // 6. 计算其余审批中请假的占用工时（按年份和假别），逐日累计
            var pendingHoursByYearAndType = new Dictionary<string, Dictionary<string, decimal>>();

            foreach (var leave in pendingLeaves)
            {
                var leaveStart = (DateTime)leave.StartDateTime!;
                var leaveEnd = (DateTime)leave.EndDateTime!;

                var cursor = leaveStart.Date;
                var lastDate = leaveEnd.Date;
                while (cursor <= lastDate)
                {
                    var morningStart = cursor.AddHours(8);
                    var morningEnd = cursor.AddHours(12);
                    var afternoonStart = cursor.AddHours(13);
                    var afternoonEnd = cursor.AddHours(17);

                    decimal dayHours;
                    if (cursor == leaveStart.Date && cursor == leaveEnd.Date)
                    {
                        if (leaveEnd <= morningEnd)
                            dayHours = (decimal)(leaveEnd - leaveStart).TotalHours;
                        else if (leaveStart >= afternoonStart)
                            dayHours = (decimal)(leaveEnd - leaveStart).TotalHours;
                        else
                            dayHours = (decimal)(morningEnd - leaveStart).TotalHours + (decimal)(leaveEnd - afternoonStart).TotalHours;
                    }
                    else if (cursor == leaveStart.Date)
                    {
                        if (leaveStart < morningEnd)
                            dayHours = (decimal)(morningEnd - leaveStart).TotalHours + 4;
                        else
                            dayHours = (decimal)(afternoonEnd - leaveStart).TotalHours;
                    }
                    else if (cursor == leaveEnd.Date)
                    {
                        if (leaveEnd <= morningEnd)
                            dayHours = (decimal)(leaveEnd - morningStart).TotalHours;
                        else
                            dayHours = 4 + (decimal)(leaveEnd - afternoonStart).TotalHours;
                    }
                    else
                    {
                        dayHours = 8;
                    }

                    var yearKey = cursor.Year.ToString();
                    if (!pendingHoursByYearAndType.ContainsKey(yearKey))
                        pendingHoursByYearAndType[yearKey] = new Dictionary<string, decimal>();
                    if (!pendingHoursByYearAndType[yearKey].ContainsKey(leave.LeaveType!))
                        pendingHoursByYearAndType[yearKey][leave.LeaveType!] = 0;
                    pendingHoursByYearAndType[yearKey][leave.LeaveType!] += dayHours;

                    cursor = cursor.AddDays(1);
                }
            }

            // 7. 计算当前表单本次请假的占用工时（按年份），逐日累计
            var currentHoursByYear = new Dictionary<string, decimal>();

            var currentCursor = currentStart.Date;
            var currentLastDate = currentEnd.Date;
            while (currentCursor <= currentLastDate)
            {
                var morningStart = currentCursor.AddHours(8);
                var morningEnd = currentCursor.AddHours(12);
                var afternoonStart = currentCursor.AddHours(13);
                var afternoonEnd = currentCursor.AddHours(17);

                decimal dayHours;
                if (currentCursor == currentStart.Date && currentCursor == currentEnd.Date)
                {
                    if (currentEnd <= morningEnd)
                        dayHours = (decimal)(currentEnd - currentStart).TotalHours;
                    else if (currentStart >= afternoonStart)
                        dayHours = (decimal)(currentEnd - currentStart).TotalHours;
                    else
                        dayHours = (decimal)(morningEnd - currentStart).TotalHours + (decimal)(currentEnd - afternoonStart).TotalHours;
                }
                else if (currentCursor == currentStart.Date)
                {
                    if (currentStart < morningEnd)
                        dayHours = (decimal)(morningEnd - currentStart).TotalHours + 4;
                    else
                        dayHours = (decimal)(afternoonEnd - currentStart).TotalHours;
                }
                else if (currentCursor == currentEnd.Date)
                {
                    if (currentEnd <= morningEnd)
                        dayHours = (decimal)(currentEnd - morningStart).TotalHours;
                    else
                        dayHours = 4 + (decimal)(currentEnd - afternoonStart).TotalHours;
                }
                else
                {
                    dayHours = 8;
                }

                var yearKey = currentCursor.Year.ToString();
                if (!currentHoursByYear.ContainsKey(yearKey))
                    currentHoursByYear[yearKey] = 0;
                currentHoursByYear[yearKey] += dayHours;

                currentCursor = currentCursor.AddDays(1);
            }

            // 8. 按年份逐一校验余额（天数，1天=8小时），遇到第一个不符合立即返回
            foreach (var year in involvedYears)
            {
                var yearKey = year.ToString();

                var baseRemainingDays = (decimal)balanceList
                    .Where(b => b.Year == year && b.LeaveType == currentLeave.LeaveType)
                    .Sum(b => b.RemainingDays);

                var pendingHours = pendingHoursByYearAndType.ContainsKey(yearKey)
                    && pendingHoursByYearAndType[yearKey].ContainsKey(currentLeave.LeaveType!)
                    ? pendingHoursByYearAndType[yearKey][currentLeave.LeaveType!]
                    : 0;

                var currentHours = currentHoursByYear.ContainsKey(yearKey)
                    ? currentHoursByYear[yearKey]
                    : 0;

                // 本次请假在该年为 0 工时（例如 17:00→次日 08:00 衔接），不校验、不提示
                if (currentHours <= 0)
                    continue;

                // 转天数后统一保留 2 位小数，保证计算值与显示值一致
                var availableRemainingDays = Math.Round(baseRemainingDays - (pendingHours / 8), 2);
                var currentUsageDays = Math.Round(currentHours / 8, 2);
                var remainingDays = availableRemainingDays - currentUsageDays;

                if (remainingDays < 0)
                {
                    var leaveTypeDic = leaveTypeDics.FirstOrDefault(d => d.DicCode == currentLeave.LeaveType);
                    var leaveTypeName = _lang.Locale == "zh-CN" ? leaveTypeDic?.DicNameCn : leaveTypeDic?.DicNameEn;

                    return Result<bool>.Failure(402, _localization.ReturnMsg(
                        $"{_form}LeaveBalanceNotEnough",
                        year,
                        leaveTypeName,
                        currentUsageDays.ToString("0.##"),
                        availableRemainingDays.ToString("0.##")
                    ));
                }
            }

            return Result<bool>.Ok(true);
        }

        /// <summary>
        /// 初始化请假单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveFormDto>> InitLeaveForm(string formTypeId)
        {
            try
            {
                var isCan = await _formChecker.CanApply(long.Parse(formTypeId));
                if (!isCan)
                {
                    return Result<LeaveFormDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanApply"));
                }
                else
                {
                    await _db.BeginTranAsync();
                    var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                    var leaveForm = new LeaveFormEntity()
                    {
                        FormId = long.Parse(formId),
                        LeaveType = null,
                        LeaveReason = null,
                        StartDateTime = null,
                        EndDateTime = null,
                        LeaveHours = 0.00m,
                        AgentUserId = null,
                        AgentUserName = null,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now
                    };

                    await _leaveForm.InitLeaveForm(leaveForm);
                    await _formmanger.MatchWorkflowRule(long.Parse(formTypeId), long.Parse(formId));
                    await _db.CommitTranAsync();

                    var leaveFormDto = await _leaveForm.GetLeaveForm(long.Parse(formId));
                    leaveFormDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                    leaveFormDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                    leaveFormDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId);
                    return Result<LeaveFormDto>.Ok(leaveFormDto);
                }
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<LeaveFormDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存请假单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveLeaveForm(LeaveFormSave save)
        {
            try
            {
                var entity = new LeaveFormEntity()
                {
                    FormId = long.Parse(save.FormId),
                    LeaveType = save.LeaveType,
                    LeaveReason = save.LeaveReason,
                    StartDateTime = save.StartDateTime,
                    EndDateTime = save.EndDateTime,
                    LeaveHours = CalculateLeaveHours(save.StartDateTime, save.EndDateTime),
                    AgentUserId = string.IsNullOrWhiteSpace(save.AgentUserId) ? null : long.Parse(save.AgentUserId),
                    AgentUserName = save.AgentUserName,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _leaveForm.SaveLeaveForm(entity);
                await _formmanger.SaveFormInstance(long.Parse(save.FormId));
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_form}SaveSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_form}SaveFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询请假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveFormDto>> GetLeaveForm(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<LeaveFormDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _leaveForm.GetLeaveForm(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId);
                return Result<LeaveFormDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<LeaveFormDto>.Failure(500, ex.Message);
            }
        }

        #region 计算请假时数

        private decimal CalculateLeaveHours(DateTime startDateTime, DateTime endDateTime)
        {
            decimal totalHours = 0m;

            DateTime currentDate = startDateTime.Date;
            DateTime lastDate = endDateTime.Date;

            while (currentDate <= lastDate)
            {
                DateTime dayMorningStart = currentDate.AddHours(8);
                DateTime dayMorningEnd = currentDate.AddHours(12);
                DateTime dayAfternoonStart = currentDate.AddHours(13);
                DateTime dayAfternoonEnd = currentDate.AddHours(17);

                if (currentDate == startDateTime.Date && currentDate == endDateTime.Date)
                {
                    // 同一天
                    if (endDateTime <= dayMorningEnd)
                    {
                        // 都在上午
                        totalHours += (decimal)(endDateTime - startDateTime).TotalHours;
                    }
                    else if (startDateTime >= dayAfternoonStart)
                    {
                        // 都在下午
                        totalHours += (decimal)(endDateTime - startDateTime).TotalHours;
                    }
                    else
                    {
                        // 跨上下午：上午部分 + 下午部分
                        totalHours += (decimal)(dayMorningEnd - startDateTime).TotalHours;
                        totalHours += (decimal)(endDateTime - dayAfternoonStart).TotalHours;
                    }
                }
                else if (currentDate == startDateTime.Date)
                {
                    // 开始日期
                    if (startDateTime < dayMorningEnd)
                    {
                        // 开始时间在上午，算上午剩余 + 整个下午
                        totalHours += (decimal)(dayMorningEnd - startDateTime).TotalHours;
                        totalHours += 4;
                    }
                    else
                    {
                        // 开始时间在下午，只算下午部分
                        totalHours += (decimal)(dayAfternoonEnd - startDateTime).TotalHours;
                    }
                }
                else if (currentDate == endDateTime.Date)
                {
                    // 结束日期
                    if (endDateTime <= dayMorningEnd)
                    {
                        // 结束时间在上午，只算上午部分
                        totalHours += (decimal)(endDateTime - dayMorningStart).TotalHours;
                    }
                    else
                    {
                        // 结束时间在下午，算整个上午 + 下午部分
                        totalHours += 4;
                        totalHours += (decimal)(endDateTime - dayAfternoonStart).TotalHours;
                    }
                }
                else
                {
                    // 中间的完整工作日
                    totalHours += 8;
                }

                currentDate = currentDate.AddDays(1);
            }

            return Math.Round(totalHours, 2);
        }

        #endregion
    }
}
