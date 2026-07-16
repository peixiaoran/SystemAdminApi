using Mapster;
using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
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
    /// 表单基础😈
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
        public async Task<FormNotifyUserDto> GetFormNotifyTokenWithUser(string tokenValue)
        {
            return await _db.Queryable<FormNotifyTokenEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<UserInfoEntity>((token, user) => token.ReviewUserId == user.UserId)
                            .Where((token, user) => token.Token == tokenValue)
                            .Select((token, user) => new FormNotifyUserDto
                            {
                                user = user,
                                FormId = token.FormId
                            }).FirstAsync();
        }

        /// <summary>
        /// 初始化表单
        /// </summary>
        public async Task<string> InitFormInstance(long formTypeId)
        {
            var now = DateTime.Now;
            var ym = now.ToString("yyyyMM");
            var formId = SnowFlakeSingle.Instance.NextId();

            var prefix = await _db.Queryable<FormTypeEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(formtype => formtype.FormTypeId == formTypeId)
                                  .Select(formtype => formtype.Prefix)
                                  .FirstAsync();

            long? startStepId = await _db.Queryable<WorkflowStepEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(step => step.FormTypeId == formTypeId && step.IsStartStep == 1)
                                         .Select(step => (long?)step.StepId)
                                         .FirstAsync();

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
                         }).Where(s => s.FormTypeId == formTypeId && s.Ym == ym)
                         .ExecuteCommandAsync();
            }

            var formNo = $"{prefix}-{ym}{nextNo:D4}";

            await _db.Insertable(new FormInstanceEntity
            {
                FormId = formId,
                FormTypeId = formTypeId,
                FormNo = formNo,
                FormStatus = FormStatus.PendingSubmit.ToEnumString(),
                ApplicantUserId = _loginuser.UserId,
                ApplicantDate = DateOnly.FromDateTime(now),
                RuleId = null,
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

            return formId.ToString();
        }

        /// <summary>
        /// 匹配工作流规则
        /// </summary>
        public async Task<long?> MatchWorkflowRule(long formTypeId, long formId, long applicantUserId)
        {
            var appPositionId = await _db.Queryable<UserInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(user => user.UserId == applicantUserId)
                                         .Select(user => user.PositionId)
                                         .FirstAsync();

            // 申请日期需在规则生效区间内（EffectiveEndTime 为 null 表示无期限）
            var applicantDate = await _db.Queryable<FormInstanceEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(instance => instance.FormId == formId)
                                         .Select(instance => instance.ApplicantDate)
                                         .FirstAsync();

            var applyDate = applicantDate.ToDateTime(TimeOnly.MinValue);
            var applyDateEnd = applyDate.AddDays(1);

            var ruleList = await _db.Queryable<WorkflowRuleEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(rule => rule.FormTypeId == formTypeId
                                                && rule.EffectiveStartTime < applyDateEnd
                                                && (rule.EffectiveEndTime == null || rule.EffectiveEndTime >= applyDate))
                                    .ToListAsync();

            // 没有匹配到规则时保持 null
            long? ruleId = null;

            foreach (var rule in ruleList)
            {
                string? guidance = rule.Guidance;

                bool hasGuidance = !string.IsNullOrWhiteSpace(guidance);
                bool positionMatch = rule.PositionId == appPositionId;
                bool isDefaultRule = rule.PositionId == null && !hasGuidance;

                // 默认规则：职位和条件都不限制
                if (isDefaultRule)
                {
                    if (ruleId == null)
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
                if (ruleId == null && rule.PositionId == null && guidanceMatch)
                {
                    ruleId = rule.RuleId;
                }
            }

            await _db.Updateable<FormInstanceEntity>()
                     .SetColumns(instance => new FormInstanceEntity
                     {
                         RuleId = ruleId
                     }).Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();

            return ruleId;
        }

        /// <summary>
        /// 保存表单实例
        /// </summary>
        public async Task<int> SaveFormInstance(long formId)
        {
            var formInstance = await _db.Queryable<FormInstanceEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(instance => instance.FormId == formId)
                                        .FirstAsync();

            // 每次保存都重新匹配工作流规则
            await MatchWorkflowRule(formInstance.FormTypeId, formId, formInstance.ApplicantUserId);

            return await _db.Updateable<FormInstanceEntity>()
                            .SetColumns(f => new FormInstanceEntity
                            {
                                ModifiedBy = _loginuser.UserId,
                                ModifiedDate = DateTime.Now
                            }).Where(instance => instance.FormId == formId)
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
                                    }).ToListAsync();

            return list.Adapt<List<FormReviewRecordDto>>();
        }

        /// <summary>
        /// 查询步骤栏位权限列表
        /// </summary>
        public async Task<List<StepFieldPermissionDto>> GetStepFieldPermissionList(long formId, long loginUserId)
        {
            // 1. 该用户在「待审批」中所属的步骤
            var pendingStepIds = await _db.Queryable<PendingReviewEntity>()
                                          .Where(pending => pending.FormId == formId && pending.ReviewUserId == loginUserId)
                                          .Select(pending => pending.StepId)
                                          .ToListAsync();

            // 2. 该用户在「审批记录」中所属的步骤（原始指派人 / 实际操作人）
            var recordStepIds = await _db.Queryable<FormReviewRecordEntity>()
                                         .Where(record => record.FormId == formId && (record.OriginalUserId == loginUserId || record.OperationUserId == loginUserId))
                                         .Select(record => record.StepId)
                                         .ToListAsync();

            // 合并去重，得到该用户在此表单的所有审批步骤（待审批 StepId 可空，过滤 null 后转 long 再与记录步骤合并）
            var stepIds = pendingStepIds.Where(stepId => stepId.HasValue)
                                        .Select(stepId => stepId!.Value)
                                        .Concat(recordStepIds)
                                        .Distinct()
                                        .ToList();

            // 3. 取该表单类型下的所有栏位（FormInstance INNER JOIN FormTypeField，一次查询拿到，省一次往返）
            var fields = await _db.Queryable<FormInstanceEntity>()
                                  .InnerJoin<FormTypeFieldEntity>((formInstance, formTypeField) => formInstance.FormTypeId == formTypeField.FormTypeId)
                                  .Where((formInstance, formTypeField) => formInstance.FormId == formId)
                                  .OrderBy((formInstance, formTypeField) => formTypeField.SortOrder)
                                  .Select((formInstance, formTypeField) => formTypeField)
                                  .ToListAsync();

            // 4. 取这些步骤的栏位权限
            var permissions = await _db.Queryable<StepFieldPermissionEntity>()
                                       .Where(permission => stepIds.Contains(permission.StepId))
                                       .ToListAsync();

            // 5. 按栏位聚合「最大权限」：IsVisible / IsDisabled 都取 Max（1 表示有权限，1 > 0）
            var maxPermissionByFieldId = permissions
                                        .GroupBy(permission => permission.FieldId)
                                        .ToDictionary(
                                            group => group.Key,
                                            group => new
                                            {
                                                IsVisible = group.Max(permission => permission.IsVisible),
                                                IsDisabled = group.Max(permission => permission.IsDisabled)
                                            });

            // 6. 以表单类型的栏位为基准组装结果；无权限配置的栏位默认 0/0（不显示、不可编辑）
            var result = fields.Select(field =>
            {
                maxPermissionByFieldId.TryGetValue(field.FieldId, out var permission);
                return new StepFieldPermissionDto
                {
                    FieldKey = field.FieldKey,
                    IsVisible = permission?.IsVisible ?? 0,
                    IsDisabled = permission?.IsDisabled ?? 0
                };
            }).ToList();

            return result;
        }
    }
}