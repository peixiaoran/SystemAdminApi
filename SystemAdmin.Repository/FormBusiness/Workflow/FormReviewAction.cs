using Microsoft.Extensions.Options;
using SqlSugar;
using System.Data;
using System.Net;
using System.Reflection;
using SystemAdmin.Common.EmailTemplates;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Upsert;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Dto;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;
using SystemAdmin.Repository.FormBusiness.Workflow.PublickWorkflow;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    public class FormReviewAction
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly MailKitEmailSender _email;
        private readonly AppUrlOptions _formNotice;
        private readonly LocalizationService _localization;
        private readonly Language _lang;
        private readonly WorkflowCustomResolver _personResolver;
        private readonly WorkflowStepCompletion _stepcomletion;
        private readonly string _this = "FormBusiness.Workflow";

        public FormReviewAction(CurrentUser loginuser, SqlSugarScope db, MailKitEmailSender email, IOptions<AppUrlOptions> formNotice, LocalizationService localization, Language lang, WorkflowCustomResolver personResolver, WorkflowStepCompletion stepcomletion)
        {
            _loginuser = loginuser;
            _db = db;
            _email = email;
            _formNotice = formNotice.Value;
            _localization = localization;
            _lang = lang;
            _personResolver = personResolver;
            _stepcomletion = stepcomletion;
        }

        #region 表单核准

        /// <summary>
        /// 表单核准
        /// </summary>
        public async Task<bool> FromApprove(ApproveForm approveForm)
        {
            long formId = long.Parse(approveForm.FormId);
            var (stepInfo, ruleId) = await GetCurrentStepInfo(formId);

            // 手动核准
            bool hasPendingUser = await ProcessStepApproval(formId, stepInfo, ReviewType.Manual, approveForm.Comment);

            // 还有剩余待审批人
            if (hasPendingUser)
            {
                return true;
            }
            else
            {
                // 核准完成执行 Guidance
                await ExecuteStepGuidance(formId);

                var nextStep = await GetNextStep(ruleId, stepInfo.StepId);

                if (nextStep.NextStepId == 0)
                {
                    // 没有下一步骤，核准完成邮件通知申请人
                    await ApproveForm(formId);
                    await NotifyApplicantApproved(formId);
                    return true;
                }
                else
                {
                    // 推进步骤
                    await AdvanceCurrentStep(formId, nextStep.NextStepId);

                    // 自动核准
                    bool needNotify = await AutoApproveIfSelfInNextSteps(formId);

                    if (needNotify)
                    {
                        // 获取当前步骤信息，通知剩余待审批人
                        var (currentStepInfo, _) = await GetCurrentStepInfo(formId);
                        await NotifyPendingReviewers(formId, currentStepInfo.StepId, ReviewResult.Approve);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 手动核准
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepInfo"></param>
        /// <param name="reviewType"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        private async Task<bool> ProcessStepApproval(long formId, WorkflowStepEntity stepInfo, ReviewType reviewType, string comment)
        {
            string reviewMode = stepInfo.ReviewMode;
            var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

            // 取当前登录用户在待审批人表中对应的归属人 UserId（登录用户可能是代理人）
            long selfOriginalUserId = await _db.Queryable<PendingReviewEntity>()
                                               .With(SqlWith.NoLock)
                                               .LeftJoin<UserAgentEntity>((pending, agent) => pending.ReviewUserId == agent.SubstituteUserId && agent.StartTime <= DateTime.Now && agent.EndTime >= DateTime.Now)
                                               .Where((pending, agent) => pending.FormId == formId && pending.StepId == stepInfo.StepId && (pending.ReviewUserId == _loginuser.UserId || agent.AgentUserId == _loginuser.UserId))
                                               .Select((pending, agent) => pending.ReviewUserId)
                                               .FirstAsync();

            if (reviewMode == ReviewMode.Review.ToEnumString())
            {
                await _db.Deleteable<PendingReviewEntity>()
                         .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                         .ExecuteCommandAsync();

                await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, comment, reviewType, _loginuser.UserId);
            }
            else if (reviewMode == ReviewMode.OrReview.ToEnumString())
            {
                // 排除自己，其余人仍需记录
                var otherPendingUserIds = await _db.Queryable<PendingReviewEntity>()
                                                   .With(SqlWith.NoLock)
                                                   .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId && pending.ReviewUserId != selfOriginalUserId)
                                                   .Select(pending => pending.ReviewUserId)
                                                   .ToListAsync();

                await _db.Deleteable<PendingReviewEntity>()
                         .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                         .ExecuteCommandAsync();

                await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, comment, reviewType, _loginuser.UserId);

                foreach (long otherUserId in otherPendingUserIds)
                {
                    // 其他人的自动操作记录：操作人视为归属人本人
                    var otherAppointments = await GetStepReviewUser(formId, stepInfo, otherUserId);
                    await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, otherAppointments, string.Empty, ReviewType.Automatic, otherUserId);
                }
            }
            else if (reviewMode == ReviewMode.AndReview.ToEnumString())
            {
                // 会审：按归属人 UserId 删除自己的待审批记录
                await _db.Deleteable<PendingReviewEntity>()
                         .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId && pending.ReviewUserId == selfOriginalUserId)
                         .ExecuteCommandAsync();

                await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, comment, reviewType, _loginuser.UserId);
            }

            await _db.Updateable<FormInstanceEntity>()
                     .SetColumns(instance => instance.FormStatus == FormStatus.UnderReview.ToEnumString())
                     .Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();

            return await _db.Queryable<PendingReviewEntity>()
                            .With(SqlWith.NoLock)
                            .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                            .AnyAsync();
        }

        /// <summary>
        /// 自动核准
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        private async Task<bool> AutoApproveIfSelfInNextSteps(long formId)
        {
            while (true)
            {
                var (stepInfo, ruleId) = await GetCurrentStepInfo(formId);

                // 每轮循环进入时先初始化当前步骤的待审批人
                await EnsurePendingReviewExists(formId, stepInfo.StepId);

                // 跳过条件
                if (await ShouldSkipStep(formId, stepInfo))
                {
                    await _db.Deleteable<PendingReviewEntity>()
                             .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                             .ExecuteCommandAsync();

                    var skippedNext = await GetNextStep(ruleId, stepInfo.StepId);

                    if (skippedNext.NextStepId == 0)
                    {
                        // 没有下一步骤，核准完成邮件通知申请人
                        await ApproveForm(formId);
                        await NotifyApplicantApproved(formId);
                        return false;
                    }

                    await AdvanceCurrentStep(formId, skippedNext.NextStepId);
                    continue;
                }

                // 当前步骤待审批人中是否包含自己（含代理情况）
                var selfPendingUserId = await _db.Queryable<PendingReviewEntity>()
                                                 .With(SqlWith.NoLock)
                                                 .LeftJoin<UserAgentEntity>((pending, agent) => pending.ReviewUserId == agent.SubstituteUserId && agent.StartTime <= DateTime.Now && agent.EndTime >= DateTime.Now)
                                                 .Where((pending, agent) => pending.FormId == formId && pending.StepId == stepInfo.StepId && (pending.ReviewUserId == _loginuser.UserId || agent.AgentUserId == _loginuser.UserId))
                                                 .Select((pending, agent) => pending.ReviewUserId)
                                                 .FirstAsync();

                // 不包含自己，停止自动推进
                if (selfPendingUserId == 0)
                {
                    return true; // 需要通知当前步骤的待签人
                }
                else
                {
                    string reviewMode = stepInfo.ReviewMode;
                    if (reviewMode == ReviewMode.Review.ToEnumString())
                    {
                        var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                        await _db.Deleteable<PendingReviewEntity>()
                                 .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                                 .ExecuteCommandAsync();

                        await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, string.Empty, ReviewType.Automatic, _loginuser.UserId);
                    }
                    else if (reviewMode == ReviewMode.OrReview.ToEnumString())
                    {
                        var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                        var otherPendingUserIds = await _db.Queryable<PendingReviewEntity>()
                                                           .With(SqlWith.NoLock)
                                                           .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId && pending.ReviewUserId != selfPendingUserId)
                                                           .Select(pending => pending.ReviewUserId)
                                                           .ToListAsync();

                        await _db.Deleteable<PendingReviewEntity>()
                                 .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                                 .ExecuteCommandAsync();

                        await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, string.Empty, ReviewType.Automatic, _loginuser.UserId);

                        foreach (long otherUserId in otherPendingUserIds)
                        {
                            var otherAppointments = await GetStepReviewUser(formId, stepInfo, otherUserId);
                            await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, otherAppointments, string.Empty, ReviewType.Automatic, otherUserId);
                        }
                    }
                    else if (reviewMode == ReviewMode.AndReview.ToEnumString())
                    {
                        var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                        await _db.Deleteable<PendingReviewEntity>()
                                 .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId && pending.ReviewUserId == selfPendingUserId)
                                 .ExecuteCommandAsync();

                        await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, string.Empty, ReviewType.Automatic, _loginuser.UserId);

                        // 会审还有其他人未签，停止自动推进
                        bool othersPending = await _db.Queryable<PendingReviewEntity>()
                                                      .With(SqlWith.NoLock)
                                                      .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                                                      .AnyAsync();
                        if (othersPending)
                        {
                            return true; // 需要通知剩余待签人
                        }
                    }
                }

                // 核准完成执行 Guidance
                await ExecuteStepGuidance(formId);

                // 核准完成，推进到下一步骤
                var nextStep = await GetNextStep(ruleId, stepInfo.StepId);

                if (nextStep.NextStepId == 0)
                {
                    await ApproveForm(formId);
                    await NotifyApplicantApproved(formId);
                    return false;
                }

                await AdvanceCurrentStep(formId, nextStep.NextStepId);
            }
        }

        #endregion

        #region 初始化指定步骤的待审批人

        /// <summary>
        /// 初始化指定步骤的待审批人
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepId"></param>
        /// <returns></returns>
        private async Task EnsurePendingReviewExists(long formId, long stepId)
        {
            bool hasPending = await _db.Queryable<PendingReviewEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(pending => pending.FormId == formId && pending.StepId == stepId)
                                       .AnyAsync();
            if (hasPending)
            {
                return;
            }
            else
            {
                var stepInfo = await _db.Queryable<WorkflowStepEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(step => step.StepId == stepId)
                                        .FirstAsync();

                var reviewUser = await GetActualConStepReviewUser(formId, stepInfo);
                // 始终存实/兼归属人
                var pendingRecords = reviewUser
                                    .Select(review => new PendingReviewEntity
                                    {
                                        FormId = formId,
                                        StepId = stepId,
                                        ReviewUserId = review.ReviewUserId,
                                        AppointmentType = review.AppointmentType,
                                    }).ToList();
                await _db.Insertable(pendingRecords).ExecuteCommandAsync();
            }
        }

        #endregion

        #region 查询步骤全部审批人身份

        /// <summary>
        /// 查询步骤全部审批人身份
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepInfo"></param>
        /// <param name="reviewUserId"></param>
        /// <returns></returns>
        private async Task<List<UserAppointment>> GetStepReviewUser(long formId, WorkflowStepEntity stepInfo, long? reviewUserId = null)
        {
            var formDetail = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .InnerJoin<UserInfoEntity>((instance, user) => instance.ApplicantUserId == user.UserId)
                                      .InnerJoin<DepartmentInfoEntity>((instance, user, dept) => user.DepartmentId == dept.DepartmentId)
                                      .InnerJoin<DepartmentLevelEntity>((instance, user, dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                                      .InnerJoin<PositionInfoEntity>((instance, user, dept, deptlevel, position) => user.PositionId == position.PositionId)
                                      .Where((instance, user, dept, deptlevel, position) => instance.FormId == formId)
                                      .Select((instance, user, dept, deptlevel, position) => new ApplyFormDetail
                                      {
                                          FormId = instance.FormId,
                                          RuleId = instance.RuleId,
                                          UserId = user.UserId,
                                          DeptId = dept.DepartmentId,
                                          DeptLevelSort = deptlevel.SortOrder,
                                          PositionSort = position.SortOrder,
                                      }).FirstAsync();

            var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                     .With(SqlWith.NoLock)
                                     .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

            List<UserAppointment> result;

            if (stepInfo.IsStartStep == 1)
            {
                result = await GetStartReviewUser(formDetail.UserId);
            }
            else if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                var orgInfo = await _db.Queryable<WorkflowStepOrgEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(steporg => steporg.StepId == stepInfo.StepId)
                                       .FirstAsync();
                result = await GetOrgReviewUser(applyDept, orgInfo.DeptLeaveId, orgInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                var deptUserInfo = await _db.Queryable<WorkflowStepDeptUserEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(stepdeptuser => stepdeptuser.StepId == stepInfo.StepId)
                                            .FirstAsync();
                result = await GetDeptUserReviewUser(deptUserInfo.DepartmentId, deptUserInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                var userInfo = await _db.Queryable<WorkflowStepUserEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(stepuser => stepuser.StepId == stepInfo.StepId)
                                        .FirstAsync();
                result = await GetUserReviewUser(userInfo.UserId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                var customInfo = await _db.Queryable<WorkflowStepCustomEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(steporg => steporg.StepId == stepInfo.StepId)
                                          .FirstAsync();
                result = await GetCustomReviewUser(formDetail.FormId, customInfo.Guidance, stepInfo.ReviewMode);
            }
            else
            {
                return new List<UserAppointment>();
            }

            // 传入 reviewUserId 时，只返回匹配该人的记录
            if (reviewUserId.HasValue)
                result = result.Where(result => result.ReviewUserId == reviewUserId.Value || result.AgentUserId == reviewUserId.Value).ToList();

            return result;
        }

        #endregion

        #region 查询各指派类型审批人身份

        /// <summary>
        /// 查询起始步骤审批人身份
        /// </summary>
        private async Task<List<UserAppointment>> GetStartReviewUser(long applicantUserId)
        {
            var (actual, agent, _, _, _, _, _, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            #region SQL

            string sql = $@"SELECT TOP 1
                                t.ReviewUserId,
                                t.AgentUserId,
                                t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @Agent ELSE @Actual END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                LEFT JOIN Basic.UserAgent agent     ON [user].UserId      = agent.SubstituteUserId
                                                                   AND agent.StartTime  <= @Now
                                                                   AND agent.EndTime    >= @Now
                                LEFT JOIN Basic.UserInfo agentuser ON agent.AgentUserId = agentuser.UserId
                                WHERE [user].UserId = @ApplicantUserId
                            ) t";

            var result = await _db.Ado.SqlQueryAsync<UserAppointment>(sql, new[]
            {
                new SugarParameter("@Now", now),
                new SugarParameter("@Actual", actual),
                new SugarParameter("@Agent", agent),
                new SugarParameter("@ApplicantUserId", applicantUserId),
            });

            #endregion

            return result ?? new List<UserAppointment>();
        }

        /// <summary>
        /// 查询审批人身份 - 按组织架构
        /// </summary>
        public async Task<List<UserAppointment>> GetOrgReviewUser(List<DepartmentInfoEntity> applyParentDept, long deptLeaveId, long positionId, string reviewMode)
        {
            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == deptLeaveId)
                                     .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionId == positionId)
                                    .FirstAsync();

            string parentDeptIds = string.Join(",", applyParentDept.Select(dept => dept.DepartmentId));
            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, agent, concurrent, concurrentAgent, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
            string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AgentUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @Agent ELSE @Actual END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                              AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                    WHERE dept.DepartmentId IN ({parentDeptIds})
                      AND deptlevel.SortOrder = @DeptLevelSort AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent ELSE @Concurrent END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                              AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                    WHERE dept.DepartmentId IN ({parentDeptIds})
                      AND deptlevel.SortOrder = @DeptLevelSort AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@DeptLevelSort", deptlevel.SortOrder),
                    new SugarParameter("@PositionSort", position.SortOrder),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Agent", agent),
                    new SugarParameter("@Concurrent", concurrent),
                    new SugarParameter("@ConcurrentAgent",concurrentAgent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AgentUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent ELSE @AutoActual END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                                          AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent ELSE @AutoConcurrent END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                                          AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoAgent", autoAgent),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                                new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询审批人身份 - 按指定部门职级
        /// </summary>
        public async Task<List<UserAppointment>> GetDeptUserReviewUser(long departmentId, long positionId, string reviewMode)
        {
            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionId == positionId)
                                    .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.DepartmentId == departmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == dept.DepartmentLevelId)
                                     .FirstAsync();

            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, agent, concurrent, concurrentAgent, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
            string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AgentUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @Agent ELSE @Actual END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                              AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                    WHERE dept.DepartmentId = @DepartmentId AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent ELSE @Concurrent END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                              AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                    WHERE dept.DepartmentId = @DepartmentId AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@DepartmentId", departmentId),
                    new SugarParameter("@PositionSort", position.SortOrder),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Agent", agent),
                    new SugarParameter("@Concurrent", concurrent),
                    new SugarParameter("@ConcurrentAgent", concurrentAgent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AgentUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent ELSE @AutoActual END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                                          AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent ELSE @AutoConcurrent END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                                          AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                                WHERE dept.DepartmentId = IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoAgent", autoAgent),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                                new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询审批人身份 - 按指定人
        /// </summary>
        public async Task<List<UserAppointment>> GetUserReviewUser(long userId, string reviewMode)
        {
            var user = await _db.Queryable<UserInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(user => user.UserId == userId)
                                .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionId == user.PositionId)
                                    .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.DepartmentId == user.DepartmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == dept.DepartmentLevelId)
                                     .FirstAsync();

            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, agent, concurrent, concurrentAgent, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(true, isAuto: false);
            string autoOrderBy = BuildOrderBy(true, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AgentUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @Agent ELSE @Actual END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    LEFT JOIN Basic.UserAgent agent     ON [user].UserId       = agent.SubstituteUserId
                                                       AND agent.StartTime  <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN Basic.UserInfo agentuser ON agent.AgentUserId  = agentuser.UserId
                    WHERE [user].UserId = @UserId
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent ELSE @Concurrent END AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo  [user]     ON partime.UserId    = [user].UserId
                    LEFT JOIN  Basic.UserAgent agent     ON [user].UserId      = agent.SubstituteUserId
                                                       AND agent.StartTime  <= @Now AND agent.EndTime >= @Now
                    LEFT JOIN  Basic.UserInfo agentuser ON agent.AgentUserId = agentuser.UserId
                    WHERE partime.UserId = @UserId
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@UserId", userId),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Agent", agent),
                    new SugarParameter("@Concurrent", concurrent),
                    new SugarParameter("@ConcurrentAgent", concurrentAgent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AgentUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent ELSE @AutoActual END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                                          AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                    CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent ELSE @AutoConcurrent END AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                                          AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                                LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                                WHERE dept.DepartmentId = IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoAgent", autoAgent),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                                new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询审批人身份 - 按自定义
        /// </summary>
        public async Task<List<UserAppointment>> GetCustomReviewUser(long formId, string guidance, string reviewMode)
        {
            var custom = await _personResolver.Resolve(guidance, formId);

            if (custom == null)
            {
                return new List<UserAppointment>();
            }
            else
            {
                long userId = custom.UserId;
                var user = await _db.Queryable<UserInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(user => user.UserId == userId)
                                    .FirstAsync();

                var position = await _db.Queryable<PositionInfoEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(position => position.PositionId == custom.PositionId)
                                        .FirstAsync();

                var dept = await _db.Queryable<DepartmentInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(dept => dept.DepartmentId == custom.DepartmentId)
                                    .FirstAsync();

                var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(deptlevel => deptlevel.DepartmentLevelId == custom.DepartmentLevelId)
                                         .FirstAsync();

                bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
                string topN = isSingle ? "TOP 1" : "";

                var (actual, agent, concurrent, concurrentAgent, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = AppointmentEnumStrings();
                var now = DateTime.Now;

                string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
                string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

                #region SQL

                var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                    SELECT {topN}
                        t.ReviewUserId, t.AgentUserId, t.AppointmentType
                    FROM (
                        SELECT
                            [user].UserId AS ReviewUserId,
                            ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                            CASE WHEN agent.AgentUserId IS NOT NULL THEN @Agent ELSE @Actual END AS AppointmentType,
                            [user].HireDate
                        FROM Basic.UserInfo [user]
                        LEFT JOIN Basic.UserAgent agent     ON [user].UserId       = agent.SubstituteUserId
                                                           AND agent.StartTime  <= @Now AND agent.EndTime >= @Now
                        LEFT JOIN Basic.UserInfo agentuser ON agent.AgentUserId  = agentuser.UserId
                        WHERE [user].UserId = @UserId
                          AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                        UNION ALL

                        SELECT
                            [user].UserId AS ReviewUserId,
                            ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                            CASE WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent ELSE @Concurrent END AS AppointmentType,
                            [user].HireDate
                        FROM Basic.UserPartTime partime
                        INNER JOIN Basic.UserInfo  [user]     ON partime.UserId    = [user].UserId
                        LEFT JOIN  Basic.UserAgent agent     ON [user].UserId      = agent.SubstituteUserId
                                                           AND agent.StartTime  <= @Now AND agent.EndTime >= @Now
                        LEFT JOIN  Basic.UserInfo agentuser ON agent.AgentUserId = agentuser.UserId
                        WHERE partime.UserId = @UserId
                          AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                    ) t
                    {exactOrderBy}",
                    new[]
                    {
                        new SugarParameter("@Now", now),
                        new SugarParameter("@UserId", userId),
                        new SugarParameter("@Actual", actual),
                        new SugarParameter("@Agent", agent),
                        new SugarParameter("@Concurrent", concurrent),
                        new SugarParameter("@ConcurrentAgent", concurrentAgent),
                    });

                #endregion

                if (exactResult.Any())
                {
                    return exactResult;
                }
                else
                {
                    int currentPositionSort = position.SortOrder - 1;
                    int currentDeptLevelSort = deptlevel.SortOrder;

                    var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                             .With(SqlWith.NoLock)
                                             .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                    var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                    while (currentPositionSort >= 1)
                    {
                        while (currentDeptLevelSort >= 1)
                        {
                            #region SQL

                            var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                                SELECT {topN}
                                    t.ReviewUserId, t.AgentUserId, t.AppointmentType
                                FROM (
                                    SELECT
                                        [user].UserId AS ReviewUserId,
                                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent ELSE @AutoActual END AS AppointmentType,
                                        [user].HireDate
                                    FROM Basic.UserInfo [user]
                                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId           = agent.SubstituteUserId
                                                                              AND agent.StartTime       <= @Now AND agent.EndTime >= @Now
                                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId      = agentuser.UserId
                                    WHERE dept.DepartmentId IN ({parentDeptIds})
                                      AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                    UNION ALL

                                    SELECT
                                        [user].UserId AS ReviewUserId,
                                        ISNULL(agentuser.UserId, 0)           AS AgentUserId,
                                        CASE WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent ELSE @AutoConcurrent END AS AppointmentType,
                                        [user].HireDate
                                    FROM Basic.UserPartTime partime
                                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                    LEFT JOIN  Basic.UserAgent       agent     ON [user].UserId               = agent.SubstituteUserId
                                                                              AND agent.StartTime           <= @Now AND agent.EndTime >= @Now
                                    LEFT JOIN  Basic.UserInfo agentuser       ON agent.AgentUserId          = agentuser.UserId
                                    WHERE dept.DepartmentId IN ({parentDeptIds})
                                      AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                                ) t
                                {autoOrderBy}",
                                new[]
                                {
                                    new SugarParameter("@Now", now),
                                    new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                    new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                    new SugarParameter("@AutoActual", autoActual),
                                    new SugarParameter("@AutoAgent", autoAgent),
                                    new SugarParameter("@AutoConcurrent", autoConcurrent),
                                    new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
                                });

                            #endregion

                            if (autoResult.Any())
                            {
                                return autoResult;
                            }
                            else
                            {
                                currentDeptLevelSort--;
                            }
                        }

                        currentPositionSort--;
                        currentDeptLevelSort = deptlevel.SortOrder;
                    }
                }
            }

            return new List<UserAppointment>();
        }

        #endregion

        #region 查询单步骤全部审批人身份

        /// <summary>
        /// 查询单步骤全部审批人身份
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepInfo"></param>
        /// <param name="reviewUserId"></param>
        /// <returns></returns>
        private async Task<List<UserAppointment>> GetActualConStepReviewUser(long formId, WorkflowStepEntity stepInfo, long? reviewUserId = null)
        {
            var formDetail = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .InnerJoin<UserInfoEntity>((instance, user) => instance.ApplicantUserId == user.UserId)
                                      .InnerJoin<DepartmentInfoEntity>((instance, user, dept) => user.DepartmentId == dept.DepartmentId)
                                      .InnerJoin<DepartmentLevelEntity>((instance, user, dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                                      .InnerJoin<PositionInfoEntity>((instance, user, dept, deptlevel, position) => user.PositionId == position.PositionId)
                                      .Where((instance, user, dept, deptlevel, position) => instance.FormId == formId)
                                      .Select((instance, user, dept, deptlevel, position) => new ApplyFormDetail
                                      {
                                          FormId = instance.FormId,
                                          RuleId = instance.RuleId,
                                          UserId = user.UserId,
                                          DeptId = dept.DepartmentId,
                                          DeptLevelSort = deptlevel.SortOrder,
                                          PositionSort = position.SortOrder,
                                      }).FirstAsync();

            var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                     .With(SqlWith.NoLock)
                                     .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

            List<UserAppointment> result;

            if (stepInfo.IsStartStep == 1)
            {
                result = await GetActualConStartReviewUser(formDetail.UserId);
            }
            else if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                var orgInfo = await _db.Queryable<WorkflowStepOrgEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(steporg => steporg.StepId == stepInfo.StepId)
                                       .FirstAsync();
                result = await GetActualConOrgReviewUser(applyDept, orgInfo.DeptLeaveId, orgInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                var deptUserInfo = await _db.Queryable<WorkflowStepDeptUserEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(stepdeptuser => stepdeptuser.StepId == stepInfo.StepId)
                                            .FirstAsync();
                result = await GetActualConDeptUserReviewUser(deptUserInfo.DepartmentId, deptUserInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                var userInfo = await _db.Queryable<WorkflowStepUserEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(stepuser => stepuser.StepId == stepInfo.StepId)
                                        .FirstAsync();
                result = await GetActualConUserReviewUser(userInfo.UserId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                var customInfo = await _db.Queryable<WorkflowStepCustomEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(stepcustom => stepcustom.StepId == stepInfo.StepId)
                                          .FirstAsync();
                result = await GetActualConCustomReviewUser(formDetail.FormId, customInfo.Guidance, stepInfo.ReviewMode);
            }
            else
            {
                return new List<UserAppointment>();
            }

            // 传入 reviewUserId 时，只返回匹配该人的记录
            if (reviewUserId.HasValue)
                result = result.Where(result => result.ReviewUserId == reviewUserId.Value || result.AgentUserId == reviewUserId.Value).ToList();

            return result;
        }

        #endregion

        #region 查询各指派类型审批人身份（实、兼）

        /// <summary>
        /// 查询起始步骤审批人身份
        /// </summary>
        private async Task<List<UserAppointment>> GetActualConStartReviewUser(long applicantUserId)
        {
            var (actual, _, _, _, _, _, _, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            #region SQL

            string sql = $@"SELECT TOP 1
                        t.ReviewUserId,
                        t.AppointmentType
                    FROM (
                        SELECT
                            [user].UserId AS ReviewUserId,
                            @Actual      AS AppointmentType,
                            [user].HireDate
                        FROM Basic.UserInfo [user]
                        WHERE [user].UserId = @ApplicantUserId
                    ) t";

            var result = await _db.Ado.SqlQueryAsync<UserAppointment>(sql, new[]
            {
                new SugarParameter("@Now", now),
                new SugarParameter("@Actual", actual),
                new SugarParameter("@ApplicantUserId", applicantUserId),
            });

            #endregion

            return result ?? new List<UserAppointment>();
        }

        /// <summary>
        /// 查询单步骤审批人身份 - 按组织架构
        /// </summary>
        public async Task<List<UserAppointment>> GetActualConOrgReviewUser(List<DepartmentInfoEntity> applyParentDept, long deptLeaveId, long positionId, string reviewMode)
        {
            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == deptLeaveId)
                                     .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionId == positionId)
                                    .FirstAsync();

            string parentDeptIds = string.Join(",", applyParentDept.Select(dept => dept.DepartmentId));
            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, _, concurrent, _, autoActual, _, autoConcurrent, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
            string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Actual      AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                    WHERE dept.DepartmentId IN ({parentDeptIds})
                      AND deptlevel.SortOrder = @DeptLevelSort AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Concurrent  AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                    WHERE dept.DepartmentId IN ({parentDeptIds})
                      AND deptlevel.SortOrder = @DeptLevelSort AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@DeptLevelSort", deptlevel.SortOrder),
                    new SugarParameter("@PositionSort", position.SortOrder),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Concurrent", concurrent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoActual  AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId    AS ReviewUserId,
                                    @AutoConcurrent AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询单步骤审批人身份 - 按指定部门职级
        /// </summary>
        public async Task<List<UserAppointment>> GetActualConDeptUserReviewUser(long departmentId, long positionId, string reviewMode)
        {
            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionId == positionId)
                                    .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.DepartmentId == departmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == dept.DepartmentLevelId)
                                     .FirstAsync();

            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, _, concurrent, _, autoActual, _, autoConcurrent, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
            string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Actual      AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                    WHERE dept.DepartmentId = @DepartmentId AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Concurrent  AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                    WHERE dept.DepartmentId = @DepartmentId AND position.SortOrder = @PositionSort
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@DepartmentId", departmentId),
                    new SugarParameter("@PositionSort", position.SortOrder),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Concurrent", concurrent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoActual  AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoConcurrent AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询单步骤审批人身份 - 按指定人
        /// </summary>
        public async Task<List<UserAppointment>> GetActualConUserReviewUser(long userId, string reviewMode)
        {
            var user = await _db.Queryable<UserInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(user => user.UserId == userId)
                                .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
                                   .With(SqlWith.NoLock)
                                   .Where(position => position.PositionId == user.PositionId)
                                   .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.DepartmentId == dept.DepartmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == dept.DepartmentLevelId)
                                     .FirstAsync();

            bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
            string topN = isSingle ? "TOP 1" : "";

            var (actual, _, concurrent, _, autoActual, _, autoConcurrent, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
            string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

            #region SQL

            var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                SELECT {topN}
                    t.ReviewUserId, t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Actual      AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserInfo [user]
                    WHERE [user].UserId = @UserId
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Concurrent  AS AppointmentType,
                        [user].HireDate
                    FROM Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo [user] ON partime.UserId = [user].UserId
                    WHERE partime.UserId = @UserId
                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                ) t
                {exactOrderBy}",
                new[]
                {
                    new SugarParameter("@Now", now),
                    new SugarParameter("@UserId", userId),
                    new SugarParameter("@Actual", actual),
                    new SugarParameter("@Concurrent", concurrent),
                });

            #endregion

            if (exactResult.Any())
            {
                return exactResult;
            }
            else
            {
                int currentPositionSort = position.SortOrder - 1;
                int currentDeptLevelSort = deptlevel.SortOrder;

                var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                         .With(SqlWith.NoLock)
                                         .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId, t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoActual  AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId    AS ReviewUserId,
                                    @AutoConcurrent AS AppointmentType,
                                    [user].HireDate
                                FROM Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                WHERE dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                            ) t
                            {autoOrderBy}",
                            new[]
                            {
                                new SugarParameter("@Now", now),
                                new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                new SugarParameter("@AutoActual", autoActual),
                                new SugarParameter("@AutoConcurrent", autoConcurrent),
                            });

                        #endregion

                        if (autoResult.Any())
                        {
                            return autoResult;
                        }
                        else
                        {
                            currentDeptLevelSort--;
                        }
                    }

                    currentPositionSort--;
                    currentDeptLevelSort = deptlevel.SortOrder;
                }
            }

            return new List<UserAppointment>();
        }

        /// <summary>
        /// 查询单步骤审批人身份 - 按自定义
        /// </summary>
        public async Task<List<UserAppointment>> GetActualConCustomReviewUser(long formId, string guidance, string reviewMode)
        {
            var custom = await _personResolver.Resolve(guidance, formId);
            if (custom == null)
            {
                return new List<UserAppointment>();
            }
            else
            {
                long userId = custom.UserId;
                var user = await _db.Queryable<UserInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(user => user.UserId == userId)
                                    .FirstAsync();

                var position = await _db.Queryable<PositionInfoEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(position => position.PositionId == user.PositionId)
                                       .FirstAsync();

                var dept = await _db.Queryable<DepartmentInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(dept => dept.DepartmentId == dept.DepartmentId)
                                    .FirstAsync();

                var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(deptlevel => deptlevel.DepartmentLevelId == dept.DepartmentLevelId)
                                         .FirstAsync();

                bool isSingle = reviewMode == ReviewMode.Review.ToEnumString();
                string topN = isSingle ? "TOP 1" : "";

                var (actual, _, concurrent, _, autoActual, _, autoConcurrent, _) = AppointmentEnumStrings();
                var now = DateTime.Now;

                string exactOrderBy = BuildOrderBy(isSingle, isAuto: false);
                string autoOrderBy = BuildOrderBy(isSingle, isAuto: true);

                #region SQL

                var exactResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                    SELECT {topN}
                        t.ReviewUserId, t.AppointmentType
                    FROM (
                        SELECT
                            [user].UserId AS ReviewUserId,
                            @Actual      AS AppointmentType,
                            [user].HireDate
                        FROM Basic.UserInfo [user]
                        WHERE [user].UserId = @UserId
                          AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                        UNION ALL

                        SELECT
                            [user].UserId AS ReviewUserId,
                            @Concurrent  AS AppointmentType,
                            [user].HireDate
                        FROM Basic.UserPartTime partime
                        INNER JOIN Basic.UserInfo [user] ON partime.UserId = [user].UserId
                        WHERE partime.UserId = @UserId
                          AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                    ) t
                    {exactOrderBy}",
                    new[]
                    {
                        new SugarParameter("@Now", now),
                        new SugarParameter("@UserId", userId),
                        new SugarParameter("@Actual", actual),
                        new SugarParameter("@Concurrent", concurrent),
                    });

                #endregion

                if (exactResult.Any())
                {
                    return exactResult;
                }
                else
                {
                    int currentPositionSort = position.SortOrder - 1;
                    int currentDeptLevelSort = deptlevel.SortOrder;

                    var applyDept = await _db.Queryable<DepartmentInfoEntity>()
                                             .With(SqlWith.NoLock)
                                             .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                    var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                    while (currentPositionSort >= 1)
                    {
                        while (currentDeptLevelSort >= 1)
                        {
                            #region SQL

                            var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                                SELECT {topN}
                                    t.ReviewUserId, t.AppointmentType
                                FROM (
                                    SELECT
                                        [user].UserId AS ReviewUserId,
                                        @AutoActual  AS AppointmentType,
                                        [user].HireDate
                                    FROM Basic.UserInfo [user]
                                    INNER JOIN Basic.DepartmentInfo  dept      ON [user].DepartmentId     = dept.DepartmentId
                                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                    INNER JOIN Basic.PositionInfo    position  ON [user].PositionId       = position.PositionId
                                    WHERE dept.DepartmentId IN ({parentDeptIds})
                                      AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0

                                    UNION ALL

                                    SELECT
                                        [user].UserId AS ReviewUserId,
                                        @AutoConcurrent AS AppointmentType,
                                        [user].HireDate
                                    FROM Basic.UserPartTime partime
                                    INNER JOIN Basic.UserInfo        [user]     ON partime.UserId             = [user].UserId
                                    INNER JOIN Basic.DepartmentInfo  dept      ON partime.PartTimeDeptId     = dept.DepartmentId
                                    INNER JOIN Basic.DepartmentLevel deptlevel ON dept.DepartmentLevelId     = deptlevel.DepartmentLevelId
                                    INNER JOIN Basic.PositionInfo    position  ON partime.PartTimePositionId = position.PositionId
                                    WHERE dept.DepartmentId IN ({parentDeptIds})
                                      AND position.SortOrder = @CurrentPositionSort AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                      AND [user].IsReview = 1 AND [user].IsEmployed = 1 AND [user].IsFreeze = 0
                                ) t
                                {autoOrderBy}",
                                new[]
                                {
                                    new SugarParameter("@Now", now),
                                    new SugarParameter("@CurrentPositionSort", currentPositionSort),
                                    new SugarParameter("@CurrentDeptLevelSort", currentDeptLevelSort),
                                    new SugarParameter("@AutoActual", autoActual),
                                    new SugarParameter("@AutoConcurrent", autoConcurrent),
                                });

                            #endregion

                            if (autoResult.Any())
                            {
                                return autoResult;
                            }
                            else
                            {
                                currentDeptLevelSort--;
                            }
                        }

                        currentPositionSort--;
                        currentDeptLevelSort = deptlevel.SortOrder;
                    }
                }
            }

            return new List<UserAppointment>();
        }

        #endregion

        #region 步骤跳过 & 推进 & 状态更新

        /// <summary>
        /// 检查当前步骤是否应跳过
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepInfo"></param>
        /// <returns></returns>
        private async Task<bool> ShouldSkipStep(long formId, WorkflowStepEntity stepInfo)
        {
            // 自定义审批人
            if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                var customInfo = await _db.Queryable<WorkflowStepCustomEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(custom => custom.StepId == stepInfo.StepId)
                                          .FirstAsync();

                var reviewUsers = await GetCustomReviewUser(formId, customInfo.Guidance, stepInfo.ReviewMode);

                // 找不到审批人，直接跳过
                return !reviewUsers.Any();
            }

            // 原有组织架构跳过逻辑
            if (stepInfo.Assignment != Assignment.Org.ToEnumString())
            {
                return false;
            }
            else
            {
                var formDetail = await _db.Queryable<FormInstanceEntity>()
                                          .With(SqlWith.NoLock)
                                          .InnerJoin<UserInfoEntity>((instance, user) => instance.ApplicantUserId == user.UserId)
                                          .InnerJoin<DepartmentInfoEntity>((instance, user, dept) => user.DepartmentId == dept.DepartmentId)
                                          .InnerJoin<DepartmentLevelEntity>((instance, user, dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                                          .InnerJoin<PositionInfoEntity>((instance, user, dept, deptlevel, position) => user.PositionId == position.PositionId)
                                          .Where((instance, user, dept, deptlevel, position) => instance.FormId == formId)
                                          .Select((instance, user, dept, deptlevel, position) => new
                                          {
                                              DeptLevelSort = deptlevel.SortOrder,
                                              PositionSort = position.SortOrder,
                                          }).FirstAsync();

                var orgInfo = await _db.Queryable<WorkflowStepOrgEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(steporg => steporg.StepId == stepInfo.StepId)
                                       .FirstAsync();

                var configDeptLevel = await _db.Queryable<DepartmentLevelEntity>()
                                               .With(SqlWith.NoLock)
                                               .Where(deptlevel => deptlevel.DepartmentLevelId == orgInfo.DeptLeaveId)
                                               .FirstAsync();

                var configPosition = await _db.Queryable<PositionInfoEntity>()
                                              .With(SqlWith.NoLock)
                                              .Where(postion => postion.PositionId == orgInfo.PositionId)
                                              .FirstAsync();

                return formDetail.DeptLevelSort < configDeptLevel.SortOrder || formDetail.PositionSort <= configPosition.SortOrder;
            }
        }

        /// <summary>
        /// 取当前步骤对应的流程规则
        /// </summary>
        /// <param name="ruleId"></param>
        /// <param name="currentStepId"></param>
        /// <returns></returns>
        private async Task<WorkflowRuleStepEntity> GetNextStep(long ruleId, long currentStepId)
        {
            return await _db.Queryable<WorkflowRuleStepEntity>()
                            .With(SqlWith.NoLock)
                            .Where(rulestep => rulestep.RuleId == ruleId && rulestep.CurrentStepId == currentStepId)
                            .FirstAsync();
        }

        /// <summary>
        /// 推进表单下一步骤
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="nextStepId"></param>
        /// <returns></returns>
        private async Task AdvanceCurrentStep(long formId, long? nextStepId)
        {
            await _db.Updateable<FormInstanceEntity>()
                     .SetColumns(instance => new FormInstanceEntity
                     {
                         CurrentStepId = nextStepId,
                     }).Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();
        }

        /// <summary>
        /// 更改表单为已核准
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        private async Task ApproveForm(long formId)
        {
            await _db.Updateable<FormInstanceEntity>()
                     .SetColumns(instance => new FormInstanceEntity
                     {
                         FormStatus = FormStatus.Approved.ToEnumString(),
                         CurrentStepId = null,
                     }).Where(instance => instance.FormId == formId)
                     .ExecuteCommandAsync();
        }

        /// <summary>
        /// 获取表单当前步骤信息，同时返回 FormInstance 的 RuleId
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        private async Task<(WorkflowStepEntity StepInfo, long RuleId)> GetCurrentStepInfo(long formId)
        {
            var entity = await _db.Queryable<FormInstanceEntity>()
                               .With(SqlWith.NoLock)
                               .InnerJoin<WorkflowStepEntity>((instance, step) => instance.CurrentStepId == step.StepId)
                               .Where((instance, step) => instance.FormId == formId)
                               .Select((instance, step) => new
                               {
                                   RuleId = instance.RuleId,
                                   StepInfo = step,
                               }).FirstAsync();

            return (entity.StepInfo, entity.RuleId);
        }

        /// <summary>
        /// 查询步骤配置的 Guidance 并执行
        /// </summary>
        /// <param name="formId"></param>
        private async Task ExecuteStepGuidance(long formId)
        {
            // 取表单当前的 RuleId 和 CurrentStepId
            var formInfo = await _db.Queryable<FormInstanceEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(form => form.FormId == formId)
                                    .Select(form => new { form.RuleId, form.CurrentStepId })
                                    .FirstAsync();

            if (formInfo == null)
            {
                return;
            }

            // RuleId + StepId 组合查询规则步骤,找出 Guidance
            string guidance = await _db.Queryable<WorkflowRuleStepEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(ruleStep => ruleStep.RuleId == formInfo.RuleId && ruleStep.CurrentStepId == formInfo.CurrentStepId)
                                       .Select(ruleStep => ruleStep.Guidance)
                                       .FirstAsync();

            if (string.IsNullOrEmpty(guidance))
            {
                return;
            }

            await _stepcomletion.Resolve(guidance, formId);
        }

        #endregion

        #region 表单驳回

        /// <summary>
        /// 表单驳回
        /// </summary>
        /// <param name="rejectForm"></param>
        /// <returns></returns>
        public async Task<bool> FromReject(RejectForm rejectForm)
        {
            var formId = long.Parse(rejectForm.FormId);
            var rejectStepId = long.Parse(rejectForm.RejectStepId);

            // 1. 驳回步骤信息
            var RejectStep = await _db.Queryable<WorkflowStepEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(step => step.StepId == rejectStepId)
                                      .FirstAsync();
            // 2. 当前步骤信息
            var CurrentStep = await _db.Queryable<FormInstanceEntity>()
                                       .With(SqlWith.NoLock)
                                       .InnerJoin<WorkflowStepEntity>((instance, step) => instance.CurrentStepId == step.StepId)
                                       .Where(instance => instance.FormId == formId)
                                       .Select((instance, step) => step)
                                       .FirstAsync();

            // 2. 处理驳回步骤：清待审 -> 新增审批日志 -> 更新表单状态
            var selfAppointments = await GetStepReviewUser(formId, CurrentStep, _loginuser.UserId);

            // 3. 清空待审批人
            var deleteResult = await _db.Deleteable<PendingReviewEntity>()
                                        .Where(pending => pending.FormId == formId)
                                        .ExecuteCommandAsync();

            var insertResult = await InsertReviewRecords(formId, CurrentStep.StepId, ReviewResult.Reject, rejectStepId, selfAppointments, rejectForm.Comment, ReviewType.Manual, _loginuser.UserId);

            var updateResult = await _db.Updateable<FormInstanceEntity>()
                                        .SetColumns(instance => instance.FormStatus == FormStatus.Rejected.ToEnumString())
                                        .Where(instance => instance.FormId == formId)
                                        .ExecuteCommandAsync();

            if (deleteResult <= 0 || insertResult <= 0 || updateResult <= 0)
            {
                return false;
            }
            else
            {
                // 4. 推进步骤
                await AdvanceCurrentStep(formId, rejectStepId);

                // 5. 初始化当前步骤的待审批人
                await EnsurePendingReviewExists(formId, RejectStep.StepId);

                // 6. 通知剩余待审批人
                await NotifyPendingReviewers(formId, rejectStepId, ReviewResult.Reject);

                return true;
            }
        }

        #endregion

        #region 审批日志记录

        /// <summary>
        /// 审批日志记录
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepId"></param>
        /// <param name="appointments"></param>
        /// <param name="reviewType"></param>
        /// <param name="comment"></param>
        /// <param name="operatorUserId"></param>
        /// <returns></returns>
        private async Task<int> InsertReviewRecords(long formId, long stepId, ReviewResult result, long? rejectStepId, List<UserAppointment> appointments, string comment, ReviewType reviewType, long operatorUserId)
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
                    bool isAgentOp = appoint.AgentUserId != 0 && appoint.AgentUserId == operatorUserId;

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
                    };
                }).ToList();
                return await _db.Insertable(records).ExecuteCommandAsync();
            }
        }

        #endregion

        #region 邮件通知

        private async Task NotifyPendingReviewers(long formId, long stepId, ReviewResult result)
        {
            var now = DateTime.Now;

            // 1. 表单 + 当前步骤
            var formNotice = await _db.Queryable<FormInstanceEntity>()
                                      .InnerJoin<FormTypeEntity>((instance, formtype) => instance.FormTypeId == formtype.FormTypeId)
                                      .InnerJoin<UserInfoEntity>((instance, formtype, applyuser) => instance.ApplicantUserId == applyuser.UserId)
                                      .InnerJoin<FormReviewRecordEntity>((instance, formtype, applyuser, record) => instance.FormId == record.FormId && record.ReviewDateTime == SqlFunc.Subqueryable<FormReviewRecordEntity>()
                                                                           .Where(record => record.FormId == instance.FormId)
                                                                           .Max(record => record.ReviewDateTime))
                                      .InnerJoin<WorkflowStepEntity>((instance, formtype, applyuser, record, step) => instance.CurrentStepId == step.StepId)
                                      .Where((instance, formtype, applyuser, record, step) => instance.FormId == formId)
                                      .Select((instance, formtype, applyuser, record, step) => new FormNoticeReviewDto
                                      {
                                          FormId = instance.FormId,
                                          FormNo = instance.FormNo,
                                          FormTypeNameCn = formtype.FormTypeNameCn,
                                          FormTypeNameEn = formtype.FormTypeNameEn,
                                          ApplicantUserCn = applyuser.UserNameCn,
                                          ApplicantUserEn = applyuser.UserNameEn,
                                          Comment = record.Comment,
                                          CurrentStepNameCn = step.StepNameCn,
                                          CurrentStepNameEn = step.StepNameEn,
                                          ReviewPath = formtype.ReviewPath,
                                      }).FirstAsync();

            // 2. 待审批用户
            var pendingUserIds = await _db.Queryable<PendingReviewEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(pending => pending.FormId == formId && pending.StepId == stepId)
                                          .Select(pending => pending.ReviewUserId)
                                          .ToListAsync();

            if (pendingUserIds.Count == 0)
            {
                return;
            }

            // 3. 代理人替换
            var agentMap = (await _db.Queryable<UserAgentEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(useragent => pendingUserIds.Contains(useragent.SubstituteUserId) && useragent.StartTime <= now && useragent.EndTime >= now)
                                     .Select(useragent => new { useragent.SubstituteUserId, useragent.AgentUserId })
                                     .ToListAsync())
                                     .ToDictionary(useragent => useragent.SubstituteUserId, useragent => useragent.AgentUserId);

            var notifyUserIds = pendingUserIds
                                .Select(id => agentMap.TryGetValue(id, out var agentId) ? agentId : id)
                                .Distinct()
                                .ToList();

            // 4. 收件人（含 NoticeLanguage）
            var userList = await _db.Queryable<UserInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(user => notifyUserIds.Contains(user.UserId) && user.IsRealtimeNotification == 1)
                                    .ToListAsync();

            // 5. 生成 token + 批量新增
            var expirationTime = now.AddDays(15);
            var tokens = userList.ToDictionary(user => user.UserId, _ => GenerateSecureToken());

            var tokenEntities = tokens.Select(kv => new FormNotificationTokenEntity
            {
                FormId = formNotice.FormId,
                ReviewUserId = kv.Key,
                Token = kv.Value,
                ExpirationTime = expirationTime,
                CreatedDate = now,
            }).ToList();

            await _db.Insertable(tokenEntities).ExecuteCommandAsync();

            // 6. 模板
            var template = result == ReviewResult.Approve
                ? EmailTemplateLoader.GetApproveNotice()
                : EmailTemplateLoader.GetRejectNotice();

            // 7. 逐人发送（按 NoticeLanguage 渲染）
            foreach (var user in userList)
            {
                var lang = user.NoticeLanguage;

                // ===== Greeting 内联构造 =====
                var rawName = lang == "zh-CN"
                              ? user.UserNameCn
                              : user.UserNameEn;
                var name = WebUtility.HtmlEncode(rawName ?? string.Empty);

                var titleKey = user.Gender switch
                {
                    1 => $"{_this}.EmailNoticeGreetingTitleMale",
                    2 => $"{_this}.EmailNoticeGreetingTitleFemale",
                    _ => $"{_this}.EmailNoticeGreetingTitleDefault",
                };
                var title = _localization.ReturnMsg(titleKey, lang);

                var greeting = _localization.ReturnMsg($"{_this}.EmailNoticeGreetingTemplate", lang, name, title);

                // ===== Greeting 结束 =====

                // 邮件标题
                var headerTitle = _localization.ReturnMsg($"{_this}.EmailNoticePendingTitle", lang);

                // 邮件主题
                var subjectPrefix = _localization.ReturnMsg($"{_this}.EmailNoticePendingTitle", lang);

                var resultText = result == ReviewResult.Approve
                    ? _localization.ReturnMsg($"{_this}.EmailNoticeResultApprove", lang)
                    : _localization.ReturnMsg($"{_this}.EmailNoticeResultReject", lang);

                // 按语言挑选业务字段
                var formTypeName = lang == "zh-CN" ? formNotice.FormTypeNameCn : formNotice.FormTypeNameEn;
                var applicantUser = lang == "zh-CN" ? formNotice.ApplicantUserCn : formNotice.ApplicantUserEn;
                var currentStepName = lang == "zh-CN" ? formNotice.CurrentStepNameCn : formNotice.CurrentStepNameEn;

                var reviewUrl = BuildReviewUrl(_formNotice.BaseDomain, formNotice.ReviewPath, user.NoticeLanguage, tokens[user.UserId]);

                var body = template
                    .Replace("{{Title}}", WebUtility.HtmlEncode(headerTitle))
                    .Replace("{{Greeting}}", greeting)
                    .Replace("{{FormInfo}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeFormInfo", lang)))
                    .Replace("{{LabelFormNo}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelFormNo", lang)))
                    .Replace("{{LabelFormType}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelFormType", lang)))
                    .Replace("{{LabelApplicant}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelApplicant", lang)))
                    .Replace("{{LabelResult}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelResult", lang)))
                    .Replace("{{LabelComment}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelComment", lang)))
                    .Replace("{{LabelStep}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelStep", lang)))
                    .Replace("{{ResultText}}", WebUtility.HtmlEncode(resultText))
                    .Replace("{{BtnReview}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeBtnReview", lang)))
                    .Replace("{{BtnSignIn}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeBtnSignIn", lang)))
                    .Replace("{{ExpireHint}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeExpireHint", lang)))
                    .Replace("{{FooterText}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeFooter", lang)))
                    .Replace("{{FormNo}}", WebUtility.HtmlEncode(formNotice.FormNo ?? string.Empty))
                    .Replace("{{FormTypeName}}", WebUtility.HtmlEncode(formTypeName ?? string.Empty))
                    .Replace("{{ApplicantUser}}", WebUtility.HtmlEncode(applicantUser ?? string.Empty))
                    .Replace("{{Comment}}", WebUtility.HtmlEncode(formNotice.Comment ?? string.Empty))
                    .Replace("{{CurrentStepName}}", WebUtility.HtmlEncode(currentStepName ?? string.Empty))
                    .Replace("{{LoginUrl}}", _formNotice.LoginUrl)
                    .Replace("{{ReviewUrl}}", reviewUrl);

                await _email.SendAsync(new EmailMessage
                {
                    To = new List<string> { user.Email },
                    Subject = $"{subjectPrefix} {formNotice.FormNo} - {formTypeName}",
                    Body = body,
                });
            }
        }

        /// <summary>
        /// 邮件通知申请人表单核准完成
        /// </summary>
        private async Task NotifyApplicantApproved(long formId)
        {
            var now = DateTime.Now;
            var template = EmailTemplateLoader.GetApproveNotice();

            // 1. 表单 + 申请人
            var formNotice = await _db.Queryable<FormInstanceEntity>()
                                      .With(SqlWith.NoLock)
                                      .InnerJoin<FormTypeEntity>((instance, formtype) => instance.FormTypeId == formtype.FormTypeId)
                                      .InnerJoin<UserInfoEntity>((instance, formtype, user) => instance.ApplicantUserId == user.UserId)
                                      .Where((instance, formtype, userInfo) => instance.FormId == formId)
                                      .Select((instance, formtype, user) => new FormNoticeApprovedDto
                                      {
                                          FormId = instance.FormId,
                                          FormNo = instance.FormNo,
                                          FormTypeNameCn = formtype.FormTypeNameCn,
                                          FormTypeNameEn = formtype.FormTypeNameEn,
                                          ReviewPath = formtype.ReviewPath,
                                          UserId = user.UserId,
                                          UserNameCn = user.UserNameCn,
                                          UserNameEn = user.UserNameEn,
                                          Gender = user.Gender,
                                          Email = user.Email,
                                          IsRealtimeNotification = user.IsRealtimeNotification,
                                          NoticeLanguage = user.NoticeLanguage,
                                      }).FirstAsync();

            // 2. 代理人
            var agentUserId = await _db.Queryable<UserAgentEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(useragent => useragent.SubstituteUserId == formNotice.UserId && useragent.StartTime <= now && useragent.EndTime >= now)
                                       .Select(useragent => useragent.AgentUserId)
                                       .FirstAsync();

            // 3. 选定收件人
            UserInfoEntity? recipient;

            if (agentUserId > 0)
            {
                recipient = await _db.Queryable<UserInfoEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(user => user.UserId == agentUserId && user.IsRealtimeNotification == 1)
                                     .FirstAsync();

                if (recipient == null)
                {
                    return;
                }
            }
            else
            {
                if (formNotice.IsRealtimeNotification != 1 || string.IsNullOrWhiteSpace(formNotice.Email))
                {
                    return;
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

            // 4. 生成 token
            var expirationTime = now.AddDays(15);
            var token = GenerateSecureToken();

            var tokenEntity = new FormNotificationTokenEntity
            {
                FormId = formNotice.FormId,
                ReviewUserId = recipient.UserId,
                Token = token,
                ExpirationTime = expirationTime,
                CreatedDate = now,
            };

            await _db.Insertable(tokenEntity).ExecuteCommandAsync();

            // 5. 按收件人语言渲染
            var lang = recipient.NoticeLanguage;

            // ===== Greeting 内联构造 =====
            var rawName = lang == "zh-CN" ? recipient.UserNameCn : recipient.UserNameEn;
            var name = WebUtility.HtmlEncode(rawName ?? string.Empty);

            var titleKey = recipient.Gender switch
            {
                1 => $"{_this}.EmailNoticeGreetingTitleMale",
                2 => $"{_this}.EmailNoticeGreetingTitleFemale",
                _ => $"{_this}.EmailNoticeGreetingTitleDefault",
            };
            var title = _localization.ReturnMsg(titleKey, lang);

            var greeting = _localization.ReturnMsg($"{_this}.EmailNoticeGreetingTemplate", lang, name, title);

            // ===== Greeting 结束 =====

            var headerTitle = _localization.ReturnMsg($"{_this}.EmailNoticeApprovedTitle", lang);
            var subjectPrefix = _localization.ReturnMsg($"{_this}.EmailNoticeSubjectApproved", lang);
            var resultText = _localization.ReturnMsg($"{_this}.EmailNoticeResultApproved", lang);

            var formTypeName = lang == "zh-CN" ? formNotice.FormTypeNameCn : formNotice.FormTypeNameEn;
            var reviewUrl = BuildReviewUrl(_formNotice.BaseDomain, formNotice.ReviewPath, recipient.NoticeLanguage, token);

            var body = template
                .Replace("{{Title}}", WebUtility.HtmlEncode(headerTitle))
                .Replace("{{Greeting}}", greeting)
                .Replace("{{LabelStep}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelStep", lang)))
                .Replace("{{FormInfo}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeFormInfo", lang)))
                .Replace("{{LabelFormNo}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelFormNo", lang)))
                .Replace("{{LabelFormType}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelFormType", lang)))
                .Replace("{{LabelApplicant}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelApplicant", lang)))
                .Replace("{{LabelResult}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelResult", lang)))
                .Replace("{{LabelComment}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeLabelComment", lang)))
                .Replace("{{ResultText}}", WebUtility.HtmlEncode(resultText))
                .Replace("{{BtnReview}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeBtnReview", lang)))
                .Replace("{{BtnSignIn}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeBtnSignIn", lang)))
                .Replace("{{ExpireHint}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeExpireHint", lang)))
                .Replace("{{FooterText}}", WebUtility.HtmlEncode(_localization.ReturnMsg($"{_this}.EmailNoticeFooter", lang)))
                .Replace("{{FormNo}}", WebUtility.HtmlEncode(formNotice.FormNo ?? string.Empty))
                .Replace("{{FormTypeName}}", WebUtility.HtmlEncode(formTypeName ?? string.Empty))
                .Replace("{{LoginUrl}}", _formNotice.LoginUrl)
                .Replace("{{ReviewUrl}}", reviewUrl);

            await _email.SendAsync(new EmailMessage
            {
                To = new List<string> { recipient.Email },
                Subject = $"{subjectPrefix} {formNotice.FormNo} - {formTypeName}",
                Body = body,
            });
        }

        #endregion

        #region 工具

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="isSingle"></param>
        /// <param name="isAuto"></param>
        /// <returns></returns>
        private string BuildOrderBy(bool isSingle, bool isAuto)
        {
            if (!isSingle)
            {
                return "ORDER BY t.HireDate DESC";
            }
            else
            {
                string c0 = isAuto ? AppointmentType.AutoActual.ToEnumString() : AppointmentType.Actual.ToEnumString();
                string c1 = isAuto ? AppointmentType.AutoAgent.ToEnumString() : AppointmentType.Agent.ToEnumString();
                string c2 = isAuto ? AppointmentType.AutoConcurrent.ToEnumString() : AppointmentType.Concurrent.ToEnumString();
                string c3 = isAuto ? AppointmentType.AutoConcurrentAgent.ToEnumString() : AppointmentType.ConcurrentAgent.ToEnumString();

                return $@"ORDER BY CASE t.AppointmentType
                        WHEN '{c0}' THEN 0
                        WHEN '{c1}' THEN 1
                        WHEN '{c2}' THEN 2
                        WHEN '{c3}' THEN 3
                        ELSE 9
                    END ASC, t.HireDate DESC";
            }
        }

        /// <summary>
        /// 一次性取出所有 AppointmentType 枚举字符串
        /// </summary>
        /// <returns></returns>
        private (string actual, string agent, string concurrent, string concurrentAgent, string autoActual, string autoAgent, string autoConcurrent, string autoConcurrentAgent) AppointmentEnumStrings() =>
        (
            AppointmentType.Actual.ToEnumString(),
            AppointmentType.Agent.ToEnumString(),
            AppointmentType.Concurrent.ToEnumString(),
            AppointmentType.ConcurrentAgent.ToEnumString(),
            AppointmentType.AutoActual.ToEnumString(),
            AppointmentType.AutoAgent.ToEnumString(),
            AppointmentType.AutoConcurrent.ToEnumString(),
            AppointmentType.AutoConcurrentAgent.ToEnumString()
        );

        /// <summary>
        /// 产生Token
        /// </summary>
        private static string GenerateSecureToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(buffer);

            // URL-safe Base64：移除 '+' '/' '='
            return Convert.ToBase64String(buffer)
                          .Replace('+', '-')
                          .Replace('/', '_')
                          .TrimEnd('=');
        }

        /// <summary>
        /// 拼接待审批地址
        /// </summary>
        /// <param name="baseDomain"></param>
        /// <param name="reviewPath"></param>
        /// <param name="noticeLanguage"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private static string BuildReviewUrl(string baseDomain, string reviewPath, string noticeLanguage, string token)
        {
            return $"{baseDomain}{reviewPath}?lang={noticeLanguage}&token={Uri.EscapeDataString(token)}";
        }

        #endregion
    }
}