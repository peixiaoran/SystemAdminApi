using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveCancell.Queries;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class LeaveCancellService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<LeaveCancellService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveCancellRepository _leaveCancell;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveCancellService(CurrentUser loginuser, ILogger<LeaveCancellService> logger, SqlSugarScope db, FormPermissionChecker formchecker, LeaveCancellRepository leaveCancell, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _leaveCancell = leaveCancell;
            _formmanger = formmanger;
            _localization = localization;
        }

        /// <summary>
        /// 初始化销假单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveCancellDto>> InitLeaveCancell(string formTypeId)
        {
            try
            {
                var isCan = await _formChecker.CanApply(long.Parse(formTypeId));
                if (!isCan)
                {
                    return Result<LeaveCancellDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanApply"));
                }
                else
                {
                    await _db.BeginTranAsync();
                    var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                    var leaveCancell = new LeaveCancellEntity()
                    {
                        FormId = long.Parse(formId),
                        LeaveRequestId = null,
                        LeaveRequestNo = null,
                        StartDateTime = null,
                        EndDateTime = null,
                        CancellHours = 0.00m,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now
                    };

                    await _leaveCancell.InitLeaveCancell(leaveCancell);
                    await _formmanger.MatchWorkflowRule(long.Parse(formId));
                    await _db.CommitTranAsync();

                    var leaveCancellDto = await _leaveCancell.GetLeaveCancell(long.Parse(formId));
                    leaveCancellDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                    leaveCancellDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId);
                    return Result<LeaveCancellDto>.Ok(leaveCancellDto);
                }
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<LeaveCancellDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询销假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Result<LeaveCancellDto>> GetLeaveCancell(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<LeaveCancellDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _leaveCancell.GetLeaveCancell(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId);
                return Result<LeaveCancellDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<LeaveCancellDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存销假单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveLeaveCancell(LeaveCancellSave save)
        {
            try
            {
                // 计算销假时数（午休 12:00-13:00 不计入，去年及更早的部分不能消除）
                var cancellHours = Math.Round(CalcWorkingHours(save.StartDateTime, save.EndDateTime), 2);

                // 查询绑定请假单的单号
                var leaveRequestNo = await _leaveCancell.GetLeaveRequestNo(long.Parse(save.LeaveRequestId));

                var entity = new LeaveCancellEntity()
                {
                    FormId = long.Parse(save.FormId),
                    LeaveRequestId = long.Parse(save.LeaveRequestId),
                    LeaveRequestNo = leaveRequestNo,
                    StartDateTime = save.StartDateTime,
                    EndDateTime = save.EndDateTime,
                    CancellHours = cancellHours,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _leaveCancell.SaveLeaveCancell(entity);
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
        /// 查询登录用户可销假的请假单
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<LeaveRequestDto>> GetLeaveRequestView(GetLeaveRequestPage getPage)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                var yearStart = new DateTime(currentYear, 1, 1);

                // 查询登录用户已批准的请假单
                var leaveList = await _leaveCancell.GetUserApprovedLeaveRequests(getPage, _loginuser.UserId, yearStart);

                // 查询登录用户名下已绑定的销假单（除作废外都计入）
                var boundCancells = await _leaveCancell.GetUserBoundLeaveCancells(_loginuser.UserId);

                var cancellableList = new List<LeaveRequestDto>();
                foreach (var leave in leaveList)
                {
                    if (leave.StartDateTime == null || leave.EndDateTime == null)
                        continue;

                    var leaveStart = (DateTime)leave.StartDateTime;
                    var leaveEnd = (DateTime)leave.EndDateTime;

                    // 计算请假单可消除的总时数（午休 12:00-13:00 不计入，去年及更早的部分不能消除）
                    decimal totalHours = CalcWorkingHours(leaveStart, leaveEnd);

                    // 扣除该请假单已绑定销假单占用的时数
                    var occupiedHours = boundCancells
                        .Where(cancell => cancell.LeaveRequestId == leave.LeaveRequestId)
                        .Sum(cancell => cancell.CancellHours ?? 0);

                    var cancellableHours = Math.Round(totalHours - occupiedHours, 2);

                    // 可销假时数小于等于 0 的不显示在选择范围中
                    if (cancellableHours <= 0)
                        continue;

                    leave.CancellableHours = cancellableHours;
                    cancellableList.Add(leave);
                }

                // 内存分页（可销假时数需计算后才能过滤）
                var page = cancellableList
                    .Skip((getPage.PageIndex - 1) * getPage.PageSize)
                    .Take(getPage.PageSize)
                    .ToList();

                return ResultPaged<LeaveRequestDto>.Ok(page, cancellableList.Count, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<LeaveRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询请假单明细（含假别名称）
        /// </summary>
        /// <param name="leaveRequestId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveRequestDetailDto>> GetLeaveRequestDetail(string leaveRequestId)
        {
            try
            {
                var detail = await _leaveCancell.GetLeaveRequestDetail(long.Parse(leaveRequestId));
                if (detail == null)
                {
                    return Result<LeaveRequestDetailDto>.Failure(400, _localization.ReturnMsg($"{_form}LeaveRequestNotFound"));
                }
                return Result<LeaveRequestDetailDto>.Ok(detail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<LeaveRequestDetailDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询请假单的剩余可销假时数
        /// </summary>
        /// <param name="leaveRequestId"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<decimal>> GetRemainingCancellHours(string leaveRequestId, string formId)
        {
            try
            {
                var requestId = long.Parse(leaveRequestId);
                var cancellFormId = long.Parse(formId);

                // 请假单可销除的总时数（午休 12:00-13:00 不计入，去年及更早的部分不能消除）
                var leave = await _leaveCancell.GetLeaveRequest(requestId);
                if (leave == null || leave.StartDateTime == null || leave.EndDateTime == null)
                {
                    return Result<decimal>.Failure(400, _localization.ReturnMsg($"{_form}LeaveRequestNotFound"));
                }
                var totalHours = CalcWorkingHours((DateTime)leave.StartDateTime, (DateTime)leave.EndDateTime);

                // 已被除作废外的销假单占用的时数（排除本单自身）
                var boundCancells = await _leaveCancell.GetBoundLeaveCancells(requestId);
                var occupiedHours = boundCancells
                    .Where(cancell => cancell.FormId != cancellFormId)
                    .Sum(cancell => cancell.CancellHours ?? 0);

                var remainingHours = Math.Round(totalHours - occupiedHours, 2);
                return Result<decimal>.Ok(remainingHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<decimal>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 销假单送审校验
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<bool>> ValidateLeaveCancell(LeaveCancellSave save)
        {
            try
            {
                var leaveRequestId = long.Parse(save.LeaveRequestId);
                var formId = long.Parse(save.FormId);

                // 请假单可销除的总时数
                var leave = await _leaveCancell.GetLeaveRequest(leaveRequestId);
                var totalHours = CalcWorkingHours((DateTime)leave!.StartDateTime!, (DateTime)leave!.EndDateTime!);

                // 已被除作废外的销假单占用的时数
                var boundCancells = await _leaveCancell.GetBoundLeaveCancells(leaveRequestId);
                var occupiedHours = boundCancells
                    .Where(cancell => cancell.FormId != formId)
                    .Sum(cancell => cancell.CancellHours ?? 0);

                // 本次销假时数与剩余可销除时数比较
                var cancellHours = Math.Round(CalcWorkingHours(save.StartDateTime, save.EndDateTime), 2);
                var remainingHours = Math.Round(totalHours - occupiedHours, 2);

                if (cancellHours > remainingHours)
                {
                    return Result<bool>.Failure(402, _localization.ReturnMsg($"{_form}CancellHoursExceed", args: new object[]
                    {
                        cancellHours.ToString("0.##"),
                        remainingHours.ToString("0.##")
                    }));
                }

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<bool>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 计算指定时间段的工作时数
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns></returns>
        private static decimal CalcWorkingHours(DateTime start, DateTime end)
        {
            var currentYear = DateTime.Now.Year;

            decimal hours = 0m;
            var cursor = start.Date;
            var lastDate = end.Date;
            while (cursor <= lastDate)
            {
                if (cursor.Year < currentYear)
                {
                    cursor = cursor.AddDays(1);
                    continue;
                }

                var morningStart = cursor.AddHours(8);
                var morningEnd = cursor.AddHours(12);
                var afternoonStart = cursor.AddHours(13);
                var afternoonEnd = cursor.AddHours(17);

                if (cursor == start.Date && cursor == end.Date)
                {
                    // 同一天
                    if (end <= morningEnd)
                        hours += (decimal)(end - start).TotalHours;
                    else if (start >= afternoonStart)
                        hours += (decimal)(end - start).TotalHours;
                    else
                        hours += (decimal)(morningEnd - start).TotalHours + (decimal)(end - afternoonStart).TotalHours;
                }
                else if (cursor == start.Date)
                {
                    // 开始日期
                    if (start < morningEnd)
                        hours += (decimal)(morningEnd - start).TotalHours + 4;
                    else
                        hours += (decimal)(afternoonEnd - start).TotalHours;
                }
                else if (cursor == end.Date)
                {
                    // 结束日期
                    if (end <= morningEnd)
                        hours += (decimal)(end - morningStart).TotalHours;
                    else
                        hours += 4 + (decimal)(end - afternoonStart).TotalHours;
                }
                else
                {
                    // 中间的完整工作日
                    hours += 8;
                }

                cursor = cursor.AddDays(1);
            }

            return hours;
        }
    }
}
