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
                        LeaveRequestId = 0,
                        LeaveRequestNo = string.Empty,
                        StartDateTime = null,
                        EndDateTime = null,
                        CancellHours = 0.00m,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now
                    };

                    await _leaveCancell.InitLeaveCancell(leaveCancell);
                    await _formmanger.MatchWorkflowRule(long.Parse(formTypeId), long.Parse(formId), _loginuser.UserId);
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
                var currentYear = DateTime.Now.Year;
                var cancellStart = save.StartDateTime;
                var cancellEnd = save.EndDateTime;

                decimal cancellHours = 0m;
                var cursor = cancellStart.Date;
                var lastDate = cancellEnd.Date;
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

                    if (cursor == cancellStart.Date && cursor == cancellEnd.Date)
                    {
                        // 同一天
                        if (cancellEnd <= morningEnd)
                            cancellHours += (decimal)(cancellEnd - cancellStart).TotalHours;
                        else if (cancellStart >= afternoonStart)
                            cancellHours += (decimal)(cancellEnd - cancellStart).TotalHours;
                        else
                            cancellHours += (decimal)(morningEnd - cancellStart).TotalHours + (decimal)(cancellEnd - afternoonStart).TotalHours;
                    }
                    else if (cursor == cancellStart.Date)
                    {
                        // 开始日期
                        if (cancellStart < morningEnd)
                            cancellHours += (decimal)(morningEnd - cancellStart).TotalHours + 4;
                        else
                            cancellHours += (decimal)(afternoonEnd - cancellStart).TotalHours;
                    }
                    else if (cursor == cancellEnd.Date)
                    {
                        // 结束日期
                        if (cancellEnd <= morningEnd)
                            cancellHours += (decimal)(cancellEnd - morningStart).TotalHours;
                        else
                            cancellHours += 4 + (decimal)(cancellEnd - afternoonStart).TotalHours;
                    }
                    else
                    {
                        // 中间的完整工作日
                        cancellHours += 8;
                    }

                    cursor = cursor.AddDays(1);
                }
                cancellHours = Math.Round(cancellHours, 2);

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

                // 查询这些请假单已绑定的销假单（审批中、已批准）
                var boundCancells = leaveList.Count > 0
                    ? await _leaveCancell.GetBoundLeaveCancells(leaveList.Select(leave => leave.LeaveRequestId).ToList())
                    : new List<LeaveCancellEntity>();

                var cancellableList = new List<LeaveRequestDto>();
                foreach (var leave in leaveList)
                {
                    if (leave.StartDateTime == null || leave.EndDateTime == null)
                        continue;

                    var leaveStart = (DateTime)leave.StartDateTime;
                    var leaveEnd = (DateTime)leave.EndDateTime;

                    // 计算请假单可消除的总时数（午休 12:00-13:00 不计入，去年及更早的部分不能消除）
                    decimal totalHours = 0m;
                    var cursor = leaveStart.Date;
                    var lastDate = leaveEnd.Date;
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

                        if (cursor == leaveStart.Date && cursor == leaveEnd.Date)
                        {
                            // 同一天
                            if (leaveEnd <= morningEnd)
                                totalHours += (decimal)(leaveEnd - leaveStart).TotalHours;
                            else if (leaveStart >= afternoonStart)
                                totalHours += (decimal)(leaveEnd - leaveStart).TotalHours;
                            else
                                totalHours += (decimal)(morningEnd - leaveStart).TotalHours + (decimal)(leaveEnd - afternoonStart).TotalHours;
                        }
                        else if (cursor == leaveStart.Date)
                        {
                            // 开始日期
                            if (leaveStart < morningEnd)
                                totalHours += (decimal)(morningEnd - leaveStart).TotalHours + 4;
                            else
                                totalHours += (decimal)(afternoonEnd - leaveStart).TotalHours;
                        }
                        else if (cursor == leaveEnd.Date)
                        {
                            // 结束日期
                            if (leaveEnd <= morningEnd)
                                totalHours += (decimal)(leaveEnd - morningStart).TotalHours;
                            else
                                totalHours += 4 + (decimal)(leaveEnd - afternoonStart).TotalHours;
                        }
                        else
                        {
                            // 中间的完整工作日
                            totalHours += 8;
                        }

                        cursor = cursor.AddDays(1);
                    }

                    // 扣除已绑定销假单占用的时数
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
    }
}
