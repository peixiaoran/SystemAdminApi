using SqlSugar;
using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.FormBasicInfo.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 表单流程😈
    /// </summary>
    public class FormReviewFlow
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;
        private readonly WorkflowCustomResolver _personResolver;

        public FormReviewFlow(CurrentUser loginuser, SqlSugarScope db, Language lang, WorkflowCustomResolver personResolver)
        {
            _loginuser = loginuser;
            _db = db;
            _lang = lang;
            _personResolver = personResolver;
        }

        /// <summary>
        /// 步骤审批信息 + 步骤排序（GetFullReviewFlow / GetRejectStepDrop 共用）
        /// </summary>
        private sealed record StepFlowItem(StepReview Review, int SortOrder, int IsStartStep);

        private ReviewUserProjection Projection => ReviewUserProjection.Named(_lang.Locale == "zh-CN");

        #region 查询表单审批流程

        /// <summary>
        /// 查询表单审批流程
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<FormReview> GetFullReviewFlow(long formId)
        {
            var formDetail = await GetApplyFormDetail(formId);
            var flowSteps = await BuildStepReviewList(formDetail);
            var stepReviewList = flowSteps.Select(step => step.Review).ToList();

            return new FormReview
            {
                FormId = formId,
                StepReviewList = await GetUserReviewResult(formId, formDetail.RuleId, formDetail.CurrentStepId, stepReviewList),
                RejectCount = await GetRejectCount(formId),
            };
        }

        #endregion

        #region 查询可驳回步骤

        /// <summary>
        /// 查询可驳回步骤
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<List<RejectStepDrop>> GetRejectStepDrop(long formId)
        {
            var currentStepIsStart = await _db.Queryable<FormInstanceEntity>()
                                              .With(SqlWith.NoLock)
                                              .InnerJoin<WorkflowStepEntity>((instance, step) => instance.CurrentStepId == step.StepId)
                                              .Where((instance, step) => instance.FormId == formId)
                                              .Select((instance, step) => step.IsStartStep)
                                              .FirstAsync();

            if (currentStepIsStart == 1)
            {
                return new List<RejectStepDrop>();
            }

            var formDetail = await GetApplyFormDetail(formId);
            var flowSteps = await BuildStepReviewList(formDetail);

            int currentSortOrder = int.MaxValue;
            if (formDetail.CurrentStepId.HasValue)
            {
                long currentStepId = formDetail.CurrentStepId.Value;
                var currentStep = flowSteps.FirstOrDefault(step => step.Review.StepId == currentStepId);

                if (currentStep != null)
                {
                    currentSortOrder = currentStep.SortOrder;
                }
                else
                {
                    var currentStepInfo = await _db.Queryable<WorkflowStepEntity>()
                                                   .With(SqlWith.NoLock)
                                                   .Where(step => step.StepId == currentStepId)
                                                   .FirstAsync();
                    currentSortOrder = currentStepInfo?.SortOrder ?? int.MaxValue;
                }
            }

            // 可驳回：起始步骤始终保留；其余需位于当前步骤之前，且当前操作人不在该步骤审批人中
            return flowSteps
                   .Where(step => step.Review.Skip != 1)
                   .Where(step => step.IsStartStep == 1
                                  || (step.SortOrder < currentSortOrder
                                      && !step.Review.StepReviewUser.Any(user =>
                                             user.ReviewUserId == _loginuser.UserId ||
                                             user.AgentUserId == _loginuser.UserId)))
                   .OrderBy(step => step.SortOrder)
                   .Select(step => new RejectStepDrop
                   {
                       StepId = step.Review.StepId,
                       StepName = step.Review.StepName,
                   }).ToList();
        }

        #endregion

        #region 构建步骤审批人列表

        /// <summary>
        /// 查询申请人表单详情
        /// </summary>
        private async Task<ApplyFormDetail> GetApplyFormDetail(long formId)
        {
            return await _db.Queryable<FormInstanceEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<FormTypeEntity>((instance, formtype) => instance.FormTypeId == formtype.FormTypeId)
                            .InnerJoin<UserInfoEntity>((instance, formtype, user) => instance.ApplicantUserId == user.UserId)
                            .InnerJoin<DepartmentInfoEntity>((instance, formtype, user, dept) => user.DepartmentId == dept.DepartmentId)
                            .InnerJoin<DepartmentLevelEntity>((instance, formtype, user, dept, deptlevel) => dept.DepartmentLevelId == deptlevel.DepartmentLevelId)
                            .InnerJoin<PositionInfoEntity>((instance, formtype, user, dept, deptlevel, position) => user.PositionId == position.PositionId)
                            .Where((instance, formtype, user, dept, deptlevel, position) => instance.FormId == formId)
                            .Select((instance, formtype, user, dept, deptlevel, position) => new ApplyFormDetail
                            {
                                FormId = instance.FormId,
                                FormTypeId = instance.FormTypeId,
                                RuleId = instance.RuleId,
                                CurrentStepId = instance.CurrentStepId,
                                UserId = user.UserId,
                                DeptId = dept.DepartmentId,
                                DeptLevelSort = deptlevel.SortOrder,
                                PositionSort = position.SortOrder
                            }).FirstAsync();
        }

        /// <summary>
        /// 沿规则步骤链构建各步骤的审批人列表
        /// </summary>
        private async Task<List<StepFlowItem>> BuildStepReviewList(ApplyFormDetail formDetail)
        {
            // 申请人上级部门列表（包含申请人所在部门）
            var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                           .With(SqlWith.NoLock)
                                           .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

            // 一次取出规则全部步骤，内存中沿链遍历，避免循环内逐步查询
            var ruleSteps = await _db.Queryable<WorkflowRuleStepEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(rule => rule.RuleId == formDetail.RuleId)
                                     .ToListAsync();

            var stepIds = ruleSteps.Select(rule => rule.CurrentStepId).Distinct().ToList();
            var stepInfoMap = (await _db.Queryable<WorkflowStepEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(step => stepIds.Contains(step.StepId))
                                        .ToListAsync())
                                        .ToDictionary(step => step.StepId);
            var nextStepMap = ruleSteps.ToDictionary(rule => rule.CurrentStepId, rule => rule.NextStepId);

            bool isChinese = _lang.Locale == "zh-CN";
            var result = new List<StepFlowItem>();
            var visited = new HashSet<long>();

            long? currentStepId = ruleSteps.FirstOrDefault(rule => rule.SortOrder == 1)?.CurrentStepId;
            while (currentStepId.HasValue && visited.Add(currentStepId.Value))
            {
                if (!stepInfoMap.TryGetValue(currentStepId.Value, out var stepInfo))
                {
                    break;
                }

                var stepReview = new StepReview
                {
                    StepId = stepInfo.StepId,
                    StepName = isChinese ? stepInfo.StepNameCn : stepInfo.StepNameEn,
                };

                var userReview = await ResolveStepReviewUsers(formDetail, applyParentDept, stepInfo, stepReview);
                stepReview.StepReviewUser.AddRange(userReview);

                result.Add(new StepFlowItem(stepReview, stepInfo.SortOrder, stepInfo.IsStartStep));

                currentStepId = nextStepMap.TryGetValue(currentStepId.Value, out var nextStepId) ? nextStepId : null;
            }

            return result;
        }

        /// <summary>
        /// 按步骤指派类型解析审批人；组织架构级别不足或自定义找不到人时标记步骤跳过
        /// </summary>
        private async Task<List<UserReview>> ResolveStepReviewUsers(ApplyFormDetail formDetail, List<DepartmentInfoEntity> applyParentDept, WorkflowStepEntity stepInfo, StepReview stepReview)
        {
            if (stepInfo.IsStartStep == 1)
            {
                return await GetStartReviewUser(formDetail.UserId);
            }

            if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                var orgInfo = await _db.Queryable<WorkflowStepOrgEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(steporg => steporg.StepId == stepInfo.StepId)
                                       .FirstAsync();

                var deptInfo = await _db.Queryable<DepartmentLevelEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(dept => dept.DepartmentLevelId == orgInfo.DeptLeaveId)
                                        .FirstAsync();
                var posInfo = await _db.Queryable<PositionInfoEntity>()
                                       .With(SqlWith.NoLock)
                                       .Where(pos => pos.PositionId == orgInfo.PositionId)
                                       .FirstAsync();

                if (formDetail.DeptLevelSort < deptInfo.SortOrder || formDetail.PositionSort <= posInfo.SortOrder)
                {
                    stepReview.Skip = 1;
                    return new List<UserReview>();
                }

                return await GetOrgReviewUser(applyParentDept, orgInfo.DeptLeaveId, orgInfo.PositionId, stepInfo.ReviewMode);
            }

            if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                var deptUserInfo = await _db.Queryable<WorkflowStepDeptUserEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(stepdeptuser => stepdeptuser.StepId == stepInfo.StepId)
                                            .FirstAsync();

                return await GetDeptUserReviewUser(deptUserInfo.DepartmentId, deptUserInfo.PositionId, stepInfo.ReviewMode);
            }

            if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                var userInfo = await _db.Queryable<WorkflowStepUserEntity>()
                                        .With(SqlWith.NoLock)
                                        .Where(stepuser => stepuser.StepId == stepInfo.StepId)
                                        .FirstAsync();

                return await GetUserReviewUser(userInfo.UserId, stepInfo.ReviewMode);
            }

            if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                var customInfo = await _db.Queryable<WorkflowStepCustomEntity>()
                                          .With(SqlWith.NoLock)
                                          .Where(stepcustom => stepcustom.StepId == stepInfo.StepId)
                                          .FirstAsync();

                var userReview = await GetCustomReviewUser(formDetail.FormId, customInfo.Guidance, stepInfo.ReviewMode);
                if (userReview.Count == 0)
                {
                    stepReview.Skip = 1;
                }

                return userReview;
            }

            return new List<UserReview>();
        }

        #endregion

        #region 查询各指派类型审批人员

        /// <summary>
        /// 查询起始步骤审批人员
        /// </summary>
        /// <param name="applicantUserId"></param>
        /// <returns></returns>
        public async Task<List<UserReview>> GetStartReviewUser(long applicantUserId)
        {
            bool isChinese = _lang.Locale == "zh-CN";
            string userNameCol = isChinese ? "[user].UserNameCn" : "[user].UserNameEn";
            string agentNameCol = isChinese ? "agentusers.UserNameCn" : "agentusers.UserNameEn";
            string dicNameCol = isChinese ? "dic.DicNameCn" : "dic.DicNameEn";

            var (actual, agent, _, _, _, _, _, _) = ReviewUserSql.AppointmentEnumStrings();

            #region SQL

            string sql = $@"
            SELECT
                [user].UserId AS ReviewUserId,
                {userNameCol} AS ReviewUserName,
                agentusers.UserId AS AgentUserId,
                {agentNameCol} AS AgentUserName,
                CASE
                    WHEN agent.AgentUserId IS NOT NULL THEN @Agent
                    ELSE @Actual
                END AS AppointmentType,
                CASE
                    WHEN agent.AgentUserId IS NOT NULL
                        THEN (
                            SELECT {dicNameCol}
                            FROM Basic.DictionaryInfo dic
                            WHERE dic.DicType = 'AppointmentType'
                              AND dic.DicCode = @Agent
                        )
                    ELSE (
                        SELECT {dicNameCol}
                        FROM Basic.DictionaryInfo dic
                        WHERE dic.DicType = 'AppointmentType'
                          AND dic.DicCode = @Actual
                    )
                END AS AppointmentTypeName
            FROM
                Basic.UserInfo [user]
            LEFT JOIN Basic.UserAgent agent
                ON [user].UserId = agent.SubstituteUserId
               AND agent.StartTime <= @Now
               AND agent.EndTime >= @Now
            LEFT JOIN Basic.UserInfo agentusers
                ON agent.AgentUserId = agentusers.UserId
            WHERE
                [user].UserId = @ApplicantUserId";

            #endregion

            var result = await _db.Ado.SqlQueryAsync<UserReview>(sql, new[]
            {
                new SugarParameter("@ApplicantUserId", applicantUserId),
                new SugarParameter("@Now", DateTime.Now),
                new SugarParameter("@Actual", actual),
                new SugarParameter("@Agent", agent),
            });

            return result ?? new List<UserReview>();
        }

        /// <summary>
        /// 查询审批人身份 - 按组织架构
        /// </summary>
        /// <param name="applyParentDept"></param>
        /// <param name="deptLeaveId"></param>
        /// <param name="positionId"></param>
        /// <param name="reviewMode"></param>
        /// <returns></returns>
        public async Task<List<UserReview>> GetOrgReviewUser(List<DepartmentInfoEntity> applyParentDept, long deptLeaveId, long positionId, string reviewMode)
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

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.Org, parentDeptIds, isSingle,
                new SugarParameter("@DeptLevelSort", deptlevel.SortOrder),
                new SugarParameter("@PositionSort", position.SortOrder));

            if (exactResult.Any())
            {
                return exactResult;
            }

            return await FindDowngradeReviewUsers(parentDeptIds, position.SortOrder, deptlevel.SortOrder, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按指定部门职级
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="positionId"></param>
        /// <param name="reviewMode"></param>
        /// <returns></returns>
        public async Task<List<UserReview>> GetDeptUserReviewUser(long departmentId, long positionId, string reviewMode)
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

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.Dept, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@DepartmentId", departmentId),
                new SugarParameter("@PositionSort", position.SortOrder));

            if (exactResult.Any())
            {
                return exactResult;
            }

            // 降级时沿目标部门的上级部门链查找
            var targetParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                            .With(SqlWith.NoLock)
                                            .ToParentListAsync(parent => parent.ParentId, dept.DepartmentId);
            string parentDeptIds = string.Join(",", targetParentDept.Select(parent => parent.DepartmentId));

            return await FindDowngradeReviewUsers(parentDeptIds, position.SortOrder, deptlevel.SortOrder, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按指定人
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="reviewMode"></param>
        /// <returns></returns>
        public async Task<List<UserReview>> GetUserReviewUser(long userId, string reviewMode)
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

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.User, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@UserId", userId));

            if (exactResult.Any())
            {
                return exactResult;
            }

            var targetParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                            .With(SqlWith.NoLock)
                                            .ToParentListAsync(parent => parent.ParentId, dept.DepartmentId);
            string parentDeptIds = string.Join(",", targetParentDept.Select(parent => parent.DepartmentId));

            return await FindDowngradeReviewUsers(parentDeptIds, position.SortOrder, deptlevel.SortOrder, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按自定义
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="guidance"></param>
        /// <param name="reviewMode"></param>
        /// <returns></returns>
        public async Task<List<UserReview>> GetCustomReviewUser(long formId, string guidance, string reviewMode)
        {
            var custom = await _personResolver.Resolve(guidance, formId);

            if (custom == null)
            {
                return new List<UserReview>();
            }

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

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.User, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@UserId", custom.UserId));

            if (exactResult.Any())
            {
                return exactResult;
            }

            var targetParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                            .With(SqlWith.NoLock)
                                            .ToParentListAsync(parent => parent.ParentId, dept.DepartmentId);
            string parentDeptIds = string.Join(",", targetParentDept.Select(parent => parent.DepartmentId));

            return await FindDowngradeReviewUsers(parentDeptIds, position.SortOrder, deptlevel.SortOrder, isSingle);
        }

        /// <summary>
        /// 精确匹配查询审批人
        /// </summary>
        private async Task<List<UserReview>> QueryExactReviewUsers(ReviewUserFilter filter, string parentDeptIds, bool isSingle, params SugarParameter[] filterParams)
        {
            var (actual, agent, concurrent, concurrentAgent, _, _, _, _) = ReviewUserSql.AppointmentEnumStrings();

            string sql = ReviewUserSql.ExactSql(
                Projection,
                filter,
                parentDeptIds,
                topN: isSingle ? "TOP 1" : "",
                orderBy: ReviewUserSql.BuildOrderBy(isSingle, isAuto: false));

            var parameters = new List<SugarParameter>
            {
                new SugarParameter("@Now", DateTime.Now),
                new SugarParameter("@Actual", actual),
                new SugarParameter("@Agent", agent),
                new SugarParameter("@Concurrent", concurrent),
                new SugarParameter("@ConcurrentAgent", concurrentAgent),
            };
            parameters.AddRange(filterParams);

            var result = await _db.Ado.SqlQueryAsync<UserReview>(sql, parameters.ToArray());
            return result ?? new List<UserReview>();
        }

        /// <summary>
        /// 自动降级查询审批人：职级自高向低、部门级别自内向外逐一尝试，取第一个命中的组合
        /// </summary>
        private async Task<List<UserReview>> FindDowngradeReviewUsers(string parentDeptIds, int fromPositionSort, int fromDeptLevelSort, bool isSingle)
        {
            var (_, _, _, _, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = ReviewUserSql.AppointmentEnumStrings();

            string sql = ReviewUserSql.AutoSql(
                Projection,
                parentDeptIds,
                topN: isSingle ? "TOP 1" : "",
                orderBy: ReviewUserSql.BuildOrderBy(isSingle, isAuto: true));

            var now = DateTime.Now;

            for (int positionSort = fromPositionSort - 1; positionSort >= 1; positionSort--)
            {
                for (int deptLevelSort = fromDeptLevelSort; deptLevelSort >= 1; deptLevelSort--)
                {
                    var result = await _db.Ado.SqlQueryAsync<UserReview>(sql, new[]
                    {
                        new SugarParameter("@Now", now),
                        new SugarParameter("@CurrentPositionSort", positionSort),
                        new SugarParameter("@CurrentDeptLevelSort", deptLevelSort),
                        new SugarParameter("@AutoActual", autoActual),
                        new SugarParameter("@AutoAgent", autoAgent),
                        new SugarParameter("@AutoConcurrent", autoConcurrent),
                        new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
                    });

                    if (result.Any())
                    {
                        return result;
                    }
                }
            }

            return new List<UserReview>();
        }

        #endregion

        #region 查询审批结果

        /// <summary>
        /// 查询人员审批结果
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="ruleId"></param>
        /// <param name="currentStepId"></param>
        /// <param name="reviewFlow"></param>
        /// <returns></returns>
        public async Task<List<StepReview>> GetUserReviewResult(long formId, long? ruleId, long? currentStepId, List<StepReview> reviewFlow)
        {
            // 1. 取得该表单的所有审批记录
            var allRecords = await _db.Queryable<FormReviewRecordEntity>()
                                      .With(SqlWith.NoLock)
                                      .Where(record => record.FormId == formId && record.RecordStatus == 1)
                                      .OrderBy(record => record.ReviewDateTime)
                                      .ToListAsync();

            // 2. 取得当前规则下每个步骤的排序号 (StepId -> SortOrder)
            var ruleSteps = await _db.Queryable<WorkflowRuleStepEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(rule => rule.RuleId == ruleId)
                                     .ToListAsync();

            var stepOrderMap = ruleSteps.ToDictionary(
                                            rulestep => rulestep.CurrentStepId,
                                            rulestep => rulestep.SortOrder
                                        );

            // 3. 预先把驳回记录抽出来
            var rejectRecords = allRecords
                                .Where(record => record.ReviewResult == ReviewResult.Reject.ToEnumString())
                                .OrderByDescending(record => record.ReviewDateTime)
                                .ToList();

            // 4. 预先把核准记录抽出来
            var approveRecords = allRecords
                                 .Where(record => record.ReviewResult == ReviewResult.Approve.ToEnumString())
                                 .ToList();

            // 5. 预先取出核准记录中签核人的姓名，供人员变动时回填历史签核人
            var historyUserNameMap = await GetReviewRecordUserNames(approveRecords);

            // 6. 逐步骤填充状态
            foreach (var flow in reviewFlow)
            {
                if (flow.Skip == 1)
                {
                    continue;
                }

                // 取步骤在流程中的排序号
                stepOrderMap.TryGetValue(flow.StepId, out int targetStepOrder);

                // 找出会影响当前被判断步骤的最后一次驳回
                var lastRejectAffectingThisStep = rejectRecords.FirstOrDefault(record =>
                {
                    if (!record.RejectStepId.HasValue)
                    {
                        return true;
                    }

                    stepOrderMap.TryGetValue(record.RejectStepId.Value, out int rejectTargetOrder);

                    return rejectTargetOrder <= targetStepOrder;
                });

                // 该步骤的有效核准起点时间
                DateTime? validAfter = lastRejectAffectingThisStep?.ReviewDateTime;

                bool isCurrentStep = currentStepId == flow.StepId;

                // 该步骤在有效时间后的核准记录
                var stepApproveRecords = approveRecords
                                         .Where(record => record.StepId == flow.StepId && (validAfter == null || record.ReviewDateTime > validAfter.Value))
                                         .OrderBy(record => record.ReviewDateTime)
                                         .ToList();

                // 该步骤只要有人在有效时间后核准过，就认为该步骤已核准
                bool stepHasApprove = stepApproveRecords.Any();

                // 流程未变动但人员变动：签核记录里的人已不在当前解析出的审批人中；同一步骤有多笔时取最近的一笔
                var changedRecord = stepApproveRecords
                                    .LastOrDefault(record => !flow.StepReviewUser.Any(user => IsSameReviewer(user, record)));

                foreach (var user in flow.StepReviewUser)
                {
                    if (stepHasApprove)
                    {
                        user.Result = ReviewStatus.Approve.ToEnumString();
                    }
                    else
                    {
                        user.Result = isCurrentStep
                            ? ReviewStatus.UnderReview.ToEnumString()
                            : ReviewStatus.Unsigned.ToEnumString();
                    }

                    // 该审批人自己没有签核记录，且该步骤存在他人的签核记录时，显示之前签核的人
                    bool selfSigned = stepApproveRecords.Any(record => IsSameReviewer(user, record));
                    if (!selfSigned && changedRecord != null)
                    {
                        user.HistoryUserId = changedRecord.OriginalUserId;
                        user.HistoryUserName = historyUserNameMap.TryGetValue(changedRecord.OriginalUserId, out var historyUserName)
                            ? historyUserName
                            : string.Empty;
                    }
                }
            }

            return reviewFlow;
        }

        /// <summary>
        /// 判断审批记录是否由该审批人（实或兼 / 代）签核
        /// </summary>
        private static bool IsSameReviewer(UserReview user, FormReviewRecordEntity record)
        {
            return record.OriginalUserId == user.ReviewUserId
                   || record.OperationUserId == user.ReviewUserId
                   || (user.AgentUserId.HasValue && (record.OriginalUserId == user.AgentUserId.Value || record.OperationUserId == user.AgentUserId.Value));
        }

        /// <summary>
        /// 查询审批记录中签核人的姓名（UserId -&gt; 姓名）
        /// </summary>
        private async Task<Dictionary<long, string>> GetReviewRecordUserNames(List<FormReviewRecordEntity> records)
        {
            var userIds = records.Select(record => record.OriginalUserId).Distinct().ToList();

            if (userIds.Count == 0)
            {
                return new Dictionary<long, string>();
            }

            bool isChinese = _lang.Locale == "zh-CN";

            var users = await _db.Queryable<UserInfoEntity>()
                                 .With(SqlWith.NoLock)
                                 .Where(user => userIds.Contains(user.UserId))
                                 .ToListAsync();

            return users.ToDictionary(user => user.UserId, user => isChinese ? user.UserNameCn : user.UserNameEn);
        }

        /// <summary>
        /// 查询表单总计驳回次数
        /// </summary>
        public async Task<int> GetRejectCount(long formId)
        {
            return await _db.Queryable<FormReviewRecordEntity>()
                            .With(SqlWith.NoLock)
                            .Where(record => record.FormId == formId && record.ReviewResult == ReviewResult.Reject.ToEnumString())
                            .CountAsync();
        }

        #endregion
    }
}
