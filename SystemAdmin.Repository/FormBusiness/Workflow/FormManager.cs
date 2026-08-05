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
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Queries;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

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
        /// 查询表单前缀（表单类别上的前缀，用于分发到对应业务表单）
        /// </summary>
        public async Task<string> GetFormPrefix(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<FormTypeEntity>((instance, formtype) => instance.FormTypeId == formtype.FormTypeId)
                            .Where((instance, formtype) => instance.FormId == formId)
                            .Select((instance, formtype) => formtype.Prefix)
                            .FirstAsync();
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
        public async Task<long?> MatchWorkflowRule(long formId)
        {
            // 表单类别、申请人职级、申请日期（申请日期需在规则生效区间内）
            var formInfo = await _db.Queryable<FormInstanceEntity>()
                                    .With(SqlWith.NoLock)
                                    .InnerJoin<UserInfoEntity>((instance, user) => instance.ApplicantUserId == user.UserId)
                                    .Where((instance, user) => instance.FormId == formId)
                                    .Select((instance, user) => new
                                    {
                                        instance.FormTypeId,
                                        instance.ApplicantDate,
                                        user.PositionId
                                    }).FirstAsync();

            var formTypeId = formInfo.FormTypeId;
            var applicantDate = formInfo.ApplicantDate;
            var appPositionId = formInfo.PositionId;

            var ruleList = await _db.Queryable<WorkflowRuleEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(rule => rule.FormTypeId == formTypeId
                                                && rule.EffectiveStartDate <= applicantDate
                                                && (rule.EffectiveEndDate == null || rule.EffectiveEndDate >= applicantDate))
                                    .OrderBy(rule => rule.SortOrder)
                                    .ToListAsync();

            // 没有匹配到规则时保持 null
            long? ruleId = null;

            // 优先级1：职级Id、导向都不为 null —— 职级要匹配，导向条件也要成立
            foreach (var rule in ruleList.Where(rule => rule.PositionId != null
                                                     && !string.IsNullOrWhiteSpace(rule.Guidance)
                                                     && rule.PositionId == appPositionId))
            {
                if (await _workflowRuleConditions.Resolve(rule.Guidance!, formId))
                {
                    ruleId = rule.RuleId;
                    break;
                }
            }

            // 优先级2：职级Id 不为 null、导向为 null —— 只判断职级
            if (ruleId == null)
            {
                ruleId = ruleList.FirstOrDefault(rule => rule.PositionId != null
                                                      && string.IsNullOrWhiteSpace(rule.Guidance)
                                                      && rule.PositionId == appPositionId)?.RuleId;
            }

            // 优先级3：职级Id、导向都为 null —— 默认规则
            if (ruleId == null)
            {
                ruleId = ruleList.FirstOrDefault(rule => rule.PositionId == null
                                                      && string.IsNullOrWhiteSpace(rule.Guidance))?.RuleId;
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
            // 每次保存都重新匹配工作流规则
            await MatchWorkflowRule(formId);

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
        /// 部门树下拉
        /// </summary>
        public async Task<List<DepartmentDropDto>> GetDepartmentDrop()
        {
            return await _db.Queryable<DepartmentInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DepartmentLevelEntity>((dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                            .OrderBy(dept => dept.SortOrder)
                            .Select((dept, deptlevel) => new DepartmentDropDto
                            {
                                DepartmentId = dept.DepartmentId,
                                DepartmentName = _lang.Locale == "zh-CN"
                                                 ? dept.DepartmentNameCn
                                                 : dept.DepartmentNameEn,
                                ParentId = dept.ParentId,
                            }).ToTreeAsync(dept => dept.DepartmentChildList, dept => dept.ParentId, null);
        }

        /// <summary>
        /// 查询加审用户分页
        /// </summary>
        public async Task<ResultPaged<AddReviewUserDto>> GetAddReviewUserPage(GetAddReviewUserPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DepartmentInfoEntity>((user, dept) => user.DepartmentId == dept.DepartmentId)
                           .InnerJoin<PositionInfoEntity>((user, dept, position) => user.PositionId == position.PositionId)
                           .InnerJoin<UserLaborEntity>((user, dept, position, labor) => user.LaborId == labor.LaborId)
                           .InnerJoin<NationalityInfoEntity>((user, dept, position, labor, nation) =>
                            user.Nationality == nation.NationId)
                           .Where((user, dept, position, labor, nation) => user.IsEmployed == 1 && user.IsFreeze == 0);

            // 用户工号
            if (!string.IsNullOrEmpty(getPage.UserNo))
            {
                query = query.Where((user, dept, position, labor, nation) => user.UserNo.Contains(getPage.UserNo));
            }
            // 用户姓名
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.UserNameCn.Contains(getPage.UserName) ||
                    user.UserNameEn.Contains(getPage.UserName));
            }
            // 部门Id
            if (!string.IsNullOrEmpty(getPage.DepartmentId) && long.Parse(getPage.DepartmentId) > -1)
            {
                query = query.Where((user, dept, position, labor, nation) =>
                    user.DepartmentId == long.Parse(getPage.DepartmentId));
            }

            // 排序
            query = query.OrderBy((user, dept, position, labor, nation) => new { position.SortOrder, user.HireDate });

            var page = await query.Select((user, dept, position, labor, nation) => new AddReviewUserDto
            {
                UserId = user.UserId,
                UserNo = user.UserNo,
                UserName = _lang.Locale == "zh-CN"
                           ? user.UserNameCn
                           : user.UserNameEn,
                DepartmentName = _lang.Locale == "zh-CN"
                           ? dept.DepartmentNameCn
                           : dept.DepartmentNameEn,
                PositionName = _lang.Locale == "zh-CN"
                           ? position.PositionNameCn
                           : position.PositionNameEn,
                LaborName = _lang.Locale == "zh-CN"
                           ? labor.LaborNameCn
                           : labor.LaborNameEn,
                NationalityName = _lang.Locale == "zh-CN"
                           ? nation.NationNameCn
                           : nation.NationNameEn,
                IsAgent = user.IsAgent,
                IsReview = user.IsReview,
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);

            return ResultPaged<AddReviewUserDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 查询表单加审人列表
        /// </summary>
        public async Task<List<FormAddReviewDto>> GetAddReviewList(long formId)
        {
            var list = await _db.Queryable<FormAddReviewEntity>()
                                .With(SqlWith.NoLock)
                                .Where(addreview => addreview.FormId == formId)
                                .OrderBy(addreview => addreview.SortOrder)
                                .ToListAsync();

            return list.Adapt<List<FormAddReviewDto>>();
        }

        /// <summary>
        /// 查询该表单是否已加审过此人
        /// </summary>
        public async Task<bool> IsAddReviewExist(long formId, long userId)
        {
            return await _db.Queryable<FormAddReviewEntity>()
                            .With(SqlWith.NoLock)
                            .Where(addReview => addReview.FormId == formId && addReview.UserId == userId)
                            .AnyAsync();
        }

        /// <summary>
        /// 新增加审人
        /// </summary>
        public async Task<int> InsertAddReview(FormAddReviewEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除加审人
        /// </summary>
        public async Task<int> DeleteAddReview(long formId, long userId, int sortOrder)
        {
            return await _db.Deleteable<FormAddReviewEntity>()
                            .Where(addreview => addreview.FormId == formId
                                             && addreview.UserId == userId
                                             && addreview.SortOrder == sortOrder)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改加审人
        /// </summary>
        public async Task<int> UpdateAddReview(FormAddReviewEntity entity)
        {
            return await _db.Updateable<FormAddReviewEntity>()
                            .SetColumns(addReview => new FormAddReviewEntity
                            {
                                DeptName = entity.DeptName,
                                UserId = entity.UserId,
                                UserNo = entity.UserNo,
                                UserName = entity.UserName,
                                SortOrder = entity.SortOrder,
                                ModifiedBy = entity.ModifiedBy,
                                ModifiedDate = entity.ModifiedDate
                            }).Where(addReview => addReview.FormId == entity.FormId && addReview.SortOrder == entity.SortOrder)
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
        public async Task<List<StepFieldPermissionDto>> GetStepFieldPermissionList(long formId, long loginUserId, bool isVerification = false)
        {
            if (isVerification)
            {
                var verificationFields = await _db.Queryable<FormInstanceEntity>()
                                      .InnerJoin<FormTypeFieldEntity>((formInstance, formTypeField) => formInstance.FormTypeId == formTypeField.FormTypeId)
                                      .Where((formInstance, formTypeField) => formInstance.FormId == formId)
                                      .OrderBy((formInstance, formTypeField) => formTypeField.SortOrder)
                                      .Select((formInstance, formTypeField) => formTypeField)
                                      .ToListAsync();

                return verificationFields.Select(field => new StepFieldPermissionDto
                {
                    FieldKey = field.FieldKey,
                    IsVisible = 1,
                    IsDisabled = 1
                }).ToList();
            }

            // 1. 该用户在「待审批」中所属的步骤（含代理：当前用户是待审批人的代理人）
            var pendingStepIds = await _db.Queryable<PendingReviewEntity>()
                                          .LeftJoin<UserAgentEntity>((pending, useragent) => pending.ReviewUserId == useragent.SubstituteUserId && useragent.StartTime <= DateTime.Now && useragent.EndTime >= DateTime.Now)
                                          .Where((pending, useragent) => pending.FormId == formId && (pending.ReviewUserId == loginUserId || useragent.AgentUserId == loginUserId))
                                          .Select((pending, useragent) => pending.StepId)
                                          .ToListAsync();

            // 2. 该用户在「审批记录」中所属的步骤（原始指派人 / 实际操作人 / 原始指派人的代理人）
            var recordStepIds = await _db.Queryable<FormReviewRecordEntity>()
                                         .LeftJoin<UserAgentEntity>((record, useragent) => record.OriginalUserId == useragent.SubstituteUserId && useragent.StartTime <= DateTime.Now && useragent.EndTime >= DateTime.Now)
                                         .Where((record, useragent) => record.FormId == formId && (record.OriginalUserId == loginUserId || record.OperationUserId == loginUserId || useragent.AgentUserId == loginUserId))
                                         .Select((record, useragent) => record.StepId)
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