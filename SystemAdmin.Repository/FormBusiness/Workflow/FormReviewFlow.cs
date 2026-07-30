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

        /// <summary>
        /// 一次流程构建所需的步骤链、步骤配置与组织架构基础资料。
        /// 在进入步骤循环前批量载入，循环内仅保留每个步骤必需的审批人查询
        /// </summary>
        private sealed class FlowContext
        {
            public Dictionary<long, WorkflowStepEntity> StepInfoMap { get; set; } = new Dictionary<long, WorkflowStepEntity>();
            public Dictionary<long, long?> NextStepMap { get; set; } = new Dictionary<long, long?>();
            public Dictionary<long, int> RuleStepSortMap { get; set; } = new Dictionary<long, int>();
            public long? FirstStepId { get; set; }
            public List<DepartmentInfoEntity> ApplyParentDept { get; set; } = new List<DepartmentInfoEntity>();
            public Dictionary<long, WorkflowStepOrgEntity> OrgConfigMap { get; set; } = new Dictionary<long, WorkflowStepOrgEntity>();
            public Dictionary<long, WorkflowStepDeptUserEntity> DeptUserConfigMap { get; set; } = new Dictionary<long, WorkflowStepDeptUserEntity>();
            public Dictionary<long, WorkflowStepUserEntity> UserConfigMap { get; set; } = new Dictionary<long, WorkflowStepUserEntity>();
            public Dictionary<long, WorkflowStepCustomEntity> CustomConfigMap { get; set; } = new Dictionary<long, WorkflowStepCustomEntity>();
            public Dictionary<long, DepartmentInfoEntity> DeptMap { get; set; } = new Dictionary<long, DepartmentInfoEntity>();
            public Dictionary<long, UserInfoEntity> UserMap { get; set; } = new Dictionary<long, UserInfoEntity>();
            public Dictionary<long, int> DeptLevelSortMap { get; set; } = new Dictionary<long, int>();
            public Dictionary<long, int> PositionSortMap { get; set; } = new Dictionary<long, int>();

            public int DeptLevelSort(long deptLevelId) => DeptLevelSortMap.TryGetValue(deptLevelId, out var sortOrder) ? sortOrder : 0;

            public int PositionSort(long positionId) => PositionSortMap.TryGetValue(positionId, out var sortOrder) ? sortOrder : 0;
        }

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
            var context = await BuildFlowContext(formDetail);
            var flowSteps = await BuildStepReviewList(formDetail, context);
            var stepReviewList = flowSteps.Select(step => step.Review).ToList();

            // 审批记录一次取回：步骤状态只认有效记录，驳回次数按全部记录统计
            var reviewRecords = await _db.Queryable<FormReviewRecordEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(record => record.FormId == formId)
                                         .OrderBy(record => record.ReviewDateTime)
                                         .ToListAsync();

            FillUserReviewResult(formDetail.CurrentStepId, context.RuleStepSortMap, stepReviewList, reviewRecords);

            return new FormReview
            {
                FormId = formId,
                StepReviewList = stepReviewList,
                RejectCount = reviewRecords.Count(record => record.ReviewResult == ReviewResult.Reject.ToEnumString()),
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
            var context = await BuildFlowContext(formDetail);
            var flowSteps = await BuildStepReviewList(formDetail, context);

            int currentSortOrder = int.MaxValue;
            if (formDetail.CurrentStepId.HasValue)
            {
                long currentStepId = formDetail.CurrentStepId.Value;
                var currentStep = flowSteps.FirstOrDefault(step => step.Review.StepId == currentStepId);

                if (currentStep != null)
                {
                    currentSortOrder = currentStep.SortOrder;
                }
                else if (context.StepInfoMap.TryGetValue(currentStepId, out var currentStepInfo))
                {
                    // 当前步骤未出现在步骤链上（如被跳过），仍可由规则内的步骤资料取排序
                    currentSortOrder = currentStepInfo.SortOrder;
                }
                else
                {
                    var offRuleStep = await _db.Queryable<WorkflowStepEntity>()
                                               .With(SqlWith.NoLock)
                                               .Where(step => step.StepId == currentStepId)
                                               .FirstAsync();
                    currentSortOrder = offRuleStep?.SortOrder ?? int.MaxValue;
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
        /// 预载步骤链、各指派类型的步骤配置与组织架构基础资料。
        /// 步骤配置与部门/人员按 Id 集合批量取回，部门级别与职级为小型基础资料整表缓存，
        /// 使步骤循环内不再逐步骤往返数据库
        /// </summary>
        private async Task<FlowContext> BuildFlowContext(ApplyFormDetail formDetail)
        {
            var ruleSteps = await _db.Queryable<WorkflowRuleStepEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(rule => rule.RuleId == formDetail.RuleId)
                                     .ToListAsync();

            var stepIds = ruleSteps.Select(rule => rule.CurrentStepId).Distinct().ToList();
            var stepInfos = await _db.Queryable<WorkflowStepEntity>()
                                     .With(SqlWith.NoLock)
                                     .Where(step => stepIds.Contains(step.StepId))
                                     .ToListAsync();

            // 申请人上级部门列表（包含申请人所在部门）
            var applyParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                           .With(SqlWith.NoLock)
                                           .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

            // 仅查询流程中实际出现的指派类型
            var assignStepIds = stepInfos.Where(step => step.IsStartStep != 1)
                                         .GroupBy(step => step.Assignment)
                                         .ToDictionary(group => group.Key, group => group.Select(step => step.StepId).ToList());

            var orgStepIds = AssignedStepIds(assignStepIds, Assignment.Org);
            var deptUserStepIds = AssignedStepIds(assignStepIds, Assignment.DeptUser);
            var userStepIds = AssignedStepIds(assignStepIds, Assignment.User);
            var customStepIds = AssignedStepIds(assignStepIds, Assignment.Custom);

            var orgConfigs = orgStepIds.Count == 0
                ? new List<WorkflowStepOrgEntity>()
                : await _db.Queryable<WorkflowStepOrgEntity>()
                           .With(SqlWith.NoLock)
                           .Where(steporg => orgStepIds.Contains(steporg.StepId))
                           .ToListAsync();

            var deptUserConfigs = deptUserStepIds.Count == 0
                ? new List<WorkflowStepDeptUserEntity>()
                : await _db.Queryable<WorkflowStepDeptUserEntity>()
                           .With(SqlWith.NoLock)
                           .Where(stepdeptuser => deptUserStepIds.Contains(stepdeptuser.StepId))
                           .ToListAsync();

            var userConfigs = userStepIds.Count == 0
                ? new List<WorkflowStepUserEntity>()
                : await _db.Queryable<WorkflowStepUserEntity>()
                           .With(SqlWith.NoLock)
                           .Where(stepuser => userStepIds.Contains(stepuser.StepId))
                           .ToListAsync();

            var customConfigs = customStepIds.Count == 0
                ? new List<WorkflowStepCustomEntity>()
                : await _db.Queryable<WorkflowStepCustomEntity>()
                           .With(SqlWith.NoLock)
                           .Where(stepcustom => customStepIds.Contains(stepcustom.StepId))
                           .ToListAsync();

            // 指定人步骤需先取本人档案，才能得到其部门与职级
            var configUserIds = userConfigs.Select(config => config.UserId).Distinct().ToList();
            var users = configUserIds.Count == 0
                ? new List<UserInfoEntity>()
                : await _db.Queryable<UserInfoEntity>()
                           .With(SqlWith.NoLock)
                           .Where(user => configUserIds.Contains(user.UserId))
                           .ToListAsync();

            var deptIds = deptUserConfigs.Select(config => config.DepartmentId)
                                         .Concat(users.Select(user => user.DepartmentId))
                                         .Distinct()
                                         .ToList();
            var depts = deptIds.Count == 0
                ? new List<DepartmentInfoEntity>()
                : await _db.Queryable<DepartmentInfoEntity>()
                           .With(SqlWith.NoLock)
                           .Where(dept => deptIds.Contains(dept.DepartmentId))
                           .ToListAsync();

            var deptLevels = await _db.Queryable<DepartmentLevelEntity>()
                                      .With(SqlWith.NoLock)
                                      .ToListAsync();
            var positions = await _db.Queryable<PositionInfoEntity>()
                                     .With(SqlWith.NoLock)
                                     .ToListAsync();

            return new FlowContext
            {
                StepInfoMap = stepInfos.ToDictionary(step => step.StepId),
                NextStepMap = ruleSteps.ToDictionary(rule => rule.CurrentStepId, rule => rule.NextStepId),
                RuleStepSortMap = ruleSteps.ToDictionary(rule => rule.CurrentStepId, rule => rule.SortOrder),
                FirstStepId = ruleSteps.FirstOrDefault(rule => rule.SortOrder == 1)?.CurrentStepId,
                ApplyParentDept = applyParentDept,
                OrgConfigMap = orgConfigs.ToDictionary(config => config.StepId),
                DeptUserConfigMap = deptUserConfigs.ToDictionary(config => config.StepId),
                UserConfigMap = userConfigs.ToDictionary(config => config.StepId),
                CustomConfigMap = customConfigs.ToDictionary(config => config.StepId),
                DeptMap = depts.ToDictionary(dept => dept.DepartmentId),
                UserMap = users.ToDictionary(user => user.UserId),
                DeptLevelSortMap = deptLevels.ToDictionary(level => level.DepartmentLevelId, level => level.SortOrder),
                PositionSortMap = positions.ToDictionary(position => position.PositionId, position => position.SortOrder),
            };
        }

        private static List<long> AssignedStepIds(Dictionary<string, List<long>> assignStepIds, Assignment assignment)
        {
            return assignStepIds.TryGetValue(assignment.ToEnumString(), out var stepIds) ? stepIds : new List<long>();
        }

        /// <summary>
        /// 沿规则步骤链构建各步骤的审批人列表
        /// </summary>
        private async Task<List<StepFlowItem>> BuildStepReviewList(ApplyFormDetail formDetail, FlowContext context)
        {
            bool isChinese = _lang.Locale == "zh-CN";
            var result = new List<StepFlowItem>();
            var visited = new HashSet<long>();

            long? currentStepId = context.FirstStepId;
            while (currentStepId.HasValue && visited.Add(currentStepId.Value))
            {
                if (!context.StepInfoMap.TryGetValue(currentStepId.Value, out var stepInfo))
                {
                    break;
                }

                var stepReview = new StepReview
                {
                    StepId = stepInfo.StepId,
                    StepName = isChinese ? stepInfo.StepNameCn : stepInfo.StepNameEn,
                };

                var userReview = await ResolveStepReviewUsers(formDetail, context, stepInfo, stepReview);
                stepReview.StepReviewUser.AddRange(userReview);

                result.Add(new StepFlowItem(stepReview, stepInfo.SortOrder, stepInfo.IsStartStep));

                currentStepId = context.NextStepMap.TryGetValue(currentStepId.Value, out var nextStepId) ? nextStepId : null;
            }

            return result;
        }

        /// <summary>
        /// 按步骤指派类型解析审批人；组织架构级别不足或自定义找不到人时标记步骤跳过
        /// </summary>
        private async Task<List<UserReview>> ResolveStepReviewUsers(ApplyFormDetail formDetail, FlowContext context, WorkflowStepEntity stepInfo, StepReview stepReview)
        {
            if (stepInfo.IsStartStep == 1)
            {
                return await GetStartReviewUser(formDetail.UserId);
            }

            bool isSingle = stepInfo.ReviewMode == ReviewMode.Review.ToEnumString();

            if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                if (!context.OrgConfigMap.TryGetValue(stepInfo.StepId, out var orgInfo))
                {
                    return new List<UserReview>();
                }

                int deptLevelSort = context.DeptLevelSort(orgInfo.DeptLeaveId);
                int positionSort = context.PositionSort(orgInfo.PositionId);

                if (formDetail.DeptLevelSort < deptLevelSort || formDetail.PositionSort <= positionSort)
                {
                    stepReview.Skip = 1;
                    return new List<UserReview>();
                }

                return await GetOrgReviewUser(context.ApplyParentDept, deptLevelSort, positionSort, isSingle);
            }

            if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                if (!context.DeptUserConfigMap.TryGetValue(stepInfo.StepId, out var deptUserInfo)
                    || !context.DeptMap.TryGetValue(deptUserInfo.DepartmentId, out var targetDept))
                {
                    return new List<UserReview>();
                }

                return await GetDeptUserReviewUser(deptUserInfo.DepartmentId,
                                                   context.PositionSort(deptUserInfo.PositionId),
                                                   context.DeptLevelSort(targetDept.DepartmentLevelId),
                                                   isSingle);
            }

            if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                if (!context.UserConfigMap.TryGetValue(stepInfo.StepId, out var userInfo)
                    || !context.UserMap.TryGetValue(userInfo.UserId, out var targetUser)
                    || !context.DeptMap.TryGetValue(targetUser.DepartmentId, out var targetDept))
                {
                    return new List<UserReview>();
                }

                return await GetUserReviewUser(targetUser.UserId,
                                               targetDept.DepartmentId,
                                               context.PositionSort(targetUser.PositionId),
                                               context.DeptLevelSort(targetDept.DepartmentLevelId),
                                               isSingle);
            }

            if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                if (!context.CustomConfigMap.TryGetValue(stepInfo.StepId, out var customInfo))
                {
                    stepReview.Skip = 1;
                    return new List<UserReview>();
                }

                var userReview = await GetCustomReviewUser(formDetail.FormId, customInfo.Guidance, context, isSingle);
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
        /// 查询审批人身份 - 按组织架构（降级沿申请人上级部门链查找）
        /// </summary>
        private async Task<List<UserReview>> GetOrgReviewUser(List<DepartmentInfoEntity> applyParentDept, int deptLevelSort, int positionSort, bool isSingle)
        {
            string parentDeptIds = JoinDeptIds(applyParentDept.Select(dept => dept.DepartmentId));

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.Org, parentDeptIds, isSingle,
                new SugarParameter("@DeptLevelSort", deptLevelSort),
                new SugarParameter("@PositionSort", positionSort));

            if (exactResult.Count > 0)
            {
                return exactResult;
            }

            return await FindDowngradeReviewUsers(parentDeptIds, positionSort, deptLevelSort, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按指定部门职级
        /// </summary>
        private async Task<List<UserReview>> GetDeptUserReviewUser(long departmentId, int positionSort, int deptLevelSort, bool isSingle)
        {
            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.Dept, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@DepartmentId", departmentId),
                new SugarParameter("@PositionSort", positionSort));

            if (exactResult.Count > 0)
            {
                return exactResult;
            }

            return await FindDowngradeFromDept(departmentId, positionSort, deptLevelSort, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按指定人
        /// </summary>
        private async Task<List<UserReview>> GetUserReviewUser(long userId, long departmentId, int positionSort, int deptLevelSort, bool isSingle)
        {
            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.User, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@UserId", userId));

            if (exactResult.Count > 0)
            {
                return exactResult;
            }

            return await FindDowngradeFromDept(departmentId, positionSort, deptLevelSort, isSingle);
        }

        /// <summary>
        /// 查询审批人身份 - 按自定义
        /// </summary>
        private async Task<List<UserReview>> GetCustomReviewUser(long formId, string guidance, FlowContext context, bool isSingle)
        {
            var custom = await _personResolver.Resolve(guidance, formId);

            if (custom == null)
            {
                return new List<UserReview>();
            }

            var exactResult = await QueryExactReviewUsers(ReviewUserFilter.User, parentDeptIds: string.Empty, isSingle,
                new SugarParameter("@UserId", custom.UserId));

            if (exactResult.Count > 0)
            {
                return exactResult;
            }

            return await FindDowngradeFromDept(custom.DepartmentId,
                                               context.PositionSort(custom.PositionId),
                                               context.DeptLevelSort(custom.DepartmentLevelId),
                                               isSingle);
        }

        /// <summary>
        /// 降级沿目标部门的上级部门链查找
        /// </summary>
        private async Task<List<UserReview>> FindDowngradeFromDept(long departmentId, int positionSort, int deptLevelSort, bool isSingle)
        {
            var targetParentDept = await _db.Queryable<DepartmentInfoEntity>()
                                            .With(SqlWith.NoLock)
                                            .ToParentListAsync(parent => parent.ParentId, departmentId);

            return await FindDowngradeReviewUsers(JoinDeptIds(targetParentDept.Select(parent => parent.DepartmentId)),
                                                  positionSort, deptLevelSort, isSingle);
        }

        private static string JoinDeptIds(IEnumerable<long> deptIds) => string.Join(",", deptIds);

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
        /// 自动降级查询审批人：职级自高向低、部门级别自内向外取第一个有人的组合。
        /// 由数据库一次排名取回，无需逐组合往返
        /// </summary>
        private async Task<List<UserReview>> FindDowngradeReviewUsers(string parentDeptIds, int fromPositionSort, int fromDeptLevelSort, bool isSingle)
        {
            // 降级从低于当前职级一级开始；无可降级范围或无部门链时直接结束
            int maxPositionSort = fromPositionSort - 1;
            if (maxPositionSort < 1 || fromDeptLevelSort < 1 || string.IsNullOrEmpty(parentDeptIds))
            {
                return new List<UserReview>();
            }

            var (_, _, _, _, autoActual, autoAgent, autoConcurrent, autoConcurrentAgent) = ReviewUserSql.AppointmentEnumStrings();

            string sql = ReviewUserSql.AutoRankedSql(
                Projection,
                parentDeptIds,
                topN: isSingle ? "TOP 1" : "",
                orderBy: ReviewUserSql.BuildOrderBy(isSingle, isAuto: true));

            var result = await _db.Ado.SqlQueryAsync<UserReview>(sql, new[]
            {
                new SugarParameter("@Now", DateTime.Now),
                new SugarParameter("@MaxPositionSort", maxPositionSort),
                new SugarParameter("@MaxDeptLevelSort", fromDeptLevelSort),
                new SugarParameter("@AutoActual", autoActual),
                new SugarParameter("@AutoAgent", autoAgent),
                new SugarParameter("@AutoConcurrent", autoConcurrent),
                new SugarParameter("@AutoConcurrentAgent", autoConcurrentAgent),
            });

            return result ?? new List<UserReview>();
        }

        #endregion

        #region 查询审批结果

        /// <summary>
        /// 按审批记录填充各步骤人员的审批状态（纯内存计算，记录与步骤排序由调用方一次取回）
        /// </summary>
        /// <param name="currentStepId">表单当前所在步骤</param>
        /// <param name="stepOrderMap">规则内步骤排序号 (StepId -> SortOrder)</param>
        /// <param name="reviewFlow">待填充的步骤列表</param>
        /// <param name="reviewRecords">该表单的全部审批记录（含已失效记录）</param>
        private static void FillUserReviewResult(long? currentStepId, Dictionary<long, int> stepOrderMap, List<StepReview> reviewFlow, List<FormReviewRecordEntity> reviewRecords)
        {
            // 步骤状态只认有效记录；驳回记录取最近优先，核准记录按步骤分组便于逐步骤判断
            var validRecords = reviewRecords.Where(record => record.RecordStatus == 1).ToList();

            var rejectRecords = validRecords
                                .Where(record => record.ReviewResult == ReviewResult.Reject.ToEnumString())
                                .OrderByDescending(record => record.ReviewDateTime)
                                .ToList();

            var approvesByStep = validRecords
                                 .Where(record => record.ReviewResult == ReviewResult.Approve.ToEnumString())
                                 .ToLookup(record => record.StepId);

            foreach (var flow in reviewFlow)
            {
                if (flow.Skip == 1)
                {
                    continue;
                }

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

                // 该步骤只要有人在有效时间后核准过，就认为该步骤已核准
                bool stepHasApprove = approvesByStep[flow.StepId]
                                      .Any(record => validAfter == null || record.ReviewDateTime > validAfter.Value);

                string result = stepHasApprove
                    ? ReviewStatus.Approve.ToEnumString()
                    : isCurrentStep
                        ? ReviewStatus.UnderReview.ToEnumString()
                        : ReviewStatus.Unsigned.ToEnumString();

                foreach (var user in flow.StepReviewUser)
                {
                    user.Result = result;
                }
            }
        }

        #endregion
    }
}
