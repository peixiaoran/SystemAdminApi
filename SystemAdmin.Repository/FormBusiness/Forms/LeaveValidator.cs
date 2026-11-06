using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Repository.SystemBasicMgmt.UserSettings;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    /// <summary>
    /// 请假单、销假单送审校验
    /// </summary>
    public class LeaveValidator
    {
        private readonly LeaveRequestRepository _leaveRequest;
        private readonly LeaveCancellRepository _leaveCancell;
        private readonly UserAgentRepository _userAgentRepo;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveValidator(LeaveRequestRepository leaveRequest, LeaveCancellRepository leaveCancell, UserAgentRepository userAgentRepo, LocalizationService localization, Language lang)
        {
            _leaveRequest = leaveRequest;
            _leaveCancell = leaveCancell;
            _userAgentRepo = userAgentRepo;
            _localization = localization;
            _lang = lang;
        }

        /// <summary>
        /// 请假单送审校验
        /// </summary>
        public async Task<Result<bool>> ValidateLeaveRequest(long formId)
        {
            var currentLeave = await _leaveRequest.GetCurrentLeaveRequest(formId);

            // 必填项
            if (currentLeave.StartDateTime == null || currentLeave.EndDateTime == null)
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}LeaveTimeRequired"));

            var currentStart = currentLeave.StartDateTime.Value;
            var currentEnd = currentLeave.EndDateTime.Value;
            if (currentEnd <= currentStart)
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}LeaveTimeInvalid"));

            if (string.IsNullOrWhiteSpace(currentLeave.LeaveType))
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}LeaveTypeRequired"));

            // 批准后会写入代理关系，代理人必填
            if (!currentLeave.AgentUserId.HasValue)
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}AgentUserRequired"));

            var applicantUserId = await _leaveRequest.GetApplicantUserId(formId);

            // 1. 申请人、代理人在本次请假期间是否已有请假单或代理关系（任一角色时间重叠即冲突）
            var involvedUserIds = new List<long?> { applicantUserId, currentLeave.AgentUserId };

            // 审批中、已驳回的请假单（已批准的已写入代理关系表）
            var pendingConflicts = await _userAgentRepo.GetOverlappingPendingLeaves(involvedUserIds, formId, currentStart, currentEnd);

            // 已生效的代理关系
            var agentConflicts = await _userAgentRepo.GetOverlappingUserAgents(involvedUserIds, currentStart, currentEnd);

            var conflicts = pendingConflicts.Concat(agentConflicts).ToList();

            // 申请人冲突：已有重叠的请假，或正在代理他人
            var applicantConflict = conflicts.FirstOrDefault(conflict =>
                conflict.SubstituteUserId == applicantUserId || conflict.AgentUserId == applicantUserId);

            if (applicantConflict != null)
            {
                return Result<bool>.Failure(400, _localization.ReturnMsg(
                    $"{_form}ApplicantTimeConflict",
                    args: new object[]
                    {
                        applicantConflict.StartTime.ToString("yyyy-MM-dd HH:mm"),
                        applicantConflict.EndTime.ToString("yyyy-MM-dd HH:mm")
                    }
                ));
            }

            // 代理人冲突：本人也在请假/被代理，或已代理他人
            var agentConflict = conflicts.FirstOrDefault(conflict =>
                conflict.SubstituteUserId == currentLeave.AgentUserId || conflict.AgentUserId == currentLeave.AgentUserId);

            if (agentConflict != null)
            {
                return Result<bool>.Failure(400, _localization.ReturnMsg(
                    $"{_form}AgentUserTimeConflict",
                    args: new object[]
                    {
                        currentLeave.AgentUserName!,
                        agentConflict.StartTime.ToString("yyyy-MM-dd HH:mm"),
                        agentConflict.EndTime.ToString("yyyy-MM-dd HH:mm")
                    }
                ));
            }

            // 2. 只有 Annual、Sick 假别需要校验余额，其余假别直接通过
            var leaveType = currentLeave.LeaveType;
            if (leaveType != LeaveType.Annual.ToEnumString() && leaveType != LeaveType.Sick.ToEnumString())
                return Result<bool>.Ok(true);

            // 本次请假各年度工时，0 工时无需校验
            var currentHoursByYear = LeaveHoursCalculator.CalcHoursByYear(currentStart, currentEnd);
            if (currentHoursByYear.Count == 0)
                return Result<bool>.Ok(true);

            // 基础余额 - 其他审批中、已驳回请假单的占用
            var balanceList = await _leaveRequest.GetApplicantLeaveBalances(formId, currentHoursByYear.Keys.ToList());
            var pendingLeaves = await _leaveRequest.GetApplicantPendingLeaves(applicantUserId, formId);
            var pendingHours = CalcPendingHours(pendingLeaves);

            // 3. 按年份校验余额，第一个不足即返回
            foreach (var (year, currentHours) in currentHoursByYear.OrderBy(item => item.Key))
            {
                var availableHours = CalcAvailableHours(balanceList, year, leaveType, pendingHours);

                if (availableHours < currentHours)
                {
                    var leaveTypeDics = await _leaveRequest.GetLeaveTypeDictionary();
                    var leaveTypeDic = leaveTypeDics.FirstOrDefault(d => d.DicCode == leaveType);
                    var leaveTypeName = _lang.Locale == "zh-CN" ? leaveTypeDic?.DicNameCn : leaveTypeDic?.DicNameEn;

                    return Result<bool>.Failure(400, _localization.ReturnMsg(
                        $"{_form}LeaveBalanceNotEnough",
                        year,
                        leaveTypeName,
                        currentHours.ToString("0.##"),
                        availableHours.ToString("0.##")
                    ));
                }
            }

            return Result<bool>.Ok(true);
        }

        /// <summary>
        /// 销假单送审校验
        /// </summary>
        public async Task<Result<bool>> ValidateLeaveCancell(long leaveRequestId, long formId, DateTime? startDateTime, DateTime? endDateTime)
        {
            // 请假单本年度部分的总时数
            var leave = await _leaveCancell.GetLeaveRequest(leaveRequestId);
            var currentYear = DateTime.Now.Year;
            var totalHours = LeaveHoursCalculator.CalcHoursInYear((DateTime)leave!.StartDateTime!, (DateTime)leave!.EndDateTime!, currentYear);

            // 其他销假单（除作废）在本年度占用的时数
            var boundCancells = await _leaveCancell.GetBoundLeaveCancells(leaveRequestId);
            var otherCancells = boundCancells.Where(cancell => cancell.FormId != formId).ToList();
            var occupiedHours = otherCancells.Sum(cancell => CalcCancellHoursInYear(cancell, currentYear));

            // 校验销假时间段是否与其他销假单重叠
            if (startDateTime != null && endDateTime != null)
            {
                var overlapped = otherCancells.FirstOrDefault(cancell =>
                    cancell.StartDateTime != null && cancell.EndDateTime != null
                    && cancell.StartDateTime < endDateTime && startDateTime < cancell.EndDateTime);

                if (overlapped != null)
                {
                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}CancellPeriodOverlap", args: new object[]
                    {
                        overlapped.StartDateTime!.Value.ToString("yyyy-MM-dd HH:mm"),
                        overlapped.EndDateTime!.Value.ToString("yyyy-MM-dd HH:mm")
                    }));
                }
            }

            // 本次销假时数与剩余可销除时数比较
            var cancellHours = startDateTime == null || endDateTime == null
                ? 0m
                : LeaveHoursCalculator.CalcHoursInYear(startDateTime.Value, endDateTime.Value, currentYear);
            var remainingHours = totalHours - occupiedHours;

            // 没有本年度时数（未填或整段在往年）
            if (cancellHours <= 0)
            {
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}CancellHoursNone"));
            }

            if (cancellHours > remainingHours)
            {
                return Result<bool>.Failure(400, _localization.ReturnMsg($"{_form}CancellHoursExceed", args: new object[]
                {
                    cancellHours.ToString("0.##"),
                    remainingHours.ToString("0.##")
                }));
            }

            return Result<bool>.Ok(true);
        }

        /// <summary>
        /// 汇总请假单占用的工时（按年份、假别）
        /// </summary>
        public static Dictionary<(int Year, string LeaveType), decimal> CalcPendingHours(List<LeaveRequestEntity> leaves)
        {
            var pendingHours = new Dictionary<(int Year, string LeaveType), decimal>();

            foreach (var leave in leaves)
            {
                var hoursByYear = LeaveHoursCalculator.CalcHoursByYear(leave.StartDateTime!.Value, leave.EndDateTime!.Value);
                foreach (var (year, hours) in hoursByYear)
                {
                    var key = (year, leave.LeaveType!);
                    pendingHours[key] = pendingHours.GetValueOrDefault(key) + hours;
                }
            }

            return pendingHours;
        }

        /// <summary>
        /// 某年度某假别的可用时数 = 余额表剩余时数 - 其他请假单占用时数
        /// </summary>
        public static decimal CalcAvailableHours(IEnumerable<UserLeaveBalanceEntity> balances, int year, string leaveType, Dictionary<(int Year, string LeaveType), decimal> pendingHours)
        {
            var remainingHours = balances
                .Where(balance => balance.Year == year && balance.LeaveType == leaveType)
                .Sum(balance => balance.RemainingHours);

            return remainingHours - pendingHours.GetValueOrDefault((year, leaveType));
        }

        /// <summary>
        /// 销假单在指定年度内的时数（按时间段计算，不用已存的 CancellHours）
        /// </summary>
        public static decimal CalcCancellHoursInYear(LeaveCancellEntity cancell, int year)
        {
            return cancell.StartDateTime == null || cancell.EndDateTime == null
                ? 0m
                : LeaveHoursCalculator.CalcHoursInYear(cancell.StartDateTime.Value, cancell.EndDateTime.Value, year);
        }
    }
}
