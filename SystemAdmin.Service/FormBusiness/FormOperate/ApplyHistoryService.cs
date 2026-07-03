using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.PdfTemplates;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Repository.FormBusiness.FormOperate;
using SystemAdmin.Repository.FormBusiness.Forms;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.FormOperate
{
    public class ApplyHistoryService

    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<FormPendingService> _logger;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly FormPermissionChecker _formChecker;
        private readonly FormReviewAction _formReviewAction;
        private readonly ApplyHistoryRepository _applyHistoryRepo;
        private readonly LeaveFormRepository _leaveFormRepo;
        private readonly FormManager _formmanger;
        private readonly HtmlToPdfConverter _pdfConverter;
        private readonly LocalizationService _localization;
        private readonly string _this = "FormBusiness.FormOperate.FormPending";
        private readonly string _forms = "FormBusiness.Forms.";

        public ApplyHistoryService(CurrentUser loginuser, ILogger<FormPendingService> logger, SqlSugarScope db, Language lang, FormPermissionChecker formChecker, FormReviewAction formReviewAction, ApplyHistoryRepository applyHistoryRepo, LeaveFormRepository leaveFormRepo, FormManager formmanger, HtmlToPdfConverter pdfConverter, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _lang = lang;
            _formChecker = formChecker;
            _formReviewAction = formReviewAction;
            _applyHistoryRepo = applyHistoryRepo;
            _leaveFormRepo = leaveFormRepo;
            _formmanger = formmanger;
            _pdfConverter = pdfConverter;
            _localization = localization;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormGroupDropDto>>> GetFormGroupDrop()
        {
            try
            {
                var drop = await _applyHistoryRepo.GetFormGroupDrop();
                return Result<List<FormGroupDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormGroupDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormTypeDropDto>>> GetFormTypeDrop(string formGroupId)
        {
            try
            {
                var drop = await _applyHistoryRepo.GetFormTypeDrop(long.Parse(formGroupId));
                return Result<List<FormTypeDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormTypeDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单状态下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<FormStatusDropDto>>> GetFormStatusDrop()
        {
            try
            {
                var drop = await _applyHistoryRepo.GetFormStatusDrop();
                return Result<List<FormStatusDropDto>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormStatusDropDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询申请记录分页
        /// </summary>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage(GetFormHistoryPage getpage)
        {
            try
            {
                return await _applyHistoryRepo.GetApplyHistoryPage(getpage, _loginuser.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<FormHistoryDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询待审批人用户
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<List<FormPendingUserDto>>> GetFormPendingUsers(string formId)
        {
            try
            {
                var list = await _applyHistoryRepo.GetFormPendingUsers(long.Parse(formId));
                return Result<List<FormPendingUserDto>>.Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<FormPendingUserDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单撤回
        /// </summary>
        /// <returns></returns>
        public async Task<Result<int>> WithdrawForm(string formId)
        {
            try
            {
                var isCan = await _formChecker.CanWithdraw(long.Parse(formId));
                if (!isCan)
                {
                    return Result<int>.Ok(402, _localization.ReturnMsg($"{_this}NotCanWithdraw"));
                }

                await _db.BeginTranAsync();

                var stepInfo = await _applyHistoryRepo.GetStartStepInfo(long.Parse(formId));
                var pendingCount = await _applyHistoryRepo.WithdrawPendingSubmit(long.Parse(formId), _loginuser.UserId);
                var withCount = await _applyHistoryRepo.WithdrawForm(long.Parse(formId), stepInfo.StepId, _loginuser.UserId);

                var selfAppointments = await _formReviewAction.GetStepReviewUser(long.Parse(formId), stepInfo, _loginuser.UserId);
                var reacordsCount = await _applyHistoryRepo.InsertReviewRecords(long.Parse(formId), stepInfo.StepId, ReviewResult.Withdraw, null, selfAppointments, "", ReviewType.Manual, _loginuser.UserId);
                await _db.CommitTranAsync();

                return withCount >= 1
                        ? Result<int>.Ok(withCount, _localization.ReturnMsg($"{_this}WithdrawSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}WithdrawFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 表单作废
        /// </summary>
        /// <returns></returns>
        public async Task<Result<int>> VoidedForm(string formId)
        {
            try
            {
                var isCan = await _formChecker.CanVoided(long.Parse(formId));
                if (!isCan)
                {
                    return Result<int>.Ok(500, _localization.ReturnMsg($"{_this}NotCanVoided"));
                }

                await _db.BeginTranAsync();
                var count = await _applyHistoryRepo.VoidedForm(long.Parse(formId), _loginuser.UserId);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}VoidedSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VoidedFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 打印PDF（根据表单前缀分发，LVR=请假单）
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public async Task<Result<FormPdfDto>> PrintFormPdf(string formId, string prefix)
        {
            try
            {
                return prefix switch
                {
                    "LVR" => await PrintLeaveFormPdf(long.Parse(formId)),
                    _ => Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_this}PrintNotSupport"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormPdfDto>.Failure(500, ex.Message);
            }
        }

        #region 请假单PDF

        /// <summary>
        /// 生成请假单PDF
        /// </summary>
        private async Task<Result<FormPdfDto>> PrintLeaveFormPdf(long formId)
        {
            var isCan = await _formChecker.CanView(formId, "View");
            if (!isCan)
            {
                return Result<FormPdfDto>.Failure(400, _localization.ReturnMsg($"{_forms}NotCanView"));
            }

            // 表头、附件、审批记录、栏位权限（按当前登录用户）
            var form = await _leaveFormRepo.GetLeaveForm(formId);
            form.Attachment = await _formmanger.GetAttachmentList(formId);
            form.ReviewRecord = await _formmanger.GetReviewRecordList(formId);
            form.StepFieldPermission = await _formmanger.GetStepFieldPermissionList(formId, _loginuser.UserId);

            var html = await BuildLeaveFormHtml(form);
            var bytes = await _pdfConverter.ConvertAsync(html);

            var pdf = new FormPdfDto
            {
                FileName = $"{_localization.ReturnMsg($"{_forms}PdfLeaveTitle")}_{form.FormNo}.pdf",
                FileBytes = bytes
            };
            return Result<FormPdfDto>.Ok(pdf);
        }

        /// <summary>
        /// 填充请假单HTML模板
        /// </summary>
        private async Task<string> BuildLeaveFormHtml(LeaveFormDto form)
        {
            // 假别名称（按当前语言取字典）
            var leaveTypeDics = await _leaveFormRepo.GetLeaveTypeDictionary();
            var leaveTypeDic = leaveTypeDics.FirstOrDefault(dic => dic.DicCode == form.LeaveType);
            var leaveTypeName = _lang.Locale == "zh-CN" ? leaveTypeDic?.DicNameCn : leaveTypeDic?.DicNameEn;

            var html = PdfTemplateLoader.GetLeaveForm();

            html = html.Replace("{{Lang}}", _lang.Locale)
                       .Replace("{{Title}}", Encode(_localization.ReturnMsg($"{_forms}PdfLeaveTitle")))
                       .Replace("{{LabelFormNo}}", Encode(_localization.ReturnMsg($"{_forms}PdfFormNo")))
                       .Replace("{{LabelApplicantDate}}", Encode(_localization.ReturnMsg($"{_forms}PdfApplicantDate")))
                       .Replace("{{LabelUserNo}}", Encode(_localization.ReturnMsg($"{_forms}PdfUserNo")))
                       .Replace("{{LabelUserName}}", Encode(_localization.ReturnMsg($"{_forms}PdfUserName")))
                       .Replace("{{LabelDepartment}}", Encode(_localization.ReturnMsg($"{_forms}PdfDepartment")))
                       .Replace("{{LabelLeaveType}}", Encode(_localization.ReturnMsg($"{_forms}PdfLeaveType")))
                       .Replace("{{LabelLeavePeriod}}", Encode(_localization.ReturnMsg($"{_forms}PdfLeavePeriod")))
                       .Replace("{{LabelAgentUser}}", Encode(_localization.ReturnMsg($"{_forms}PdfAgentUser")))
                       .Replace("{{LabelLeaveHours}}", Encode(_localization.ReturnMsg($"{_forms}PdfLeaveHours")))
                       .Replace("{{LabelLeaveReason}}", Encode(_localization.ReturnMsg($"{_forms}PdfLeaveReason")))
                       .Replace("{{LabelAttachment}}", Encode(_localization.ReturnMsg($"{_forms}PdfAttachment")))
                       .Replace("{{LabelAttachmentName}}", Encode(_localization.ReturnMsg($"{_forms}PdfAttachmentName")))
                       .Replace("{{LabelAttachmentSize}}", Encode(_localization.ReturnMsg($"{_forms}PdfAttachmentSize")))
                       .Replace("{{LabelReviewRecord}}", Encode(_localization.ReturnMsg($"{_forms}PdfReviewRecord")))
                       .Replace("{{LabelReviewStep}}", Encode(_localization.ReturnMsg($"{_forms}PdfReviewStep")))
                       .Replace("{{LabelReviewUser}}", Encode(_localization.ReturnMsg($"{_forms}PdfReviewUser")))
                       .Replace("{{LabelReviewResult}}", Encode(_localization.ReturnMsg($"{_forms}PdfReviewResult")))
                       .Replace("{{LabelComment}}", Encode(_localization.ReturnMsg($"{_forms}PdfComment")))
                       .Replace("{{LabelReviewTime}}", Encode(_localization.ReturnMsg($"{_forms}PdfReviewTime")))
                       .Replace("{{FormNo}}", Encode(form.FormNo))
                       .Replace("{{ApplicantDate}}", form.ApplicantDate.ToString("yyyy-MM-dd"))
                       .Replace("{{UserNo}}", Encode(form.ApplicantUserNo))
                       .Replace("{{UserName}}", Encode(form.ApplicantUserName))
                       .Replace("{{Department}}", Encode(form.ApplicantDeptName))
                       .Replace("{{LeaveTypeName}}", Encode(leaveTypeName))
                       .Replace("{{StartDateTime}}", form.StartDateTime.HasValue ? $"{form.StartDateTime:yyyy-MM-dd HH:mm:ss}" : string.Empty)
                       .Replace("{{EndDateTime}}", form.EndDateTime.HasValue ? $"{form.EndDateTime:yyyy-MM-dd HH:mm:ss}" : string.Empty)
                       .Replace("{{AgentUserName}}", Encode(form.AgentUserName))
                       .Replace("{{LeaveHours}}", (form.LeaveHours ?? 0).ToString("0.00"))
                       .Replace("{{LeaveReason}}", Encode(form.LeaveReason))
                       .Replace("{{AttachmentRows}}", BuildAttachmentRows(form.Attachment))
                       .Replace("{{ReviewRecordRows}}", BuildReviewRecordRows(form.ReviewRecord));

            // 栏位权限：仅根据当前登录用户判断是否显示
            html = ApplyFieldVisibility(html, form.StepFieldPermission);
            return html;
        }

        /// <summary>
        /// 拼接附件表格行
        /// </summary>
        private string BuildAttachmentRows(List<FormAttachmentDto> attachments)
        {
            if (attachments.Count == 0)
            {
                return $"<tr><td class=\"no-data\" colspan=\"3\">{Encode(_localization.ReturnMsg($"{_forms}PdfNoData"))}</td></tr>";
            }

            var rows = new StringBuilder();
            var index = 1;
            foreach (var attachment in attachments)
            {
                rows.Append("<tr>")
                    .Append($"<td>{index++}</td>")
                    .Append($"<td>{Encode(attachment.AttachmentName)}</td>")
                    .Append($"<td>{attachment.AttachmentSize} KB</td>")
                    .Append("</tr>");
            }
            return rows.ToString();
        }

        /// <summary>
        /// 拼接审批记录表格行
        /// </summary>
        private string BuildReviewRecordRows(List<FormReviewRecordDto> records)
        {
            if (records.Count == 0)
            {
                return $"<tr><td class=\"no-data\" colspan=\"6\">{Encode(_localization.ReturnMsg($"{_forms}PdfNoData"))}</td></tr>";
            }

            var rows = new StringBuilder();
            var index = 1;
            foreach (var record in records)
            {
                rows.Append("<tr>")
                    .Append($"<td>{index++}</td>")
                    .Append($"<td>{Encode(record.StepName)}</td>")
                    .Append($"<td>{Encode(record.OperationUserName)}</td>")
                    .Append($"<td><span class=\"badge\">{Encode(record.ReviewResultName)}</span></td>")
                    .Append($"<td>{Encode(record.Comment)}</td>")
                    .Append($"<td>{record.ReviewDateTime:yyyy-MM-dd HH:mm:ss}</td>")
                    .Append("</tr>");
            }
            return rows.ToString();
        }

        /// <summary>
        /// 按栏位可见性裁剪模板（IsVisible != 1 的栏位整块移除）
        /// </summary>
        private static string ApplyFieldVisibility(string html, List<StepFieldPermissionDto> permissions)
        {
            foreach (var permission in permissions.Where(p => p.IsVisible != 1))
            {
                var fieldKey = Regex.Escape(permission.FieldKey);
                html = Regex.Replace(html, $"<!--BEGIN:{fieldKey}-->[\\s\\S]*?<!--END:{fieldKey}-->", string.Empty);
            }

            // 清理剩余的可见性标记注释
            return Regex.Replace(html, "<!--(BEGIN|END):[A-Za-z0-9_]+-->", string.Empty);
        }

        /// <summary>
        /// HTML转义（空值返回空字符串）
        /// </summary>
        private static string Encode(string? value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : WebUtility.HtmlEncode(value);
        }

        #endregion
    }
}
