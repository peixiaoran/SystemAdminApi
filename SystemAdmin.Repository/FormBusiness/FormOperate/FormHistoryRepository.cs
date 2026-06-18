using SqlSugar;
using System.Data;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Dto;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Queries;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow;

namespace SystemAdmin.Repository.FormBusiness.FormOperate
{
    public class FormHistoryRepository
    {
        private readonly SqlSugarScope _db;
        private readonly WorkflowRuleConditions _workflowRuleConditions;
        private readonly Language _lang;

        public FormHistoryRepository(SqlSugarScope db, WorkflowRuleConditions workflowRuleConditions, Language lang)
        {
            _db = db;
            _workflowRuleConditions = workflowRuleConditions;
            _lang = lang;
        }

        /// <summary>
        /// 表单组别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormGroupDropDto>> GetFormGroupDrop()
        {
            return await _db.Queryable<FormGroupEntity>()
                            .With(SqlWith.NoLock)
                            .OrderBy(formgroup => formgroup.SortOrder)
                            .Select(formgroup => new FormGroupDropDto
                            {
                                FormGroupId = formgroup.FormGroupId,
                                FormGroupName = _lang.Locale == "zh-CN"
                                                ? formgroup.FormGroupNameCn
                                                : formgroup.FormGroupNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 表单类别下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormTypeDropDto>> GetFormTypeDrop(long formGroupId)
        {
            return await _db.Queryable<FormTypeEntity>()
                            .With(SqlWith.NoLock)
                            .Where(formgroup => formgroup.FormGroupId == formGroupId)
                            .OrderBy(formgroup => formgroup.SortOrder)
                            .Select(formgroup => new FormTypeDropDto
                            {
                                FormTypeId = formgroup.FormTypeId,
                                FormTypeName = _lang.Locale == "zh-CN"
                                               ? formgroup.FormTypeNameCn
                                               : formgroup.FormTypeNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 表单状态下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<FormStatusDropDto>> GetFormStatusDrop()
        {
            return await _db.Queryable<DictionaryInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(dic => dic.DicType == "FormStatus")
                            .OrderBy(dic => dic.SortOrder)
                            .Select(dic => new FormStatusDropDto
                            {
                                FormStatusCode = dic.DicCode,
                                FormStatusName = _lang.Locale == "zh-CN"
                                                 ? dic.DicNameCn
                                                 : dic.DicNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询申请记录分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetApplyHistoryPage(GetFormHistoryPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && dic.DicCode == instance.FormStatus && instance.FormStatus != FormStatus.PendingSubmit.ToEnumString() && instance.FormStatus != FormStatus.Rejected.ToEnumString())
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .LeftJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .LeftJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applyuserdept) => applyuser.DepartmentId == applyuserdept.DepartmentId)
                           .LeftJoin<UserAgentEntity>((instance, dic, formtype, applyuser, applyuserdept, useragent) => applyuser.UserId == useragent.SubstituteUserId && useragent.StartTime <= DateTime.Now && useragent.EndTime >= DateTime.Now)
                           .Where((instance, dic, formtype, applyuser, applyuserdept, useragent) => instance.ApplicantUserId == loginUserId || useragent.AgentUserId == loginUserId);

            // 表单组别Id
            if (!string.IsNullOrEmpty(getPage.FormGroupId) && long.Parse(getPage.FormGroupId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept, useragent) =>
                    formtype.FormGroupId == long.Parse(getPage.FormGroupId));
            }
            // 表单类别Id
            if (!string.IsNullOrEmpty(getPage.FormTypeId) && long.Parse(getPage.FormTypeId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept, useragent) =>
                    formtype.FormTypeId == long.Parse(getPage.FormTypeId));
            }

            // 排序
            query = query.OrderBy((instance, dic, formtype, applyuser, applyuserdept, useragent) => new { instance.CreatedDate });

            var page = await query.Select((instance, dic, formtype, applyuser, applyuserdept, useragent) => new FormHistoryDto
            {
                FormId = instance.FormId,
                FormNo = instance.FormNo,
                FormTypeId = formtype.FormTypeId,
                FormTypeName = _lang.Locale == "zh-CN"
                               ? formtype.FormTypeNameCn
                               : formtype.FormTypeNameEn,
                FormStatus = instance.FormStatus,
                FormStatusName = _lang.Locale == "zh-CN"
                               ? dic.DicNameCn
                               : dic.DicNameEn,
                ApplyUserName = _lang.Locale == "zh-CN"
                               ? applyuser.UserNameCn
                               : applyuser.UserNameEn,
                ApplyUserDeptName = _lang.Locale == "zh-CN"
                               ? applyuserdept.DepartmentNameCn
                               : applyuserdept.DepartmentNameEn,
                ApplicantDate = instance.ApplicantDate,
                ViewPath = formtype.ViewPath,
                IsWithdraw = instance.FormStatus != FormStatus.Voided.ToEnumString() ? 1 : 0
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<FormHistoryDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 查询审批记录分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<FormHistoryDto>> GetReviewHistoryPage(GetFormHistoryPage getPage, long loginUserId)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<FormInstanceEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((instance, dic) => dic.DicType == "FormStatus" && instance.FormStatus == dic.DicCode)
                           .InnerJoin<FormTypeEntity>((instance, dic, formtype) => instance.FormTypeId == formtype.FormTypeId)
                           .InnerJoin<UserInfoEntity>((instance, dic, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                           .InnerJoin<DepartmentInfoEntity>((instance, dic, formtype, applyuser, applyuserdept) => applyuser.DepartmentId == applyuserdept.DepartmentId)
                           .Where((instance, dic, formtype, applyuser, applyuserdept) =>
                               SqlFunc.Subqueryable<FormReviewRecordEntity>()
                                      .InnerJoin<WorkflowStepEntity>((record, step) => record.StepId == step.StepId)
                                      .Where((record, step) => record.FormId == instance.FormId && (record.OriginalUserId == loginUserId || record.OperationUserId == loginUserId) && step.IsStartStep != 1)
                           .Any());

            // 表单组别Id
            if (!string.IsNullOrEmpty(getPage.FormGroupId) && long.Parse(getPage.FormGroupId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept) =>
                    formtype.FormGroupId == long.Parse(getPage.FormGroupId));
            }
            // 表单类别Id
            if (!string.IsNullOrEmpty(getPage.FormTypeId) && long.Parse(getPage.FormTypeId) > 0)
            {
                query = query.Where((instance, dic, formtype, applyuser, applyuserdept) =>
                    formtype.FormTypeId == long.Parse(getPage.FormTypeId));
            }

            // 排序
            query = query.OrderBy((instance, dic, formtype, applyuser, applyuserdept) => instance.CreatedDate);

            var page = await query.Select((instance, dic, formtype, applyuser, applyuserdept) => new FormHistoryDto
            {
                FormId = instance.FormId,
                FormNo = instance.FormNo,
                FormTypeId = formtype.FormTypeId,
                FormTypeName = _lang.Locale == "zh-CN"
                               ? formtype.FormTypeNameCn
                               : formtype.FormTypeNameEn,
                FormStatus = instance.FormStatus,
                FormStatusName = _lang.Locale == "zh-CN"
                               ? dic.DicNameCn
                               : dic.DicNameEn,
                ApplyUserName = _lang.Locale == "zh-CN"
                               ? applyuser.UserNameCn
                               : applyuser.UserNameEn,
                ApplyUserDeptName = _lang.Locale == "zh-CN"
                               ? applyuserdept.DepartmentNameCn
                               : applyuserdept.DepartmentNameEn,
                ViewPath = formtype.ViewPath,
                IsWithdraw = 0
            }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<FormHistoryDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 表单撤回-待审批人还原
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        public async Task<int> WithdrawPendingSubmit(long formId, long loginUserId)
        {
            var formTypeId = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(instance => instance.FormId == formId)
                                      .Select(instance => instance.FormTypeId)
                                      .FirstAsync();

            var startStepId = await _db.Queryable<WorkflowStepEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(step => step.FormTypeId == formTypeId && step.IsStartStep == 1)
                                       .Select(step => step.StepId)
                                       .FirstAsync();

            await _db.Deleteable<PendingReviewEntity>()
                     .Where(pending => pending.FormId == formId)
                     .ExecuteCommandAsync();

            await _db.Insertable(new PendingReviewEntity
            {
                FormId = formId,
                StepId = startStepId,
                AppointmentType = AppointmentType.Actual.ToEnumString(),
                ReviewUserId = loginUserId
            }).ExecuteCommandAsync();

            return await _db.Updateable<FormReviewRecordEntity>()
                            .SetColumns(record => new FormReviewRecordEntity
                            {
                                RecordStatus = 0
                            }).Where(instance => instance.FormId == formId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询表单开始步骤信息
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<WorkflowStepEntity> GetStartStepInfo(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<WorkflowStepEntity>((instance, step) => instance.FormTypeId == step.FormTypeId)
                            .Where((instance, step) => instance.FormId == formId && step.IsStartStep == 1)
                            .Select((instance, step) => step)
                            .FirstAsync();
        }


        public async Task<int> InsertReviewRecords(long formId, long stepId, ReviewResult result, long? rejectStepId, List<UserAppointment> appointments, string comment, ReviewType reviewType, long operatorUserId)
        {
            if (!appointments.Any())
            {
                return 0;
            }
            else
            {
                var agentActual = AppointmentType.Agent.ToEnumString();
                var agentConcurrent = AppointmentType.ConcurrentAgent.ToEnumString();
                var autoAgentActual = AppointmentType.AutoAgent.ToEnumString();
                var autoAgentConc = AppointmentType.AutoConcurrentAgent.ToEnumString();

                var records = appointments.Select(appoint =>
                {
                    bool isAgentOp = appoint.AgentUserId != null && appoint.AgentUserId == operatorUserId;

                    // 代理操作：ReviewUserId 取代理人，AppointmentType 保持代理身份（Agent/ConcurrentAgent）
                    // 本人操作：ReviewUserId 取归属人，AppointmentType 换回实/兼身份（去掉 Agent 后缀）
                    string appointmentCode;
                    long reviewUserId;

                    if (isAgentOp)
                    {
                        appointmentCode = appoint.AppointmentType; // 已是代理身份
                        reviewUserId = operatorUserId;
                    }
                    else
                    {
                        // 本人操作，将 AppointmentType 中的代理身份还原为实/兼身份
                        appointmentCode = appoint.AppointmentType switch
                        {
                            var c when c == agentActual => AppointmentType.Actual.ToEnumString(),
                            var c when c == agentConcurrent => AppointmentType.Concurrent.ToEnumString(),
                            var c when c == autoAgentActual => AppointmentType.AutoActual.ToEnumString(),
                            var c when c == autoAgentConc => AppointmentType.AutoConcurrent.ToEnumString(),
                            _ => appoint.AppointmentType,
                        };
                        reviewUserId = appoint.ReviewUserId;
                    }

                    return new FormReviewRecordEntity
                    {
                        FormId = formId,
                        StepId = stepId,
                        ReviewResult = result.ToEnumString(),
                        RejectStepId = rejectStepId,
                        Comment = comment,
                        ReviewType = reviewType.ToEnumString(),
                        AppointmentType = appointmentCode,
                        OriginalUserId = appoint.ReviewUserId,
                        OperationUserId = reviewUserId,
                        ReviewDateTime = DateTime.Now,
                        RecordStatus = 1
                    };
                }).ToList();
                return await _db.Insertable(records).ExecuteCommandAsync();
            }
        }

        /// <summary>
        /// 匹配工作流规则
        /// </summary>
        public async Task<long> MatchWorkflowRule(long formId, long loginUserId)
        {
            var formTypeId = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(instance => instance.FormId == formId)
                                      .Select(instance => instance.FormTypeId)
                                      .FirstAsync();

            var appPositionId = await _db.Queryable<UserInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(user => user.UserId == loginUserId)
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
                         FormStatus = FormStatus.PendingSubmit.ToEnumString(),
                         RuleId = ruleId,
                         ModifiedBy = loginUserId,
                         ModifiedDate = DateTime.Now,
                     }).Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();

            return ruleId;
        }

        /// <summary>
        /// 表单撤回
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepId"></param>
        /// <param name="loginUserId"></param>
        /// <returns></returns>
        public Task<int> WithdrawForm(long formId, long stepId, long loginUserId)
        {
            return _db.Updateable<FormInstanceEntity>()
                      .SetColumns(instance => new FormInstanceEntity
                      {
                          FormStatus = FormStatus.PendingSubmit.ToEnumString(),
                          CurrentStepId = stepId,
                          ModifiedBy = loginUserId,
                          ModifiedDate = DateTime.Now,
                      }).Where(instance => instance.FormId == formId)
                      .ExecuteCommandAsync();
        }
    }
}
