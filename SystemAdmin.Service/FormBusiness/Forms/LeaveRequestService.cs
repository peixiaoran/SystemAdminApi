using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Queries;
using SystemAdmin.Model.HR.BasicInfo.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;
using SystemAdmin.Repository.SystemBasicMgmt.UserSettings;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class LeaveRequestService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<LeaveRequestService> _logger;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveRequestRepository _leaveRequest;
        private readonly UserAgentRepository _userAgentRepo;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly LeaveValidator _leaveValidator;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveRequestService(CurrentUser loginuser, ILogger<LeaveRequestService> logger, SqlSugarScope db, Language lang, FormPermissionChecker formchecker, LeaveRequestRepository leaveRequest, UserAgentRepository userAgentRepo, FormManager formmanger, LocalizationService localization, LeaveValidator leaveValidator)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _lang = lang;
            _leaveRequest = leaveRequest;
            _userAgentRepo = userAgentRepo;
            _formChecker = formchecker;
            _formmanger = formmanger;
            _localization = localization;
            _leaveValidator = leaveValidator;
        }

        /// <summary>
        /// 请假类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<LeaveTypeDropDto>>> GetLeaveTypeDrop()
        {
            var drop = await _leaveRequest.GetLeaveTypeDrop();
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
                var drop = await _leaveRequest.GetDepartmentDrop();
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
                // 按当前表单的请假时间段，筛掉该时段内已有请假或代理安排的用户；表单未填时间时不筛，由送审校验兜底
                var currentLeave = await _leaveRequest.GetCurrentLeaveRequest(long.Parse(getPage.FormId));
                var occupiedUserIds = currentLeave.StartDateTime.HasValue && currentLeave.EndDateTime.HasValue
                    ? await _userAgentRepo.GetOccupiedUserIds(currentLeave.StartDateTime.Value, currentLeave.EndDateTime.Value, long.Parse(getPage.FormId))
                    : new List<long>();

                return await _leaveRequest.GetUserInfoAgentView(getPage, occupiedUserIds);
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

            // 申请人基础余额
            var balanceList = await _leaveRequest.GetApplicantLeaveBalances(long.Parse(formId), yearList);

            // 其他审批中、已驳回请假单占用的工时（已批准的已直接扣减余额表）
            var applicantUserId = await _leaveRequest.GetApplicantUserId(long.Parse(formId));
            var pendingLeaves = await _leaveRequest.GetApplicantPendingLeaves(applicantUserId, long.Parse(formId));
            var pendingHours = LeaveValidator.CalcPendingHours(pendingLeaves);

            var result = balanceList
                .GroupBy(balance => balance.Year)
                .Select(yearGroup => new LeaveBalanceDto
                {
                    Year = yearGroup.Key,
                    AnnualRemainingHours = LeaveValidator.CalcAvailableHours(yearGroup, yearGroup.Key, LeaveType.Annual.ToEnumString(), pendingHours),
                    SickRemainingHours = LeaveValidator.CalcAvailableHours(yearGroup, yearGroup.Key, LeaveType.Sick.ToEnumString(), pendingHours)
                })
                .OrderBy(dto => dto.Year)
                .ToList();

            return Result<List<LeaveBalanceDto>>.Ok(result);
        }

        /// <summary>
        /// 请假单送审验证
        /// </summary>
        public async Task<Result<bool>> ValidateLeaveRequest(string formId)
        {
            try
            {
                return await _leaveValidator.ValidateLeaveRequest(long.Parse(formId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<bool>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 初始化请假单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveRequestDto>> InitLeaveRequest(string formTypeId)
        {
            try
            {
                await _db.BeginTranAsync();
                var formId = await _formmanger.InitFormInstance(long.Parse(formTypeId));

                var leaveRequest = new LeaveRequestEntity()
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

                await _leaveRequest.InitLeaveRequest(leaveRequest);
                await _formmanger.MatchWorkflowRule(long.Parse(formId));
                await _db.CommitTranAsync();

                var leaveRequestDto = await _leaveRequest.GetLeaveRequest(long.Parse(formId));
                leaveRequestDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                leaveRequestDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                leaveRequestDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formId), _loginuser.UserId, "Review");
                return Result<LeaveRequestDto>.Ok(leaveRequestDto);
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<LeaveRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询请假单明细
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveRequestDto>> GetLeaveRequest(string formId, string type)
        {
            try
            {
                var isCan = await _formChecker.CanView(long.Parse(formId), type);
                if (!isCan)
                {
                    return Result<LeaveRequestDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
                }

                var form = await _leaveRequest.GetLeaveRequest(long.Parse(formId));
                form.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                form.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormId, _loginuser.UserId, type);
                return Result<LeaveRequestDto>.Ok(form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<LeaveRequestDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 保存请假单
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public async Task<Result<int>> SaveLeaveRequest(LeaveRequestSave save)
        {
            try
            {
                var entity = new LeaveRequestEntity()
                {
                    FormId = long.Parse(save.FormId),
                    LeaveType = save.LeaveType,
                    LeaveReason = save.LeaveReason,
                    StartDateTime = save.StartDateTime,
                    EndDateTime = save.EndDateTime,
                    LeaveHours = LeaveHoursCalculator.CalcTotalHours(save.StartDateTime, save.EndDateTime),
                    AgentUserId = string.IsNullOrWhiteSpace(save.AgentUserId) ? null : long.Parse(save.AgentUserId),
                    AgentUserName = save.AgentUserName,
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now
                };
                await _db.BeginTranAsync();
                var count = await _leaveRequest.SaveLeaveRequest(entity);
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
    }
}
