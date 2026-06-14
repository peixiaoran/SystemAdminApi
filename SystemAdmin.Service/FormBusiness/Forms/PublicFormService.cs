using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Net;
using SystemAdmin.Common.EmailTemplates;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Upsert;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Dto;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Service.FormBusiness.Forms
{
    public class PublicFormService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<PublicFormService> _logger;
        private readonly JwtTokenService _jwt;
        private readonly SqlSugarScope _db;
        private readonly MailKitEmailSender _email;
        private readonly AppUrlOptions _formNotice;
        private readonly FileUploadOptions _attachmentUpload;
        private readonly MinioService _minioService;
        private readonly FormManager _formmanger;
        private readonly FormPermissionChecker _formchecker;
        private readonly FormReviewAction _formction;
        private readonly FormReviewFlow _formflow;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";
        private const string WorkflowLocalizationPrefix = "FormBusiness.Workflow";
        private readonly string _formLocalizationPrefix = "Form";

        public PublicFormService(CurrentUser loginuser, ILogger<PublicFormService> logger, SqlSugarScope db, MailKitEmailSender email, IOptions<AppUrlOptions> formNotice, IOptions<FileUploadOptions> attachmentUpload, MinioService minioService, JwtTokenService jwt, FormManager formmanger, FormPermissionChecker formchecker, FormReviewAction formaction, FormReviewFlow formflow, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _email = email;
            _formNotice = formNotice.Value;
            _attachmentUpload = attachmentUpload.Value;
            _minioService = minioService;
            _jwt = jwt;
            _formmanger = formmanger;
            _formchecker = formchecker;
            _formction = formaction;
            _formflow = formflow;
            _localization = localization;
        }

        /// <summary>
        /// 查询表单通知Token信息
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="tokenValue"></param>
        /// <returns></returns>
        public async Task<Result<FormNotificationReturnDto>> GetFormNotificationToken(HttpResponse httpResponse, string tokenValue)
        {
            var entity = await _formmanger.GetFormNotificationTokenWithUser(tokenValue);
            if (entity == null)
            {
                return Result<FormNotificationReturnDto>.Failure(400, _localization.ReturnMsg($"{_form}NotCanView"));
            }
            else
            {
                _jwt.SetAuthCookie(httpResponse, userId: entity.user.UserId, userNo: entity.user.UserNo);

                // 返回登录成功信息（JWT Cookie 已在其他层处理）
                return Result<FormNotificationReturnDto>.Ok(
                    new FormNotificationReturnDto
                    {
                        UserNo = entity.user.UserNo,
                        UserNameCn = entity.user.UserNameCn,
                        UserNameEn = entity.user.UserNameEn,
                        AvatarAddress = entity.user.AvatarAddress,
                        FormId = entity.FormId,
                    }, ""
                );
            }
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public async Task<Result<List<FormAttachmentDto>>> UploadAttachment(string formId, List<IFormFile> attachments)
        {
            try
            {
                if (attachments == null || attachments.Count == 0)
                {
                    return Result<List<FormAttachmentDto>>.Failure(400, _localization.ReturnMsg($"{_form}AttachmentNotNull"));
                }

                long maxAttachmentSize = _attachmentUpload.MaxSizeMB * 1024L * 1024L;
                var formAttachmentList = new List<FormAttachmentDto>();

                await _db.BeginTranAsync();
                foreach (var attachment in attachments)
                {
                    if (attachment == null || attachment.Length == 0)
                    {
                        return Result<List<FormAttachmentDto>>.Failure(400, _localization.ReturnMsg($"{_form}AttachmentNotNull"));
                    }
                    if (attachment.Length > maxAttachmentSize)
                    {
                        return Result<List<FormAttachmentDto>>.Failure(400, _localization.ReturnMsg($"{_form}AttachmentSizeLimit"));
                    }

                    var attachmentExt = Path.GetExtension(attachment.FileName)?.ToLowerInvariant();
                    if (string.IsNullOrWhiteSpace(attachmentExt) || !_attachmentUpload.AllowExtensions.Contains(attachmentExt))
                    {
                        return Result<List<FormAttachmentDto>>.Failure(400, _localization.ReturnMsg($"{_form}AttachmentExtensionNotAllow"));
                    }

                    using var stream = attachment.OpenReadStream();
                    var avatarUrl = await _minioService.UploadFile(attachment.FileName, stream, attachment.ContentType);

                    int attachmentSizeKb = (int)(attachment.Length / 1024);

                    var attachmentItem = new FormAttachmentEntity
                    {
                        AttachmentId = SnowFlakeSingle.Instance.NextId(),
                        FormId = long.Parse(formId),
                        AttachmentName = attachment.FileName,
                        AttachmentPath = avatarUrl.ToString(),
                        AttachmentSize = attachmentSizeKb,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = DateTime.Now
                    };
                    var attachmentItemDto = attachmentItem.Adapt<FormAttachmentDto>();
                    var count = await _formmanger.InsertAttachment(attachmentItem);
                    formAttachmentList.Add(attachmentItemDto);
                }
                await _db.CommitTranAsync();

                return Result<List<FormAttachmentDto>>.Ok(formAttachmentList, _localization.ReturnMsg($"{_form}UploadSuccess"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<List<FormAttachmentDto>>.Failure(500, _localization.ReturnMsg($"{_form}UploadFailed"));
            }
        }

        /// <summary>
        /// 删除附件
        /// </summary>
        /// <param name="attachmentId"></param>
        /// <param name="attachmentPath"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteAttachment(string attachmentId, string attachmentPath)
        {
            try
            {
                if (string.IsNullOrEmpty(attachmentId))
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_form}AttachmentIdNotNull"));
                }

                await _db.BeginTranAsync();
                var count = await _formmanger.DeleteAttachment(long.Parse(attachmentId));
                await _db.CommitTranAsync();

                await _minioService.DeleteFile(attachmentPath);
                return Result<int>.Ok(count, "");
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, _localization.ReturnMsg($"{_form}DeleteAttachmentFailed"));
            }
        }

        /// <summary>
        /// 查询驳回步骤下拉
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<RejectStepDrop>>> GetRejectStepDrop(string formId)
        {
            try
            {
                var drop = await _formflow.GetRejectStepDrop(long.Parse(formId));
                return Result<List<RejectStepDrop>>.Ok(drop);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<RejectStepDrop>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询完整审批流程
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<FormReview>> GetFullReviewFlow(string formId)
        {
            try
            {
                var fullflow = await _formflow.GetFullReviewFlow(long.Parse(formId));
                return Result<FormReview>.Ok(fullflow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<FormReview>.Failure(500, ex.Message);
            }
        }

        #region 核准与驳回

        /// <summary>
        /// 表单核准
        /// </summary>
        /// <param name="reviewForm"></param>
        /// <returns></returns>
        public Task<Result<bool>> FromApprove(ApproveForm reviewForm) =>
            ExecuteReviewAsync(
                reviewForm.FormId,
                ReviewResult.Approve,
                () => _formction.FromApprove(reviewForm));

        /// <summary>
        /// 表单驳回
        /// </summary>
        /// <param name="rejectForm"></param>
        /// <returns></returns>
        public Task<Result<bool>> FromReject(RejectForm rejectForm) =>
            ExecuteReviewAsync(
                rejectForm.FormId,
                ReviewResult.Reject,
                () => _formction.FromReject(rejectForm),
                rejectForm.RejectStepId);

        /// <summary>
        /// 统一执行签核或驳回
        /// </summary>
        /// <param name="formIdText"></param>
        /// <param name="operation"></param>
        /// <param name="reviewAction"></param>
        /// <param name="rejectStepIdText"></param>
        /// <returns></returns>
        private async Task<Result<bool>> ExecuteReviewAsync(string formId, ReviewResult operation, Func<Task<Result<bool>>> reviewAction, string? rejectStepIdText = null)
        {
            long? rejectStepId = null;
            if (operation == ReviewResult.Reject && !string.IsNullOrEmpty(rejectStepIdText))
            {
                rejectStepId = long.Parse(rejectStepIdText);
            }

            bool transactionStarted = false;
            long? originalStepId = null;

            try
            {
                await _db.BeginTranAsync();
                transactionStarted = true;

                bool canReview = await _formchecker.CanReview(long.Parse(formId));
                if (!canReview)
                {
                    await _db.RollbackTranAsync();
                    transactionStarted = false;

                    return Result<bool>.Failure(400, _localization.ReturnMsg($"{_formLocalizationPrefix}NotCanReview"));
                }

                if (operation == ReviewResult.Approve)
                {
                    originalStepId = await _db.Queryable<FormInstanceEntity>()
                                              .Where(instance => instance.FormId == long.Parse(formId))
                                              .Select(instance => instance.CurrentStepId)
                                              .FirstAsync();
                }

                Result<bool> result = await reviewAction();
                if (result.Code != 200)
                {
                    await _db.RollbackTranAsync();
                    transactionStarted = false;
                    return result;
                }

                await _db.CommitTranAsync();
                transactionStarted = false;

                try
                {
                    List<EmailMessage> messages = new();

                    if (operation == ReviewResult.Approve)
                    {
                        var state = await _db.Queryable<FormInstanceEntity>()
                                             .Where(instance => instance.FormId == long.Parse(formId))
                                             .Select(instance => new
                                             {
                                                 instance.FormStatus,
                                                 instance.CurrentStepId,
                                             }).FirstAsync();

                        if (state.FormStatus == FormStatus.Approved.ToEnumString())
                        {
                            messages = await BuildApplicantApprovedEmailsAsync(long.Parse(formId));
                        }
                        else if (state.CurrentStepId.HasValue && state.CurrentStepId != originalStepId)
                        {
                            messages = await BuildPendingReviewerEmailsAsync(
                                long.Parse(formId),
                                state.CurrentStepId.Value,
                                ReviewResult.Approve);
                        }
                    }
                    else if (rejectStepId.HasValue)
                    {
                        messages = await BuildPendingReviewerEmailsAsync(
                            long.Parse(formId),
                            rejectStepId.Value,
                            ReviewResult.Reject);
                    }

                    foreach (EmailMessage message in messages)
                    {
                        try
                        {
                            await _email.SendAsync(message);
                        }
                        catch (Exception emailException)
                        {
                            _logger.LogError(emailException, emailException.Message);
                        }
                    }
                }
                catch (Exception notificationException)
                {
                    _logger.LogError(notificationException, notificationException.Message);
                }

                return result;
            }
            catch (Exception ex)
            {
                if (transactionStarted)
                {
                    try
                    {
                        await _db.RollbackTranAsync();
                    }
                    catch (Exception rollbackException)
                    {
                        _logger.LogError(rollbackException, rollbackException.Message);
                    }
                }

                _logger.LogError(ex, ex.Message);

                return Result<bool>.Failure(500, ex.Message);
            }
        }

        #endregion

        #region 邮件通知

        /// <summary>
        /// 生成待审批人邮件，并写入邮件 Token。
        /// 此操作在主业务事务提交后执行。
        /// </summary>
        private async Task<List<EmailMessage>> BuildPendingReviewerEmailsAsync(long formId, long stepId, ReviewResult result)
        {
            DateTime now = DateTime.Now;

            var formNotice = await _db.Queryable<FormInstanceEntity>()
                                      .InnerJoin<FormTypeEntity>((instance, formType) =>
                                          instance.FormTypeId == formType.FormTypeId)
                                      .InnerJoin<UserInfoEntity>((instance, formType, applicant) =>
                                          instance.ApplicantUserId == applicant.UserId)
                                      .InnerJoin<FormReviewRecordEntity>((instance, formType, applicant, record) =>
                                          instance.FormId == record.FormId &&
                                          record.ReviewDateTime == SqlFunc.Subqueryable<FormReviewRecordEntity>()
                                                                          .Where(subRecord => subRecord.FormId == instance.FormId)
                                                                          .Max(subRecord => subRecord.ReviewDateTime))
                                      .InnerJoin<WorkflowStepEntity>((instance, formType, applicant, record, step) =>
                                          instance.CurrentStepId == step.StepId)
                                      .Where((instance, formType, applicant, record, step) =>
                                          instance.FormId == formId)
                                      .Select((instance, formType, applicant, record, step) =>
                                          new FormNoticeReviewDto
                                          {
                                              FormId = instance.FormId,
                                              FormNo = instance.FormNo,
                                              FormTypeNameCn = formType.FormTypeNameCn,
                                              FormTypeNameEn = formType.FormTypeNameEn,
                                              ApplicantUserCn = applicant.UserNameCn,
                                              ApplicantUserEn = applicant.UserNameEn,
                                              Comment = record.Comment,
                                              CurrentStepNameCn = step.StepNameCn,
                                              CurrentStepNameEn = step.StepNameEn,
                                              ReviewPath = formType.ReviewPath,
                                          })
                                      .FirstAsync();

            if (formNotice == null)
            {
                return new List<EmailMessage>();
            }

            List<long> pendingUserIds = await _db.Queryable<PendingReviewEntity>()
                                                 .Where(pending =>
                                                     pending.FormId == formId &&
                                                     pending.StepId == stepId)
                                                 .Select(pending => pending.ReviewUserId)
                                                 .ToListAsync();

            if (pendingUserIds.Count == 0)
            {
                return new List<EmailMessage>();
            }

            var agentRows = await _db.Queryable<UserAgentEntity>()
                                     .Where(agent =>
                                         pendingUserIds.Contains(agent.SubstituteUserId) &&
                                         agent.StartTime <= now &&
                                         agent.EndTime >= now)
                                     .Select(agent => new
                                     {
                                         agent.SubstituteUserId,
                                         agent.AgentUserId,
                                     })
                                     .ToListAsync();

            // 同一被代理人存在多条有效代理记录时，避免 ToDictionary 重复键异常。
            Dictionary<long, long> agentMap = agentRows
                                              .GroupBy(agent => agent.SubstituteUserId)
                                              .ToDictionary(
                                                  group => group.Key,
                                                  group => group.First().AgentUserId);

            List<long> notifyUserIds = pendingUserIds
                                       .Select(userId =>
                                           agentMap.TryGetValue(userId, out long agentUserId)
                                               ? agentUserId
                                               : userId)
                                       .Distinct()
                                       .ToList();

            List<UserInfoEntity> userList = await _db.Queryable<UserInfoEntity>()
                                                     .Where(user =>
                                                         notifyUserIds.Contains(user.UserId) &&
                                                         user.IsRealtimeNotification == 1 &&
                                                         !string.IsNullOrEmpty(user.Email))
                                                     .ToListAsync();

            if (userList.Count == 0)
            {
                return new List<EmailMessage>();
            }

            DateTime expirationTime = now.AddDays(15);
            Dictionary<long, string> tokens = userList.ToDictionary(
                user => user.UserId,
                _ => GenerateSecureToken());

            List<FormNotificationTokenEntity> tokenEntities = tokens
                                                              .Select(token => new FormNotificationTokenEntity
                                                              {
                                                                  FormId = formNotice.FormId,
                                                                  ReviewUserId = token.Key,
                                                                  Token = token.Value,
                                                                  ExpirationTime = expirationTime,
                                                                  CreatedDate = now,
                                                              })
                                                              .ToList();

            await _db.Insertable(tokenEntities).ExecuteCommandAsync();

            string template = result == ReviewResult.Approve
                ? EmailTemplateLoader.GetApproveNotice()
                : EmailTemplateLoader.GetRejectNotice();

            var messages = new List<EmailMessage>(userList.Count);

            foreach (UserInfoEntity user in userList)
            {
                string lang = user.NoticeLanguage;
                string rawName = lang == "zh-CN"
                    ? user.UserNameCn
                    : user.UserNameEn;
                string name = WebUtility.HtmlEncode(rawName ?? string.Empty);

                string titleKey = user.Gender switch
                {
                    1 => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleMale",
                    2 => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleFemale",
                    _ => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleDefault",
                };

                string title = _localization.ReturnMsg(titleKey, lang);
                string greeting = _localization.ReturnMsg(
                    $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTemplate",
                    lang,
                    name,
                    title);

                string headerTitle = _localization.ReturnMsg(
                    $"{WorkflowLocalizationPrefix}.EmailNoticePendingTitle",
                    lang);
                string subjectPrefix = headerTitle;
                string resultText = result == ReviewResult.Approve
                    ? _localization.ReturnMsg(
                        $"{WorkflowLocalizationPrefix}.EmailNoticeResultApprove",
                        lang)
                    : _localization.ReturnMsg(
                        $"{WorkflowLocalizationPrefix}.EmailNoticeResultReject",
                        lang);

                string formTypeName = lang == "zh-CN"
                    ? formNotice.FormTypeNameCn
                    : formNotice.FormTypeNameEn;
                string applicantUser = lang == "zh-CN"
                    ? formNotice.ApplicantUserCn
                    : formNotice.ApplicantUserEn;
                string currentStepName = lang == "zh-CN"
                    ? formNotice.CurrentStepNameCn
                    : formNotice.CurrentStepNameEn;

                string reviewUrl = BuildReviewUrl(
                    _formNotice.BaseDomain,
                    formNotice.ReviewPath,
                    lang,
                    tokens[user.UserId]);

                string body = template
                              .Replace("{{Title}}", WebUtility.HtmlEncode(headerTitle))
                              .Replace("{{Greeting}}", greeting)
                              .Replace("{{FormInfo}}", EncodeMessage("EmailNoticeFormInfo", lang))
                              .Replace("{{LabelFormNo}}", EncodeMessage("EmailNoticeLabelFormNo", lang))
                              .Replace("{{LabelFormType}}", EncodeMessage("EmailNoticeLabelFormType", lang))
                              .Replace("{{LabelApplicant}}", EncodeMessage("EmailNoticeLabelApplicant", lang))
                              .Replace("{{LabelResult}}", EncodeMessage("EmailNoticeLabelResult", lang))
                              .Replace("{{LabelComment}}", EncodeMessage("EmailNoticeLabelComment", lang))
                              .Replace("{{LabelStep}}", EncodeMessage("EmailNoticeLabelStep", lang))
                              .Replace("{{ResultText}}", WebUtility.HtmlEncode(resultText))
                              .Replace("{{BtnReview}}", EncodeMessage("EmailNoticeBtnReview", lang))
                              .Replace("{{BtnSignIn}}", EncodeMessage("EmailNoticeBtnSignIn", lang))
                              .Replace("{{ExpireHint}}", EncodeMessage("EmailNoticeExpireHint", lang))
                              .Replace("{{FooterText}}", EncodeMessage("EmailNoticeFooter", lang))
                              .Replace("{{FormNo}}", WebUtility.HtmlEncode(formNotice.FormNo ?? string.Empty))
                              .Replace("{{FormTypeName}}", WebUtility.HtmlEncode(formTypeName ?? string.Empty))
                              .Replace("{{ApplicantUser}}", WebUtility.HtmlEncode(applicantUser ?? string.Empty))
                              .Replace("{{Comment}}", WebUtility.HtmlEncode(formNotice.Comment ?? string.Empty))
                              .Replace("{{CurrentStepName}}", WebUtility.HtmlEncode(currentStepName ?? string.Empty))
                              .Replace("{{LoginUrl}}", _formNotice.LoginUrl)
                              .Replace("{{ReviewUrl}}", reviewUrl);

                messages.Add(new EmailMessage
                {
                    To = new List<string> { user.Email },
                    Subject = $"{subjectPrefix} {formNotice.FormNo} - {formTypeName}",
                    Body = body,
                });
            }

            return messages;
        }

        /// <summary>
        /// 生成申请人核准完成邮件，并写入邮件 Token。
        /// </summary>
        private async Task<List<EmailMessage>> BuildApplicantApprovedEmailsAsync(long formId)
        {
            DateTime now = DateTime.Now;
            string template = EmailTemplateLoader.GetApproveNotice();

            var formNotice = await _db.Queryable<FormInstanceEntity>()
                                      .InnerJoin<FormTypeEntity>((instance, formType) =>
                                          instance.FormTypeId == formType.FormTypeId)
                                      .InnerJoin<UserInfoEntity>((instance, formType, user) =>
                                          instance.ApplicantUserId == user.UserId)
                                      .Where((instance, formType, user) => instance.FormId == formId)
                                      .Select((instance, formType, user) =>
                                          new FormNoticeApprovedDto
                                          {
                                              FormId = instance.FormId,
                                              FormNo = instance.FormNo,
                                              FormTypeNameCn = formType.FormTypeNameCn,
                                              FormTypeNameEn = formType.FormTypeNameEn,
                                              ReviewPath = formType.ReviewPath,
                                              UserId = user.UserId,
                                              UserNameCn = user.UserNameCn,
                                              UserNameEn = user.UserNameEn,
                                              Gender = user.Gender,
                                              Email = user.Email,
                                              IsRealtimeNotification = user.IsRealtimeNotification,
                                              NoticeLanguage = user.NoticeLanguage,
                                          })
                                      .FirstAsync();

            if (formNotice == null)
            {
                return new List<EmailMessage>();
            }

            long agentUserId = await _db.Queryable<UserAgentEntity>()
                                        .Where(agent =>
                                            agent.SubstituteUserId == formNotice.UserId &&
                                            agent.StartTime <= now &&
                                            agent.EndTime >= now)
                                        .Select(agent => agent.AgentUserId)
                                        .FirstAsync();

            UserInfoEntity? recipient;

            if (agentUserId > 0)
            {
                recipient = await _db.Queryable<UserInfoEntity>()
                                     .Where(user =>
                                         user.UserId == agentUserId &&
                                         user.IsRealtimeNotification == 1 &&
                                         !string.IsNullOrEmpty(user.Email))
                                     .FirstAsync();
            }
            else
            {
                if (formNotice.IsRealtimeNotification != 1 ||
                    string.IsNullOrWhiteSpace(formNotice.Email))
                {
                    return new List<EmailMessage>();
                }

                recipient = new UserInfoEntity
                {
                    UserId = formNotice.UserId,
                    UserNameCn = formNotice.UserNameCn,
                    UserNameEn = formNotice.UserNameEn,
                    Gender = formNotice.Gender,
                    Email = formNotice.Email,
                    NoticeLanguage = formNotice.NoticeLanguage,
                };
            }

            if (recipient == null)
            {
                return new List<EmailMessage>();
            }

            string token = GenerateSecureToken();

            await _db.Insertable(new FormNotificationTokenEntity
            {
                FormId = formNotice.FormId,
                ReviewUserId = recipient.UserId,
                Token = token,
                ExpirationTime = now.AddDays(15),
                CreatedDate = now,
            }).ExecuteCommandAsync();

            string lang = recipient.NoticeLanguage;
            string rawName = lang == "zh-CN"
                ? recipient.UserNameCn
                : recipient.UserNameEn;
            string name = WebUtility.HtmlEncode(rawName ?? string.Empty);

            string titleKey = recipient.Gender switch
            {
                1 => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleMale",
                2 => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleFemale",
                _ => $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTitleDefault",
            };

            string title = _localization.ReturnMsg(titleKey, lang);
            string greeting = _localization.ReturnMsg(
                $"{WorkflowLocalizationPrefix}.EmailNoticeGreetingTemplate",
                lang,
                name,
                title);

            string headerTitle = _localization.ReturnMsg(
                $"{WorkflowLocalizationPrefix}.EmailNoticeApprovedTitle",
                lang);
            string subjectPrefix = _localization.ReturnMsg(
                $"{WorkflowLocalizationPrefix}.EmailNoticeSubjectApproved",
                lang);
            string resultText = _localization.ReturnMsg(
                $"{WorkflowLocalizationPrefix}.EmailNoticeResultApproved",
                lang);
            string formTypeName = lang == "zh-CN"
                ? formNotice.FormTypeNameCn
                : formNotice.FormTypeNameEn;
            string reviewUrl = BuildReviewUrl(
                _formNotice.BaseDomain,
                formNotice.ReviewPath,
                lang,
                token);

            string body = template
                          .Replace("{{Title}}", WebUtility.HtmlEncode(headerTitle))
                          .Replace("{{Greeting}}", greeting)
                          .Replace("{{LabelStep}}", EncodeMessage("EmailNoticeLabelStep", lang))
                          .Replace("{{FormInfo}}", EncodeMessage("EmailNoticeFormInfo", lang))
                          .Replace("{{LabelFormNo}}", EncodeMessage("EmailNoticeLabelFormNo", lang))
                          .Replace("{{LabelFormType}}", EncodeMessage("EmailNoticeLabelFormType", lang))
                          .Replace("{{LabelApplicant}}", EncodeMessage("EmailNoticeLabelApplicant", lang))
                          .Replace("{{LabelResult}}", EncodeMessage("EmailNoticeLabelResult", lang))
                          .Replace("{{LabelComment}}", EncodeMessage("EmailNoticeLabelComment", lang))
                          .Replace("{{ResultText}}", WebUtility.HtmlEncode(resultText))
                          .Replace("{{BtnReview}}", EncodeMessage("EmailNoticeBtnReview", lang))
                          .Replace("{{BtnSignIn}}", EncodeMessage("EmailNoticeBtnSignIn", lang))
                          .Replace("{{ExpireHint}}", EncodeMessage("EmailNoticeExpireHint", lang))
                          .Replace("{{FooterText}}", EncodeMessage("EmailNoticeFooter", lang))
                          .Replace("{{FormNo}}", WebUtility.HtmlEncode(formNotice.FormNo ?? string.Empty))
                          .Replace("{{FormTypeName}}", WebUtility.HtmlEncode(formTypeName ?? string.Empty))
                          .Replace("{{LoginUrl}}", _formNotice.LoginUrl)
                          .Replace("{{ReviewUrl}}", reviewUrl);

            return new List<EmailMessage>
            {
                new()
                {
                    To = new List<string> { recipient.Email },
                    Subject = $"{subjectPrefix} {formNotice.FormNo} - {formTypeName}",
                    Body = body,
                },
            };
        }

        private string EncodeMessage(string key, string lang) =>
            WebUtility.HtmlEncode(
                _localization.ReturnMsg(
                    $"{WorkflowLocalizationPrefix}.{key}",
                    lang));

        private static string GenerateSecureToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);

            return Convert.ToBase64String(buffer)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string BuildReviewUrl(
            string baseDomain,
            string reviewPath,
            string noticeLanguage,
            string token) =>
            $"{baseDomain}{reviewPath}?lang={noticeLanguage}&token={Uri.EscapeDataString(token)}";

        #endregion
    }
}
