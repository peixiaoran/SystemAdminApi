using Mapster;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormAudit.Entity;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Dto;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 表单基础
    /// </summary>
    public class FormManager
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly WorkflowRuleConditions _workflowRuleConditions;

        public FormManager(CurrentUser loginuser, SqlSugarScope db, Language lang, WorkflowRuleConditions workflowRuleConditions)
        {
            _loginuser = loginuser;
            _db = db;
            _lang = lang;
            _workflowRuleConditions = workflowRuleConditions;
        }

        /// <summary>
        /// 查询表单通知Token信息
        /// </summary>
        public async Task<FormNotificationUserDto> GetFormNotificationTokenWithUser(string tokenValue)
        {
            return await _db.Queryable<FormNotificationTokenEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<UserInfoEntity>((token, user) => token.ReviewUserId == user.UserId)
                            .Where((token, user) => token.Token == tokenValue)
                            .Select((token, user) => new FormNotificationUserDto
                            {
                                user = user,
                                FormId = token.FormId
                            })
                            .FirstAsync();
        }

        /// <summary>
        /// 初始化表单
        /// </summary>
        public async Task<string> InitializeFormInstance(long formTypeId)
        {
            var now = DateTime.Now;
            var ym = now.ToString("yyyyMM");
            var formId = SnowFlakeSingle.Instance.NextId();

            var prefix = await _db.Queryable<FormTypeEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(formtype => formtype.FormTypeId == formTypeId)
                                  .Select(formtype => formtype.Prefix)
                                  .FirstAsync();

            var startStepId = await _db.Queryable<WorkflowStepEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(step => step.FormTypeId == formTypeId && step.IsStartStep == 1)
                                       .Select(step => step.StepId)
                                       .FirstAsync();

            string formNo = string.Empty;

            await _db.Ado.UseTranAsync(async () =>
            {
                var lockKey = $"FormNo_{formTypeId}_{ym}";

                await _db.Ado.ExecuteCommandAsync(
                    "EXEC sp_getapplock @Resource = @lockKey, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000",
                    new { lockKey });

                var sequence = await _db.Queryable<FormSequenceEntity>()
                                        .Where(sequence => sequence.FormTypeId == formTypeId && sequence.Ym == ym)
                                        .FirstAsync();

                int nextNo;

                if (sequence == null)
                {
                    nextNo = 1;

                    await _db.Insertable(new FormSequenceEntity
                    {
                        FormTypeId = formTypeId,
                        Ym = ym,
                        Total = nextNo,
                        CreatedBy = _loginuser.UserId,
                        CreatedDate = now
                    }).ExecuteCommandAsync();
                }
                else
                {
                    nextNo = sequence.Total + 1;

                    await _db.Updateable<FormSequenceEntity>()
                             .SetColumns(s => new FormSequenceEntity
                             {
                                 Total = nextNo,
                                 ModifiedBy = _loginuser.UserId,
                                 ModifiedDate = now
                             })
                             .Where(s => s.FormTypeId == formTypeId && s.Ym == ym)
                             .ExecuteCommandAsync();
                }

                formNo = $"{prefix}-{ym}{nextNo:D4}";

                await _db.Insertable(new FormInstanceEntity
                {
                    FormId = formId,
                    FormTypeId = formTypeId,
                    FormNo = formNo,
                    FormStatus = FormStatus.PendingSubmit.ToEnumString(),
                    ApplicantUserId = _loginuser.UserId,
                    RuleId = 0,
                    CurrentStepId = startStepId,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                }).ExecuteCommandAsync();

                await _db.Insertable(new PendingReviewEntity
                {
                    FormId = formId,
                    StepId = startStepId,
                    AppointmentType = AppointmentType.Actual.ToEnumString(),
                    ReviewUserId = _loginuser.UserId
                }).ExecuteCommandAsync();
            });

            return formId.ToString();
        }

        /// <summary>
        /// 匹配工作流规则
        /// </summary>
        public async Task<long> MatchWorkflowRuleAsync(long formTypeId, long formId)
        {
            var appPositionId = await _db.Queryable<UserInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(user => user.UserId == _loginuser.UserId)
                                         .Select(user => user.PositionId)
                                         .FirstAsync();

            var ruleList = await _db.Queryable<WorkflowRuleEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(rule => rule.FormTypeId == formTypeId)
                                    .ToListAsync();

            long ruleId = 0;

            foreach (var rule in ruleList)
            {
                string? guidance = rule.Guidance;

                bool hasGuidance = !string.IsNullOrWhiteSpace(guidance);
                bool positionMatch = rule.PositionId == appPositionId;
                bool isDefaultRule = rule.PositionId == null && !hasGuidance;

                // 默认规则：职位和条件都不限制
                if (isDefaultRule)
                {
                    if (ruleId == 0)
                    {
                        ruleId = rule.RuleId;
                    }

                    continue;
                }

                // 非默认规则：Guidance 为空就跳过
                if (!hasGuidance)
                {
                    continue;
                }

                bool guidanceMatch = await _workflowRuleConditions.Resolve(guidance!, formId);

                // 优先级1：职位匹配，并且条件匹配
                if (positionMatch && guidanceMatch)
                {
                    ruleId = rule.RuleId;
                    break;
                }

                // 优先级2：职位不限制，只判断条件
                if (ruleId == 0 && rule.PositionId == null && guidanceMatch)
                {
                    ruleId = rule.RuleId;
                }
            }

            await _db.Updateable<FormInstanceEntity>()
                     .SetColumns(instance => new FormInstanceEntity
                     {
                         RuleId = ruleId
                     })
                     .Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();

            return ruleId;
        }

        /// <summary>
        /// 保存表单实例
        /// </summary>
        public async Task<int> SaveFormInstance(long formId)
        {
            var formTypeId = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(instance => instance.FormId == formId)
                                      .Select(instance => instance.FormTypeId)
                                      .FirstAsync();

            // 每次保存都重新匹配工作流规则
            await MatchWorkflowRuleAsync(formTypeId, formId);

            return await _db.Updateable<FormInstanceEntity>()
                            .SetColumns(f => new FormInstanceEntity
                            {
                                ModifiedBy = _loginuser.UserId,
                                ModifiedDate = DateTime.Now
                            })
                            .Where(instance => instance.FormId == formId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询附件列表
        /// </summary>
        public async Task<List<FormAttachmentDto>> GetAttachmentList(long formId)
        {
            var list = await _db.Queryable<FormAttachmentEntity>()
                                .With(SqlWith.NoLock)
                                .Where(formfile => formfile.FormId == formId)
                                .ToListAsync();

            return list.Adapt<List<FormAttachmentDto>>();
        }

        /// <summary>
        /// 新增附件
        /// </summary>
        public async Task<int> InsertAttachment(FormAttachmentEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除附件
        /// </summary>
        public async Task<int> DeleteAttachment(long attachmentId)
        {
            return await _db.Deleteable<FormAttachmentEntity>()
                            .Where(attach => attach.AttachmentId == attachmentId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询审批记录列表
        /// </summary>
        public async Task<List<FormReviewRecordDto>> GetReviewRecordList(long formId)
        {
            var list = await _db.Queryable<FormReviewRecordEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<WorkflowStepEntity>((record, step) => record.StepId == step.StepId)
                                .InnerJoin<DictionaryInfoEntity>((record, step, reviewresult) =>
                                    reviewresult.DicType == "ReviewResult" &&
                                    record.ReviewResult == reviewresult.DicCode)
                                .LeftJoin<WorkflowStepEntity>((record, step, reviewresult, rejectstep) =>
                                    record.RejectStepId == rejectstep.StepId)
                                .InnerJoin<DictionaryInfoEntity>((record, step, reviewresult, rejectstep, reviewtype) =>
                                    reviewtype.DicType == "ReviewType" &&
                                    record.ReviewType == reviewtype.DicCode)
                                .InnerJoin<DictionaryInfoEntity>((record, step, reviewresult, rejectstep, reviewtype, appointmenttype) =>
                                    appointmenttype.DicType == "AppointmentType" &&
                                    record.AppointmentType == appointmenttype.DicCode)
                                .InnerJoin<UserInfoEntity>((record, step, reviewresult, rejectstep, reviewtype, appointmenttype, originaluser) =>
                                    record.OriginalUserId == originaluser.UserId)
                                .InnerJoin<UserInfoEntity>((record, step, reviewresult, rejectstep, reviewtype, appointmenttype, originaluser, operationuser) =>
                                    record.OperationUserId == operationuser.UserId)
                                .Where((record, step, reviewresult, rejectstep, reviewtype, appointmenttype, originaluser, operationuser) =>
                                    record.FormId == formId)
                                .OrderBy((record, step, reviewresult, rejectstep, reviewtype, appointmenttype, originaluser, operationuser) =>
                                    record.ReviewDateTime)
                                .Select((record, step, reviewresult, rejectstep, reviewtype, appointmenttype, originaluser, operationuser) =>
                                    new FormReviewRecordDto
                                    {
                                        FormId = record.FormId,
                                        StepId = record.StepId,
                                        StepName = _lang.Locale == "zh-CN" ? step.StepNameCn : step.StepNameEn,
                                        ReviewResult = record.ReviewResult,
                                        ReviewResultName = _lang.Locale == "zh-CN" ? reviewresult.DicNameCn : reviewresult.DicNameEn,
                                        RejectStepName = _lang.Locale == "zh-CN" ? rejectstep.StepNameCn : rejectstep.StepNameEn,
                                        Comment = record.Comment,
                                        ReviewType = record.ReviewType,
                                        ReviewTypeName = _lang.Locale == "zh-CN" ? reviewtype.DicNameCn : reviewtype.DicNameEn,
                                        AppointmentType = record.AppointmentType,
                                        AppointmentTypeName = _lang.Locale == "zh-CN" ? appointmenttype.DicNameCn : appointmenttype.DicNameEn,
                                        OriginalUserName = _lang.Locale == "zh-CN" ? originaluser.UserNameCn : originaluser.UserNameEn,
                                        OperationUserName = _lang.Locale == "zh-CN" ? operationuser.UserNameCn : operationuser.UserNameEn,
                                        ReviewDateTime = record.ReviewDateTime,
                                    })
                                .ToListAsync();

            return list.Adapt<List<FormReviewRecordDto>>();
        }

        /// <summary>
        /// 查询步骤栏位权限列表
        /// </summary>
        public async Task<List<StepFieldPermissionDto>> GetStepFieldPermissionList(long formTypeId, long? stepId)
        {
            var list = await _db.Queryable<FormTypeFieldEntity>()
                                .With(SqlWith.NoLock)
                                .LeftJoin<StepFieldPermissionEntity>((formfield, fieldper) =>
                                    formfield.FieldId == fieldper.FieldId &&
                                    fieldper.StepId == stepId)
                                .Where((formfield, fieldper) => formfield.FormTypeId == formTypeId)
                                .Select((formfield, fieldper) => new StepFieldPermissionDto
                                {
                                    FieldKey = formfield.FieldKey,
                                    FieldName = _lang.Locale == "zh-CN"
                                                ? formfield.FieldNameCn
                                                : formfield.FieldNameEn,
                                    IsVisible = fieldper.IsVisible,
                                    IsDisabled = fieldper.IsDisabled,
                                })
                                .ToListAsync();

            return list.Adapt<List<StepFieldPermissionDto>>();
        }
    }
}