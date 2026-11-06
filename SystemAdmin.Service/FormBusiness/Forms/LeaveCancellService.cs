using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Utilities;
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
        private readonly LeaveValidator _leaveValidator;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveCancellService(CurrentUser loginuser, ILogger<LeaveCancellService> logger, SqlSugarScope db, FormPermissionChecker formchecker, LeaveCancellRepository leaveCancell, FormManager formmanger, LocalizationService localization, LeaveValidator leaveValidator)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _formChecker = formchecker;
            _leaveCancell = leaveCancell;
            _formmanger = formmanger;
            _localization = localization;
            _leaveValidator = leaveValidator;
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
                leaveCancellDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId, "Review");
                return Result<LeaveCancellDto>.Ok(leaveCancellDto);
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
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId, type);
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
                // 销假时数：仅计本年度部分，午休不计
                var cancellHours = save.StartDateTime == null || save.EndDateTime == null
                    ? 0m
                    : LeaveHoursCalculator.CalcHoursInYear((DateTime)save.StartDateTime, (DateTime)save.EndDateTime, DateTime.Now.Year);

                // 前端"删除绑定的请假单"按钮会把 LeaveRequestId 传空字符串，视为解绑
                long? leaveRequestId = string.IsNullOrEmpty(save.LeaveRequestId) ? null : long.Parse(save.LeaveRequestId);

                // 查询绑定请假单的单号
                var leaveRequestNo = leaveRequestId == null ? null : await _leaveCancell.GetLeaveRequestNo(leaveRequestId.Value);

                var entity = new LeaveCancellEntity()
                {
                    FormId = long.Parse(save.FormId),
                    LeaveRequestId = leaveRequestId,
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

                    // 请假单本年度部分的总时数
                    var totalHours = LeaveHoursCalculator.CalcHoursInYear(leaveStart, leaveEnd, currentYear);

                    // 扣除已绑定销假单在本年度占用的时数
                    var occupiedHours = boundCancells
                        .Where(cancell => cancell.LeaveRequestId == leave.LeaveRequestId)
                        .Sum(cancell => LeaveValidator.CalcCancellHoursInYear(cancell, currentYear));

                    var cancellableHours = totalHours - occupiedHours;

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
        /// 查询请假单明细
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
        /// 销假单送审校验
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<bool>> ValidateLeaveCancell(LeaveCancellSave save)
        {
            try
            {
                return await _leaveValidator.ValidateLeaveCancell(long.Parse(save.LeaveRequestId), long.Parse(save.FormId), save.StartDateTime, save.EndDateTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<bool>.Failure(500, ex.Message);
            }
        }
    }
}
