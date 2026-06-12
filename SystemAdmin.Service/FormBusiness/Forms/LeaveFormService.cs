using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Commands;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class LeaveFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<LeaveFormService> _logger;
        private readonly SqlSugarScope _db;
        private readonly FormPermissionChecker _formChecker;
        private readonly LeaveFormRepository _leaveForm;
        private readonly FormManager _formmanger;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public LeaveFormService(CurrentUser loginuser, ILogger<LeaveFormService> logger, SqlSugarScope db, FormPermissionChecker formchecker,  LeaveFormRepository leaveForm, FormManager formmanger, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
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
        /// 初始化表单
        /// </summary>
        /// <param name="formTypeId"></param>
        /// <returns></returns>
        public async Task<Result<LeaveFormDto>> InitializeLevel(string formTypeId)
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
                    var formId = await _formmanger.InitializeFormInstance(long.Parse(formTypeId));

                    var leaveForm = new LeaveFormEntity()
                    {
                        FormId = long.Parse(formId),
                        LeaveType = null,
                        LeaveReason = null,
                        StartDateTime = null,
                        EndDateTime = null,
                        LeaveHours = 0.00M,
                        AgentUserId = null,
                        AgentUserName = null,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now
                    };

                    await _leaveForm.InitLeaveForm(leaveForm);
                    await _formmanger.MatchWorkflowRuleAsync(long.Parse(formTypeId), long.Parse(formId));
                    await _db.CommitTranAsync();

                    var leaveFormDto = await _leaveForm.GetLeaveForm(long.Parse(formId));
                    leaveFormDto.Attachment = await _formmanger.GetAttachmentList(long.Parse(formId));
                    leaveFormDto.ReviewRecord = await _formmanger.GetReviewRecordList(long.Parse(formId));
                    leaveFormDto.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(long.Parse(formTypeId), leaveFormDto.CurrentStepId);
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
                form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(form.FormTypeId, form.CurrentStepId);
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
            if (endDateTime <= startDateTime)
            {
                return 0m;
            }

            decimal totalHours = 0m;

            TimeSpan defaultWorkStart = new TimeSpan(8, 0, 0);
            TimeSpan lunchStart = new TimeSpan(12, 0, 0);
            TimeSpan lunchEnd = new TimeSpan(13, 0, 0);
            TimeSpan defaultWorkEnd = new TimeSpan(17, 0, 0);

            DateTime currentDate = startDateTime.Date;
            DateTime lastDate = endDateTime.Date;

            while (currentDate <= lastDate)
            {
                DateTime dayStart = currentDate.Add(defaultWorkStart);
                DateTime dayEnd = currentDate.Add(defaultWorkEnd);

                // 如果是开始当天，并且开始时间早于 08:00，则从实际开始时间算
                if (currentDate == startDateTime.Date && startDateTime < dayStart)
                {
                    dayStart = startDateTime;
                }

                // 如果是结束当天，并且结束时间晚于 17:00，则算到实际结束时间
                if (currentDate == endDateTime.Date && endDateTime > dayEnd)
                {
                    dayEnd = endDateTime;
                }

                DateTime morningStart = dayStart;
                DateTime morningEnd = currentDate.Add(lunchStart);

                DateTime afternoonStart = currentDate.Add(lunchEnd);
                DateTime afternoonEnd = dayEnd;

                totalHours += GetHours(morningStart, morningEnd);
                totalHours += GetHours(afternoonStart, afternoonEnd);

                currentDate = currentDate.AddDays(1);
            }

            return Math.Round(totalHours, 2);

            decimal GetHours(DateTime periodStart, DateTime periodEnd)
            {
                DateTime actualStart = startDateTime > periodStart
                    ? startDateTime
                    : periodStart;

                DateTime actualEnd = endDateTime < periodEnd
                    ? endDateTime
                    : periodEnd;

                if (actualEnd <= actualStart)
                {
                    return 0m;
                }

                return (decimal)(actualEnd - actualStart).TotalHours;
            }
        }

        #endregion
    }
}
