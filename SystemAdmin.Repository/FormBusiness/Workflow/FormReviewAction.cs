using SqlSugar;
using System.Data;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormOperate.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Upsert;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewAction.Dto;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.UserSettings.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 表单签核、驳回、撤回😈
    /// </summary>
    public class FormReviewAction
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly WorkflowCustomResolver _personResolver;
        private readonly WorkflowStepCompletion _stepcomletion;

        public FormReviewAction(
            CurrentUser loginuser,
            SqlSugarScope db,
            WorkflowCustomResolver personResolver,
            WorkflowStepCompletion stepcomletion)
        {
            _loginuser = loginuser;
            _db = db;
            _personResolver = personResolver;
            _stepcomletion = stepcomletion;
        }

        #region 表单核准

        /// <summary>
        /// 表单核准
        /// </summary>
        public async Task<Result<bool>> FromApprove(ApproveForm approveForm)
        {
            long formId = long.Parse(approveForm.FormId);
            var (stepInfo, ruleId) = await GetCurrentStepInfo(formId);

            // 手动核准
            bool hasPendingUser = await ProcessApprove(formId, stepInfo, ReviewType.Manual, approveForm.Comment);

            // 会审尚有其他人未签，不推进流程，也不重复发送邮件。
            if (hasPendingUser)
            {
                return Result<bool>.Ok(true);
            }

            // 当前步骤完成后执行业务 Guidance。
            var guidanceResult = await ExecuteStepGuidance(formId);
            if (guidanceResult.Code != 200)
            {
                return guidanceResult;
            }

            var nextStep = await GetNextStep(ruleId, stepInfo.StepId);

            if (nextStep.NextStepId == null)
            {
                await ApproveForm(formId);
                return Result<bool>.Ok(true);
            }

            await AdvanceCurrentStep(formId, nextStep.NextStepId);

            // 自动核准
            return await AutoApprove(formId);
        }

        /// <summary>
        /// 手动核准
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="stepInfo"></param>
        /// <param name="reviewType"></param>
        /// <param name="comment"></param>
        /// <returns>true = 当前步骤还有剩余待审批人</returns>
        public async Task<bool> ProcessApprove(long formId, WorkflowStepEntity stepInfo, ReviewType reviewType, string comment)
        {
            string reviewMode = stepInfo.ReviewMode;
            var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

            // 自定义人可能会找不到，例如加签人员
            if (selfAppointments.Count > 0)
            {
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
            }

            return await _db.Queryable<PendingReviewEntity>()
                            .With(SqlWith.NoLock)
                            .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                            .AnyAsync();
        }

        /// <summary>
        /// 自动核准
        /// </summary>
        public async Task<Result<bool>> AutoApprove(long formId)
        {
            while (true)
            {
                var (stepInfo, ruleId) = await GetCurrentStepInfo(formId);

                await EnsurePendingReviewExists(formId, stepInfo.StepId);

                if (await ShouldSkipStep(formId, stepInfo))
                {
                    await _db.Deleteable<PendingReviewEntity>()
                             .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                             .ExecuteCommandAsync();

                    var skippedNext = await GetNextStep(ruleId, stepInfo.StepId);

                    if (skippedNext.NextStepId == null)
                    {
                        await ApproveForm(formId);

                        return Result<bool>.Ok(true);
                    }

                    await AdvanceCurrentStep(formId, skippedNext.NextStepId);
                    continue;
                }

                var selfPendingUserId = await _db.Queryable<PendingReviewEntity>()
                                                 .With(SqlWith.NoLock)
                                                 .LeftJoin<UserAgentEntity>((pending, agent) =>
                                                     pending.ReviewUserId == agent.SubstituteUserId &&
                                                     agent.StartTime <= DateTime.Now &&
                                                     agent.EndTime >= DateTime.Now)
                                                 .Where((pending, agent) =>
                                                     pending.FormId == formId &&
                                                     pending.StepId == stepInfo.StepId &&
                                                     (pending.ReviewUserId == _loginuser.UserId ||
                                                      agent.AgentUserId == _loginuser.UserId))
                                                 .Select((pending, agent) => pending.ReviewUserId)
                                                 .FirstAsync();

                // 后续步骤不包含当前操作人：事务提交后通知该步骤待签人。
                if (selfPendingUserId == 0)
                {
                    return Result<bool>.Ok(true);
                }

                string reviewMode = stepInfo.ReviewMode;

                if (reviewMode == ReviewMode.Review.ToEnumString())
                {
                    var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                    if (selfAppointments.Count > 0)
                    {
                        await _db.Deleteable<PendingReviewEntity>()
                                 .Where(pending => pending.FormId == formId && pending.StepId == stepInfo.StepId)
                                 .ExecuteCommandAsync();

                        await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, string.Empty, ReviewType.Automatic, _loginuser.UserId);
                    }
                }
                else if (reviewMode == ReviewMode.OrReview.ToEnumString())
                {
                    var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                    if (selfAppointments.Count > 0)
                    {
                        var otherPendingUserIds = await _db.Queryable<PendingReviewEntity>()
                                                           .With(SqlWith.NoLock)
                                                           .Where(pending =>
                                                               pending.FormId == formId &&
                                                               pending.StepId == stepInfo.StepId &&
                                                               pending.ReviewUserId != selfPendingUserId)
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
                }
                else if (reviewMode == ReviewMode.AndReview.ToEnumString())
                {
                    var selfAppointments = await GetStepReviewUser(formId, stepInfo, _loginuser.UserId);

                    if (selfAppointments.Count > 0)
                    {
                        await _db.Deleteable<PendingReviewEntity>()
                                 .Where(pending =>
                                     pending.FormId == formId &&
                                     pending.StepId == stepInfo.StepId &&
                                     pending.ReviewUserId == selfPendingUserId)
                                 .ExecuteCommandAsync();

                        await InsertReviewRecords(formId, stepInfo.StepId, ReviewResult.Approve, null, selfAppointments, string.Empty, ReviewType.Automatic, _loginuser.UserId);

                        bool othersPending = await _db.Queryable<PendingReviewEntity>()
                                                      .With(SqlWith.NoLock)
                                                      .Where(pending =>
                                                          pending.FormId == formId &&
                                                          pending.StepId == stepInfo.StepId)
                                                      .AnyAsync();

                        if (othersPending)
                        {
                            return Result<bool>.Ok(true);
                        }
                    }
                }

                var guidanceResult = await ExecuteStepGuidance(formId);
                if (guidanceResult.Code != 200)
                {
                    return guidanceResult;
                }

                var nextStep = await GetNextStep(ruleId, stepInfo.StepId);

                if (nextStep.NextStepId == null)
                {
                    await ApproveForm(formId);

                    return Result<bool>.Ok(true);
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
        public async Task EnsurePendingReviewExists(long formId, long stepId)
        {
            bool hasPending = await _db.Queryable<PendingReviewEntity>().With(SqlWith.NoLock)
                                       .Where(pending => pending.FormId == formId && pending.StepId == stepId)
                                       .AnyAsync();
            if (hasPending)
            {
                return;
            }
            else
            {
                var stepInfo = await _db.Queryable<WorkflowStepEntity>()
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
        public async Task<List<UserAppointment>> GetStepReviewUser(long formId, WorkflowStepEntity stepInfo, long? reviewUserId = null)
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

            var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
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
                result = await GetOrgReviewUser(applyParentDept, orgInfo.DeptLeaveId, orgInfo.PositionId, stepInfo.ReviewMode);
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
        public async Task<List<UserAppointment>> GetStartReviewUser(long applicantUserId)
        {
            var (actual, agent, _, _, _, _, _, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            #region SQL

            string sql = $@"
            SELECT TOP 1
                t.ReviewUserId,
                t.AgentUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                    CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN @Agent
                        ELSE @Actual
                    END AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentuser
                    ON agent.AgentUserId = agentuser.UserId
                WHERE
                    [user].UserId = @ApplicantUserId
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
                t.ReviewUserId,
                t.AgentUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                    CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN @Agent
                        ELSE @Actual
                    END AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                INNER JOIN Basic.DepartmentInfo dept
                    ON [user].DepartmentId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON [user].PositionId = position.PositionId
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentuser
                    ON agent.AgentUserId = agentuser.UserId
                WHERE
                    dept.DepartmentId IN ({parentDeptIds})
                  AND deptlevel.SortOrder = @DeptLevelSort
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0

                UNION ALL

                SELECT
                    [user].UserId AS ReviewUserId,
                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                    CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent
                        ELSE @Concurrent
                    END AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId
                INNER JOIN Basic.DepartmentInfo dept
                    ON partime.PartTimeDeptId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON partime.PartTimePositionId = position.PositionId
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentuser
                    ON agent.AgentUserId = agentuser.UserId
                WHERE
                    dept.DepartmentId IN ({parentDeptIds})
                  AND deptlevel.SortOrder = @DeptLevelSort
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0
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

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                        SELECT {topN}
                            t.ReviewUserId,
                            t.AgentUserId,
                            t.AppointmentType
                        FROM (
                            SELECT
                                [user].UserId AS ReviewUserId,
                                ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                CASE
                                    WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent
                                    ELSE @AutoActual
                                END AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserInfo [user]
                            INNER JOIN Basic.DepartmentInfo dept
                                ON [user].DepartmentId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON [user].PositionId = position.PositionId
                            LEFT JOIN Basic.UserAgent agent
                                ON [user].UserId = agent.SubstituteUserId
                               AND agent.StartTime <= @Now
                               AND agent.EndTime >= @Now
                            LEFT JOIN Basic.UserInfo agentuser
                                ON agent.AgentUserId = agentuser.UserId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0

                            UNION ALL

                            SELECT
                                [user].UserId AS ReviewUserId,
                                ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                CASE
                                    WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent
                                    ELSE @AutoConcurrent
                                END AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserPartTime partime
                            INNER JOIN Basic.UserInfo [user]
                                ON partime.UserId = [user].UserId
                            INNER JOIN Basic.DepartmentInfo dept
                                ON partime.PartTimeDeptId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON partime.PartTimePositionId = position.PositionId
                            LEFT JOIN Basic.UserAgent agent
                                ON [user].UserId = agent.SubstituteUserId
                               AND agent.StartTime <= @Now
                               AND agent.EndTime >= @Now
                            LEFT JOIN Basic.UserInfo agentuser
                                ON agent.AgentUserId = agentuser.UserId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0
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
                t.ReviewUserId,
                t.AgentUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                    CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN @Agent
                        ELSE @Actual
                    END AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                INNER JOIN Basic.DepartmentInfo dept
                    ON [user].DepartmentId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON [user].PositionId = position.PositionId
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentuser
                    ON agent.AgentUserId = agentuser.UserId
                WHERE
                    dept.DepartmentId = @DepartmentId
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0

                UNION ALL

                SELECT
                    [user].UserId AS ReviewUserId,
                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                    CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent
                        ELSE @Concurrent
                    END AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId
                INNER JOIN Basic.DepartmentInfo dept
                    ON partime.PartTimeDeptId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON partime.PartTimePositionId = position.PositionId
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentuser
                    ON agent.AgentUserId = agentuser.UserId
                WHERE
                    dept.DepartmentId = @DepartmentId
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0
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

                var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                               .With(SqlWith.NoLock)
                                               .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyParentDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                        SELECT {topN}
                            t.ReviewUserId,
                            t.AgentUserId,
                            t.AppointmentType
                        FROM (
                            SELECT
                                [user].UserId AS ReviewUserId,
                                ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                CASE
                                    WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent
                                    ELSE @AutoActual
                                END AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserInfo [user]
                            INNER JOIN Basic.DepartmentInfo dept
                                ON [user].DepartmentId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON [user].PositionId = position.PositionId
                            LEFT JOIN Basic.UserAgent agent
                                ON [user].UserId = agent.SubstituteUserId
                               AND agent.StartTime <= @Now
                               AND agent.EndTime >= @Now
                            LEFT JOIN Basic.UserInfo agentuser
                                ON agent.AgentUserId = agentuser.UserId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0

                            UNION ALL

                            SELECT
                                [user].UserId AS ReviewUserId,
                                ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                CASE
                                    WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent
                                    ELSE @AutoConcurrent
                                END AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserPartTime partime
                            INNER JOIN Basic.UserInfo [user]
                                ON partime.UserId = [user].UserId
                            INNER JOIN Basic.DepartmentInfo dept
                                ON partime.PartTimeDeptId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON partime.PartTimePositionId = position.PositionId
                            LEFT JOIN Basic.UserAgent agent
                                ON [user].UserId = agent.SubstituteUserId
                               AND agent.StartTime <= @Now
                               AND agent.EndTime >= @Now
                            LEFT JOIN Basic.UserInfo agentuser
                                ON agent.AgentUserId = agentuser.UserId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0
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

                var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                               .With(SqlWith.NoLock)
                                               .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyParentDept.Select(dept => dept.DepartmentId).ToList());

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
                    t.ReviewUserId,
                    t.AgentUserId,
                    t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0) AS AgentUserId,
                        CASE
                            WHEN agent.AgentUserId IS NOT NULL THEN @Agent
                            ELSE @Actual
                        END AS AppointmentType,
                        [user].HireDate
                    FROM
                        Basic.UserInfo [user]
                    LEFT JOIN Basic.UserAgent agent
                        ON [user].UserId = agent.SubstituteUserId
                       AND agent.StartTime <= @Now
                       AND agent.EndTime >= @Now
                    LEFT JOIN Basic.UserInfo agentuser
                        ON agent.AgentUserId = agentuser.UserId
                    WHERE
                        [user].UserId = @UserId
                      AND [user].IsReview = 1
                      AND [user].IsEmployed = 1
                      AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        ISNULL(agentuser.UserId, 0) AS AgentUserId,
                        CASE
                            WHEN agent.AgentUserId IS NOT NULL THEN @ConcurrentAgent
                            ELSE @Concurrent
                        END AS AppointmentType,
                        [user].HireDate
                    FROM
                        Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo [user]
                        ON partime.UserId = [user].UserId
                    LEFT JOIN Basic.UserAgent agent
                        ON [user].UserId = agent.SubstituteUserId
                       AND agent.StartTime <= @Now
                       AND agent.EndTime >= @Now
                    LEFT JOIN Basic.UserInfo agentuser
                        ON agent.AgentUserId = agentuser.UserId
                    WHERE
                        partime.UserId = @UserId
                      AND [user].IsReview = 1
                      AND [user].IsEmployed = 1
                      AND [user].IsFreeze = 0
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

                    var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                                   .With(SqlWith.NoLock)
                                                   .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                    var parentDeptIds = string.Join(',', applyParentDept.Select(dept => dept.DepartmentId).ToList());

                    while (currentPositionSort >= 1)
                    {
                        while (currentDeptLevelSort >= 1)
                        {
                            #region SQL

                            var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId,
                                t.AgentUserId,
                                t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                    CASE
                                        WHEN agent.AgentUserId IS NOT NULL THEN @AutoAgent
                                        ELSE @AutoActual
                                    END AS AppointmentType,
                                    [user].HireDate
                                FROM
                                    Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo dept
                                    ON [user].DepartmentId = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel
                                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo position
                                    ON [user].PositionId = position.PositionId
                                LEFT JOIN Basic.UserAgent agent
                                    ON [user].UserId = agent.SubstituteUserId
                                   AND agent.StartTime <= @Now
                                   AND agent.EndTime >= @Now
                                LEFT JOIN Basic.UserInfo agentuser
                                    ON agent.AgentUserId = agentuser.UserId
                                WHERE
                                    dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort
                                  AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1
                                  AND [user].IsEmployed = 1
                                  AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    ISNULL(agentuser.UserId, 0) AS AgentUserId,
                                    CASE
                                        WHEN agent.AgentUserId IS NOT NULL THEN @AutoConcurrentAgent
                                        ELSE @AutoConcurrent
                                    END AS AppointmentType,
                                    [user].HireDate
                                FROM
                                    Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo [user]
                                    ON partime.UserId = [user].UserId
                                INNER JOIN Basic.DepartmentInfo dept
                                    ON partime.PartTimeDeptId = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel
                                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo position
                                    ON partime.PartTimePositionId = position.PositionId
                                LEFT JOIN Basic.UserAgent agent
                                    ON [user].UserId = agent.SubstituteUserId
                                   AND agent.StartTime <= @Now
                                   AND agent.EndTime >= @Now
                                LEFT JOIN Basic.UserInfo agentuser
                                    ON agent.AgentUserId = agentuser.UserId
                                WHERE
                                    dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort
                                  AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1
                                  AND [user].IsEmployed = 1
                                  AND [user].IsFreeze = 0
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
        public async Task<List<UserAppointment>> GetActualConStepReviewUser(long formId, WorkflowStepEntity stepInfo, long? reviewUserId = null)
        {
            var formDetail = await _db.Queryable<FormInstanceEntity>()
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

            var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                           .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

            List<UserAppointment> result;

            if (stepInfo.IsStartStep == 1)
            {
                result = await GetActualConStartReviewUser(formDetail.UserId);
            }
            else if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                var orgInfo = await _db.Queryable<WorkflowStepOrgEntity>()
                                       .Where(steporg => steporg.StepId == stepInfo.StepId)
                                       .FirstAsync();
                result = await GetActualConOrgReviewUser(applyParentDept, orgInfo.DeptLeaveId, orgInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                var deptUserInfo = await _db.Queryable<WorkflowStepDeptUserEntity>()
                                            .Where(stepdeptuser => stepdeptuser.StepId == stepInfo.StepId)
                                            .FirstAsync();
                result = await GetActualConDeptUserReviewUser(deptUserInfo.DepartmentId, deptUserInfo.PositionId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                var userInfo = await _db.Queryable<WorkflowStepUserEntity>()
                                        .Where(stepuser => stepuser.StepId == stepInfo.StepId)
                                        .FirstAsync();
                result = await GetActualConUserReviewUser(userInfo.UserId, stepInfo.ReviewMode);
            }
            else if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                var customInfo = await _db.Queryable<WorkflowStepCustomEntity>()
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

        #region 查询单步骤各指派类型审批人身份

        /// <summary>
        /// 查询起始步骤审批人身份
        /// </summary>
        public async Task<List<UserAppointment>> GetActualConStartReviewUser(long applicantUserId)
        {
            var (actual, _, _, _, _, _, _, _) = AppointmentEnumStrings();
            var now = DateTime.Now;

            #region SQL

            string sql = $@"
            SELECT TOP 1
                t.ReviewUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    @Actual AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                WHERE
                    [user].UserId = @ApplicantUserId
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
                                     .Where(deptlevel => deptlevel.DepartmentLevelId == deptLeaveId)
                                     .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
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
                t.ReviewUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    @Actual AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                INNER JOIN Basic.DepartmentInfo dept
                    ON [user].DepartmentId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON [user].PositionId = position.PositionId
                WHERE
                    dept.DepartmentId IN ({parentDeptIds})
                  AND deptlevel.SortOrder = @DeptLevelSort
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0

                UNION ALL

                SELECT
                    [user].UserId AS ReviewUserId,
                    @Concurrent AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId
                INNER JOIN Basic.DepartmentInfo dept
                    ON partime.PartTimeDeptId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON partime.PartTimePositionId = position.PositionId
                WHERE
                    dept.DepartmentId IN ({parentDeptIds})
                  AND deptlevel.SortOrder = @DeptLevelSort
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0
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
                            t.ReviewUserId,
                            t.AppointmentType
                        FROM (
                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoActual AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserInfo [user]
                            INNER JOIN Basic.DepartmentInfo dept
                                ON [user].DepartmentId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON [user].PositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0

                            UNION ALL

                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoConcurrent AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserPartTime partime
                            INNER JOIN Basic.UserInfo [user]
                                ON partime.UserId = [user].UserId
                            INNER JOIN Basic.DepartmentInfo dept
                                ON partime.PartTimeDeptId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON partime.PartTimePositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0
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
                                    .Where(position => position.PositionId == positionId)
                                    .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .Where(dept => dept.DepartmentId == departmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
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
                t.ReviewUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    @Actual AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                INNER JOIN Basic.DepartmentInfo dept
                    ON [user].DepartmentId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON [user].PositionId = position.PositionId
                WHERE
                    dept.DepartmentId = @DepartmentId
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0

                UNION ALL

                SELECT
                    [user].UserId AS ReviewUserId,
                    @Concurrent AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId
                INNER JOIN Basic.DepartmentInfo dept
                    ON partime.PartTimeDeptId = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON partime.PartTimePositionId = position.PositionId
                WHERE
                    dept.DepartmentId = @DepartmentId
                  AND position.SortOrder = @PositionSort
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0
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

                var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                               .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyParentDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                        SELECT {topN}
                            t.ReviewUserId,
                            t.AppointmentType
                        FROM (
                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoActual AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserInfo [user]
                            INNER JOIN Basic.DepartmentInfo dept
                                ON [user].DepartmentId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON [user].PositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0

                            UNION ALL

                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoConcurrent AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserPartTime partime
                            INNER JOIN Basic.UserInfo [user]
                                ON partime.UserId = [user].UserId
                            INNER JOIN Basic.DepartmentInfo dept
                                ON partime.PartTimeDeptId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON partime.PartTimePositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0
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
                                .Where(user => user.UserId == userId)
                                .FirstAsync();

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .Where(position => position.PositionId == user.PositionId)
                                    .FirstAsync();

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .Where(dept => dept.DepartmentId == user.DepartmentId)
                                .FirstAsync();

            var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
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
                t.ReviewUserId,
                t.AppointmentType
            FROM (
                SELECT
                    [user].UserId AS ReviewUserId,
                    @Actual AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserInfo [user]
                WHERE
                    [user].UserId = @UserId
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0

                UNION ALL

                SELECT
                    [user].UserId AS ReviewUserId,
                    @Concurrent AS AppointmentType,
                    [user].HireDate
                FROM
                    Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId
                WHERE
                    partime.UserId = @UserId
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0
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

                var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                               .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                var parentDeptIds = string.Join(',', applyParentDept.Select(dept => dept.DepartmentId).ToList());

                while (currentPositionSort >= 1)
                {
                    while (currentDeptLevelSort >= 1)
                    {
                        #region SQL

                        var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                        SELECT {topN}
                            t.ReviewUserId,
                            t.AppointmentType
                        FROM (
                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoActual AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserInfo [user]
                            INNER JOIN Basic.DepartmentInfo dept
                                ON [user].DepartmentId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON [user].PositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0

                            UNION ALL

                            SELECT
                                [user].UserId AS ReviewUserId,
                                @AutoConcurrent AS AppointmentType,
                                [user].HireDate
                            FROM
                                Basic.UserPartTime partime
                            INNER JOIN Basic.UserInfo [user]
                                ON partime.UserId = [user].UserId
                            INNER JOIN Basic.DepartmentInfo dept
                                ON partime.PartTimeDeptId = dept.DepartmentId
                            INNER JOIN Basic.DepartmentLevel deptlevel
                                ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                            INNER JOIN Basic.PositionInfo position
                                ON partime.PartTimePositionId = position.PositionId
                            WHERE
                                dept.DepartmentId IN ({parentDeptIds})
                              AND position.SortOrder = @CurrentPositionSort
                              AND deptlevel.SortOrder = @CurrentDeptLevelSort
                              AND [user].IsReview = 1
                              AND [user].IsEmployed = 1
                              AND [user].IsFreeze = 0
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
                                    .Where(user => user.UserId == userId)
                                    .FirstAsync();

                var position = await _db.Queryable<PositionInfoEntity>()
                                        .Where(position => position.PositionId == user.PositionId)
                                        .FirstAsync();

                var dept = await _db.Queryable<DepartmentInfoEntity>()
                                    .Where(dept => dept.DepartmentId == user.DepartmentId)
                                    .FirstAsync();

                var deptlevel = await _db.Queryable<DepartmentLevelEntity>()
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
                    t.ReviewUserId,
                    t.AppointmentType
                FROM (
                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Actual AS AppointmentType,
                        [user].HireDate
                    FROM
                        Basic.UserInfo [user]
                    WHERE
                        [user].UserId = @UserId
                      AND [user].IsReview = 1
                      AND [user].IsEmployed = 1
                      AND [user].IsFreeze = 0

                    UNION ALL

                    SELECT
                        [user].UserId AS ReviewUserId,
                        @Concurrent AS AppointmentType,
                        [user].HireDate
                    FROM
                        Basic.UserPartTime partime
                    INNER JOIN Basic.UserInfo [user]
                        ON partime.UserId = [user].UserId
                    WHERE
                        partime.UserId = @UserId
                      AND [user].IsReview = 1
                      AND [user].IsEmployed = 1
                      AND [user].IsFreeze = 0
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
                                             .ToParentListAsync(dept => dept.ParentId, dept.DepartmentId);
                    var parentDeptIds = string.Join(',', applyDept.Select(dept => dept.DepartmentId).ToList());

                    while (currentPositionSort >= 1)
                    {
                        while (currentDeptLevelSort >= 1)
                        {
                            #region SQL

                            var autoResult = await _db.Ado.SqlQueryAsync<UserAppointment>($@"
                            SELECT {topN}
                                t.ReviewUserId,
                                t.AppointmentType
                            FROM (
                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoActual AS AppointmentType,
                                    [user].HireDate
                                FROM
                                    Basic.UserInfo [user]
                                INNER JOIN Basic.DepartmentInfo dept
                                    ON [user].DepartmentId = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel
                                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo position
                                    ON [user].PositionId = position.PositionId
                                WHERE
                                    dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort
                                  AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1
                                  AND [user].IsEmployed = 1
                                  AND [user].IsFreeze = 0

                                UNION ALL

                                SELECT
                                    [user].UserId AS ReviewUserId,
                                    @AutoConcurrent AS AppointmentType,
                                    [user].HireDate
                                FROM
                                    Basic.UserPartTime partime
                                INNER JOIN Basic.UserInfo [user]
                                    ON partime.UserId = [user].UserId
                                INNER JOIN Basic.DepartmentInfo dept
                                    ON partime.PartTimeDeptId = dept.DepartmentId
                                INNER JOIN Basic.DepartmentLevel deptlevel
                                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                                INNER JOIN Basic.PositionInfo position
                                    ON partime.PartTimePositionId = position.PositionId
                                WHERE
                                    dept.DepartmentId IN ({parentDeptIds})
                                  AND position.SortOrder = @CurrentPositionSort
                                  AND deptlevel.SortOrder = @CurrentDeptLevelSort
                                  AND [user].IsReview = 1
                                  AND [user].IsEmployed = 1
                                  AND [user].IsFreeze = 0
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
        public async Task<bool> ShouldSkipStep(long formId, WorkflowStepEntity stepInfo)
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
        public async Task<WorkflowRuleStepEntity> GetNextStep(long ruleId, long currentStepId)
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
        public async Task AdvanceCurrentStep(long formId, long? nextStepId)
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
        public async Task ApproveForm(long formId)
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
        public async Task<(WorkflowStepEntity stepInfo, long ruleId)> GetCurrentStepInfo(long formId)
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
        /// 执行当前步骤的 Guidance
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Result<bool>> ExecuteStepGuidance(long formId)
        {
            var form = await _db.Queryable<FormInstanceEntity>()
                                .With(SqlWith.NoLock)
                                .Where(instance => instance.FormId == formId)
                                .Select(instance => new { instance.RuleId, instance.CurrentStepId })
                                .FirstAsync();

            var guidance = await _db.Queryable<WorkflowRuleStepEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(rulestep => rulestep.RuleId == form.RuleId && rulestep.CurrentStepId == form.CurrentStepId)
                                    .Select(rulestep => rulestep.Guidance)
                                    .FirstAsync();

            if (string.IsNullOrEmpty(guidance))
            {
                return Result<bool>.Ok(true);
            }

            return await _stepcomletion.Resolve(guidance, formId);
        }

        #endregion

        #region 表单驳回

        /// <summary>
        /// 表单驳回（仅执行数据库流程，不发送邮件）
        /// </summary>
        public async Task<Result<bool>> FromReject(RejectForm rejectForm)
        {
            long formId = long.Parse(rejectForm.FormId);
            long rejectStepId = long.Parse(rejectForm.RejectStepId);

            var rejectStep = await _db.Queryable<WorkflowStepEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(step => step.StepId == rejectStepId)
                                      .FirstAsync();

            var currentStep = await _db.Queryable<FormInstanceEntity>()
                                       .With(SqlWith.NoLock)
                                       .InnerJoin<WorkflowStepEntity>((instance, step) =>
                                           instance.CurrentStepId == step.StepId)
                                       .Where((instance, step) => instance.FormId == formId)
                                       .Select((instance, step) => step)
                                       .FirstAsync();

            var selfAppointments = await GetStepReviewUser(formId, currentStep, _loginuser.UserId);

            // 清空该表单所有待审批人。删除 0 行不代表业务失败。
            await _db.Deleteable<PendingReviewEntity>()
                     .Where(pending => pending.FormId == formId)
                     .ExecuteCommandAsync();

            await InsertReviewRecords(
                formId,
                currentStep.StepId,
                ReviewResult.Reject,
                rejectStepId,
                selfAppointments,
                rejectForm.Comment,
                ReviewType.Manual,
                _loginuser.UserId);

            // 状态和当前步骤一次更新，避免中间状态。
           await _db.Updateable<FormInstanceEntity>()
                    .SetColumns(instance => new FormInstanceEntity
                    {
                        FormStatus = FormStatus.Rejected.ToEnumString(),
                        CurrentStepId = rejectStepId,
                    }).Where(instance => instance.FormId == formId)
                    .ExecuteCommandAsync();

            await EnsurePendingReviewExists(formId, rejectStepId);

            return Result<bool>.Ok(true);
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

        #endregion

        #region 工具

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="isSingle"></param>
        /// <param name="isAuto"></param>
        /// <returns></returns>
        public string BuildOrderBy(bool isSingle, bool isAuto)
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
        public (string actual, string agent, string concurrent, string concurrentAgent, string autoActual, string autoAgent, string autoConcurrent, string autoConcurrentAgent) AppointmentEnumStrings() =>
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

        #endregion
    }
}
