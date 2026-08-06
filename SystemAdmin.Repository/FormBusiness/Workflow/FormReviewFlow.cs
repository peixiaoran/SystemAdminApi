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

        private ReviewUserProjection Projection => ReviewUserProjection.Named(_lang.Locale == "zh-CN");

        #region 查询表单审批流程

        /// <summary>
        /// 查询表单审批流程
        /// </summary>
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
        public async Task<List<RejectStepDrop>> GetRejectStepDrop(long formId)
        {
            var formDetail = await GetApplyFormDetail(formId);
            if (formDetail == null)
            {
                return new List<RejectStepDrop>();
            }

            // 先取当前步骤资料：起始步骤无可驳回步骤，其排序同时作为后续只解析前置步骤的上限
            int currentSortOrder = int.MaxValue;
            if (formDetail.CurrentStepId.HasValue)
            {
                long currentStepId = formDetail.CurrentStepId.Value;
                var currentStep = await _db.Queryable<WorkflowStepEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(step => step.StepId == currentStepId)
                                           .FirstAsync();

                if (currentStep?.IsStartStep == 1)
                {
                    return new List<RejectStepDrop>();
                }

                currentSortOrder = currentStep?.SortOrder ?? int.MaxValue;
            }

            // 下拉只显示当前步骤之前的步骤，其后步骤不预载指派配置、也不查审批人
            var context = await BuildFlowContext(formDetail, currentSortOrder);
            var flowSteps = await BuildStepReviewList(formDetail, context, currentSortOrder);

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
        /// 预载步骤链、步骤配置与组织架构资料：配置与部门/人员按 Id 集合批量取回，
        /// 部门级别与职级为小型基础资料整表缓存，使步骤循环内不再往返数据库。
        /// maxSortOrder 用于只需要部分步骤的场景（如可驳回下拉），限定后不再预载其余步骤的指派配置
        /// </summary>
        private async Task<FlowContext> BuildFlowContext(ApplyFormDetail formDetail, int? maxSortOrder = null)
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

            // 仅查询需要解析审批人的步骤上实际出现的指派类型
            var assignStepIds = stepInfos.Where(step => step.IsStartStep != 1 && NeedResolveStep(step, maxSortOrder))
                                         .GroupBy(step => step.Assignment)
                                         .ToDictionary(group => group.Key, group => group.Select(step => step.StepId).ToList());

            var orgStepIds = AssignedStepIds(assignStepIds, Assignment.Org);
            var deptUserStepIds = AssignedStepIds(assignStepIds, Assignment.DeptUser);
            var userStepIds = AssignedStepIds(assignStepIds, Assignment.User);
            var customStepIds = AssignedStepIds(assignStepIds, Assignment.Custom);
            var addReviewStepIds = AssignedStepIds(assignStepIds, Assignment.AddReview);

            // 申请人上级部门列表（包含申请人所在部门），仅组织架构指派步骤会用到
            var applyParentDept = orgStepIds.Count == 0
                ? new List<DepartmentInfoEntity>()
                : await _db.Queryable<DepartmentInfoEntity>()
                           .With(SqlWith.NoLock)
                           .ToParentListAsync(dept => dept.ParentId, formDetail.DeptId);

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

            // 步骤规则给出加审顺序，实际人员来自表单上的加审设定
            var addReviewConfigs = addReviewStepIds.Count == 0
                ? new List<WorkflowStepAddReviewEntity>()
                : await _db.Queryable<WorkflowStepAddReviewEntity>()
                           .With(SqlWith.NoLock)
                           .Where(stepaddreview => addReviewStepIds.Contains(stepaddreview.StepId))
                           .ToListAsync();

            var formAddReviews = addReviewConfigs.Count == 0
                ? new List<FormAddReviewEntity>()
                : await _db.Queryable<FormAddReviewEntity>()
                           .With(SqlWith.NoLock)
                           .Where(addreview => addreview.FormId == formDetail.FormId)
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
                AddReviewConfigMap = addReviewConfigs.ToDictionary(config => config.StepId),
                FormAddReviewMap = formAddReviews.GroupBy(addreview => addreview.SortOrder)
                                                 .ToDictionary(group => group.Key,
                                                               group => group.Select(addreview => addreview.UserId).Distinct().ToList()),
            };
        }

        private static List<long> AssignedStepIds(Dictionary<string, List<long>> assignStepIds, Assignment assignment)
        {
            return assignStepIds.TryGetValue(assignment.ToEnumString(), out var stepIds) ? stepIds : new List<long>();
        }

        /// <summary>
        /// 判断步骤是否需要解析审批人：未限定排序上限时全部解析；
        /// 限定时只解析起始步骤与排序在上限之前的步骤
        /// </summary>
        private static bool NeedResolveStep(WorkflowStepEntity stepInfo, int? maxSortOrder)
        {
            return maxSortOrder == null || stepInfo.IsStartStep == 1 || stepInfo.SortOrder < maxSortOrder.Value;
        }

        /// <summary>
        /// 沿规则步骤链构建各步骤的审批人列表；
        /// maxSortOrder 限定时，排序在其之后的步骤直接略过，不再查询审批人
        /// </summary>
        private async Task<List<StepFlowItem>> BuildStepReviewList(ApplyFormDetail formDetail, FlowContext context, int? maxSortOrder = null)
        {
            var requests = await CollectStepUserRequests(formDetail, context, maxSortOrder);
            await FillStepReviewUsers(formDetail, context, requests);

            return requests.Select(request => request.Item).ToList();
        }

        /// <summary>
        /// 沿步骤链收集各步骤的取人条件（此阶段不查审批人）
        /// </summary>
        private async Task<List<StepUserRequest>> CollectStepUserRequests(ApplyFormDetail formDetail, FlowContext context, int? maxSortOrder)
        {
            bool isChinese = _lang.Locale == "zh-CN";
            var requests = new List<StepUserRequest>();
            var visited = new HashSet<long>();

            long? currentStepId = context.FirstStepId;
            while (currentStepId.HasValue && visited.Add(currentStepId.Value))
            {
                if (!context.StepInfoMap.TryGetValue(currentStepId.Value, out var stepInfo))
                {
                    break;
                }

                if (NeedResolveStep(stepInfo, maxSortOrder))
                {
                    var stepReview = new StepReview
                    {
                        StepId = stepInfo.StepId,
                        StepName = isChinese ? stepInfo.StepNameCn : stepInfo.StepNameEn,
                    };

                    var request = await BuildStepUserRequest(formDetail, context, stepInfo, stepReview);
                    request.Item = new StepFlowItem
                    {
                        Review = stepReview,
                        SortOrder = stepInfo.SortOrder,
                        IsStartStep = stepInfo.IsStartStep,
                    };

                    requests.Add(request);
                }

                currentStepId = context.NextStepMap.TryGetValue(currentStepId.Value, out var nextStepId) ? nextStepId : null;
            }

            return requests;
        }

        /// <summary>
        /// 按步骤指派类型确定取人条件；组织架构级别不足或指派配置缺失时直接标记步骤跳过
        /// </summary>
        private async Task<StepUserRequest> BuildStepUserRequest(ApplyFormDetail formDetail, FlowContext context, WorkflowStepEntity stepInfo, StepReview stepReview)
        {
            var request = new StepUserRequest();

            if (stepInfo.IsStartStep == 1)
            {
                request.IsStart = true;
                return request;
            }

            request.IsReview = stepInfo.ReviewMode == ReviewMode.Review.ToEnumString();

            if (stepInfo.Assignment == Assignment.Org.ToEnumString())
            {
                if (!context.OrgConfigMap.TryGetValue(stepInfo.StepId, out var orgInfo))
                {
                    return request;
                }

                int deptLevelSort = context.DeptLevelSort(orgInfo.DeptLeaveId);
                int positionSort = context.PositionSort(orgInfo.PositionId);

                if (formDetail.DeptLevelSort < deptLevelSort || formDetail.PositionSort <= positionSort)
                {
                    stepReview.Skip = 1;
                    return request;
                }

                request.Filter = ReviewUserFilter.Org;
                request.DeptLevelSort = deptLevelSort;
                request.PositionSort = positionSort;
                request.DowngradeFromApplicant = true;
                return request;
            }

            if (stepInfo.Assignment == Assignment.DeptUser.ToEnumString())
            {
                if (!context.DeptUserConfigMap.TryGetValue(stepInfo.StepId, out var deptUserInfo)
                    || !context.DeptMap.TryGetValue(deptUserInfo.DepartmentId, out var targetDept))
                {
                    return request;
                }

                request.Filter = ReviewUserFilter.Dept;
                request.DepartmentId = deptUserInfo.DepartmentId;
                request.PositionSort = context.PositionSort(deptUserInfo.PositionId);
                request.DeptLevelSort = context.DeptLevelSort(targetDept.DepartmentLevelId);
                return request;
            }

            if (stepInfo.Assignment == Assignment.User.ToEnumString())
            {
                if (!context.UserConfigMap.TryGetValue(stepInfo.StepId, out var userInfo)
                    || !context.UserMap.TryGetValue(userInfo.UserId, out var targetUser)
                    || !context.DeptMap.TryGetValue(targetUser.DepartmentId, out var targetDept))
                {
                    return request;
                }

                request.Filter = ReviewUserFilter.User;
                request.UserIds.Add(targetUser.UserId);
                request.DepartmentId = targetDept.DepartmentId;
                request.PositionSort = context.PositionSort(targetUser.PositionId);
                request.DeptLevelSort = context.DeptLevelSort(targetDept.DepartmentLevelId);
                return request;
            }

            if (stepInfo.Assignment == Assignment.Custom.ToEnumString())
            {
                if (!context.CustomConfigMap.TryGetValue(stepInfo.StepId, out var customInfo))
                {
                    stepReview.Skip = 1;
                    return request;
                }

                request.SkipWhenEmpty = true;

                var custom = await _personResolver.Resolve(customInfo.Guidance, formDetail.FormId);
                if (custom == null)
                {
                    return request;
                }

                request.Filter = ReviewUserFilter.User;
                request.UserIds.Add(custom.UserId);
                request.DepartmentId = custom.DepartmentId;
                request.PositionSort = context.PositionSort(custom.PositionId);
                request.DeptLevelSort = context.DeptLevelSort(custom.DepartmentLevelId);
                return request;
            }

            if (stepInfo.Assignment == Assignment.AddReview.ToEnumString())
            {
                if (!context.AddReviewConfigMap.TryGetValue(stepInfo.StepId, out var addReviewInfo))
                {
                    stepReview.Skip = 1;
                    return request;
                }

                // 加审是点名的人，查不到即略过，不做自动降级；身份优先级取最高一笔，避免专兼职并存时重复
                request.SkipWhenEmpty = true;
                request.AllowDowngrade = false;
                request.IsReview = true;
                request.RequireReviewAuth = false;
                request.Filter = ReviewUserFilter.User;
                request.UserIds.AddRange(context.AddReviewUserIds(addReviewInfo.SortOrder));
                return request;
            }

            return request;
        }

        #endregion

        #region 批量取回步骤审批人员

        /// <summary>
        /// 取回并回填各步骤审批人：按指派类型分批一次查回，精确匹配落空的步骤再兜底降级
        /// </summary>
        private async Task FillStepReviewUsers(ApplyFormDetail formDetail, FlowContext context, List<StepUserRequest> requests)
        {
            await FillStartRequests(formDetail, requests);
            await FillOrgRequests(context, requests);
            await FillDeptRequests(requests);
            await FillUserRequests(requests);
            await FillDowngradeRequests(context, requests);

            // 自定义 / 加审步骤查不到人即跳过
            foreach (var request in requests.Where(request => request.SkipWhenEmpty && request.Item.Review.StepReviewUser.Count == 0))
            {
                request.Item.Review.Skip = 1;
            }
        }

        /// <summary>
        /// 起始步骤：全流程共用申请人一次查询结果
        /// </summary>
        private async Task FillStartRequests(ApplyFormDetail formDetail, List<StepUserRequest> requests)
        {
            var startRequests = requests.Where(request => request.IsStart).ToList();
            if (startRequests.Count == 0)
            {
                return;
            }

            var startUsers = await GetStartReviewUser(formDetail.UserId);

            foreach (var request in startRequests)
            {
                AppendReviewUsers(request, startUsers);
            }
        }

        /// <summary>
        /// 组织架构指派：上级部门链全流程共用，仅 (部门级别, 职级) 组合因步骤而异，一次取回
        /// </summary>
        private async Task FillOrgRequests(FlowContext context, List<StepUserRequest> requests)
        {
            var orgRequests = requests.Where(request => request.Filter == ReviewUserFilter.Org).ToList();
            if (orgRequests.Count == 0)
            {
                return;
            }

            string parentDeptIds = JoinDeptIds(context.ApplyParentDept.Select(dept => dept.DepartmentId));
            if (string.IsNullOrEmpty(parentDeptIds))
            {
                return;
            }

            // 单审只取一笔、会审取全部，两种语义分批查询
            foreach (var group in orgRequests.GroupBy(request => request.IsReview))
            {
                var comboKeys = BuildComboKeys(group, request => (request.DeptLevelSort, request.PositionSort));
                string comboValues = string.Join(",", comboKeys.Select(combo => $"({combo.Value},{combo.Key.Item1},{combo.Key.Item2})"));

                var batch = await QueryExactReviewUsersBatch(ReviewUserFilter.Org, parentDeptIds, group.Key, comboValues);

                foreach (var request in group)
                {
                    AppendReviewUsers(request, batch, comboKeys[(request.DeptLevelSort, request.PositionSort)]);
                }
            }
        }

        /// <summary>
        /// 指定部门职级指派：(部门, 职级) 组合一次取回
        /// </summary>
        private async Task FillDeptRequests(List<StepUserRequest> requests)
        {
            var deptRequests = requests.Where(request => request.Filter == ReviewUserFilter.Dept).ToList();
            if (deptRequests.Count == 0)
            {
                return;
            }

            foreach (var group in deptRequests.GroupBy(request => request.IsReview))
            {
                var comboKeys = BuildComboKeys(group, request => (request.DepartmentId, request.PositionSort));
                string comboValues = string.Join(",", comboKeys.Select(combo => $"({combo.Value},{AsBigInt(combo.Key.Item1)},{combo.Key.Item2})"));

                var batch = await QueryExactReviewUsersBatch(ReviewUserFilter.Dept, parentDeptIds: string.Empty, group.Key, comboValues);

                foreach (var request in group)
                {
                    AppendReviewUsers(request, batch, comboKeys[(request.DepartmentId, request.PositionSort)]);
                }
            }
        }

        /// <summary>
        /// 指定人指派（含自定义、加审）：所有点名人员一次取回
        /// </summary>
        private async Task FillUserRequests(List<StepUserRequest> requests)
        {
            var userRequests = requests.Where(request => request.Filter == ReviewUserFilter.User && request.UserIds.Count > 0).ToList();
            if (userRequests.Count == 0)
            {
                return;
            }

            // 加审不校验审批权限，与其余指定人步骤条件不同，分批查询
            foreach (var group in userRequests.GroupBy(request => (request.IsReview, request.RequireReviewAuth)))
            {
                var userIds = group.SelectMany(request => request.UserIds).Distinct().ToList();
                var comboKeys = userIds.Select((userId, index) => (userId, index))
                                       .ToDictionary(pair => pair.userId, pair => pair.index);
                string comboValues = string.Join(",", comboKeys.Select(combo => $"({combo.Value},{AsBigInt(combo.Key)})"));

                var batch = await QueryExactReviewUsersBatch(ReviewUserFilter.User, parentDeptIds: string.Empty, group.Key.IsReview, comboValues, group.Key.RequireReviewAuth);

                foreach (var request in group)
                {
                    // 加审步骤有多人，按配置顺序回填
                    foreach (long userId in request.UserIds)
                    {
                        AppendReviewUsers(request, batch, comboKeys[userId]);
                    }
                }
            }
        }

        /// <summary>
        /// 精确匹配落空的步骤逐一降级兜底：部门链与相同降级条件只查一次
        /// </summary>
        private async Task FillDowngradeRequests(FlowContext context, List<StepUserRequest> requests)
        {
            var pending = requests.Where(request => request.Filter.HasValue
                                                    && request.AllowDowngrade
                                                    && request.Item.Review.StepReviewUser.Count == 0)
                                  .ToList();
            if (pending.Count == 0)
            {
                return;
            }

            string applicantDeptIds = JoinDeptIds(context.ApplyParentDept.Select(dept => dept.DepartmentId));
            var deptChainCache = new Dictionary<long, string>();
            var downgradeCache = new Dictionary<string, List<UserReview>>();

            foreach (var request in pending)
            {
                string parentDeptIds = request.DowngradeFromApplicant
                    ? applicantDeptIds
                    : await GetParentDeptIds(request.DepartmentId, deptChainCache);

                string cacheKey = $"{parentDeptIds}|{request.PositionSort}|{request.DeptLevelSort}|{request.IsReview}";
                if (!downgradeCache.TryGetValue(cacheKey, out var downgradeUsers))
                {
                    downgradeUsers = await FindDowngradeReviewUsers(parentDeptIds, request.PositionSort, request.DeptLevelSort, request.IsReview);
                    downgradeCache[cacheKey] = downgradeUsers;
                }

                AppendReviewUsers(request, downgradeUsers);
            }
        }

        /// <summary>
        /// 为去重后的条件组合编号，编号即 SQL 中带回的 ComboKey
        /// </summary>
        private static Dictionary<TCombo, int> BuildComboKeys<TCombo>(IEnumerable<StepUserRequest> requests, Func<StepUserRequest, TCombo> selector) where TCombo : notnull
        {
            return requests.Select(selector)
                           .Distinct()
                           .Select((combo, index) => (combo, index))
                           .ToDictionary(pair => pair.combo, pair => pair.index);
        }

        private static void AppendReviewUsers(StepUserRequest request, Dictionary<int, List<UserReview>> batch, int comboKey)
        {
            if (batch.TryGetValue(comboKey, out var userReview))
            {
                AppendReviewUsers(request, userReview);
            }
        }

        /// <summary>
        /// 同一批查询结果可能落在多个步骤上，回填时复制一份，避免步骤间审批状态互相覆盖
        /// </summary>
        private static void AppendReviewUsers(StepUserRequest request, List<UserReview> userReview)
        {
            request.Item.Review.StepReviewUser.AddRange(userReview.Select(user => new UserReview
            {
                ReviewUserId = user.ReviewUserId,
                ReviewUserName = user.ReviewUserName,
                AgentUserId = user.AgentUserId,
                AgentUserName = user.AgentUserName,
                AppointmentType = user.AppointmentType,
                AppointmentTypeName = user.AppointmentTypeName,
                Result = user.Result,
            }));
        }

        /// <summary>
        /// 取目标部门的上级部门链（含自身），同一部门只查一次
        /// </summary>
        private async Task<string> GetParentDeptIds(long departmentId, Dictionary<long, string> cache)
        {
            if (cache.TryGetValue(departmentId, out var cached))
            {
                return cached;
            }

            var parentDept = await _db.Queryable<DepartmentInfoEntity>()
                                      .With(SqlWith.NoLock)
                                      .ToParentListAsync(parent => parent.ParentId, departmentId);

            string parentDeptIds = JoinDeptIds(parentDept.Select(parent => parent.DepartmentId));
            cache[departmentId] = parentDeptIds;

            return parentDeptIds;
        }

        /// <summary>
        /// 步骤取人条件：批量查询前按步骤收集，查回后回填到对应步骤
        /// </summary>
        private sealed class StepUserRequest
        {
            /// <summary>步骤审批信息（审批人待回填）</summary>
            public StepFlowItem Item { get; set; } = new StepFlowItem();

            /// <summary>是否起始步骤（取申请人本人）</summary>
            public bool IsStart { get; set; }

            /// <summary>取人过滤方式，为空表示该步骤无需查询</summary>
            public ReviewUserFilter? Filter { get; set; }

            /// <summary>是否单审（只取身份优先级最高的一笔）</summary>
            public bool IsReview { get; set; }

            /// <summary>是否校验审批权限（加审为点名指派，无审批权限者亦可审批）</summary>
            public bool RequireReviewAuth { get; set; } = true;

            /// <summary>查不到人时是否标记步骤跳过</summary>
            public bool SkipWhenEmpty { get; set; }

            /// <summary>精确匹配落空时是否自动降级</summary>
            public bool AllowDowngrade { get; set; } = true;

            /// <summary>降级沿申请人部门链查找（组织架构指派）</summary>
            public bool DowngradeFromApplicant { get; set; }

            /// <summary>点名的审批人（指定人 / 自定义 / 加审）</summary>
            public List<long> UserIds { get; set; } = new List<long>();

            /// <summary>目标部门（指定部门职级过滤 / 降级起点）</summary>
            public long DepartmentId { get; set; }

            /// <summary>目标职级排序（过滤 / 降级起点）</summary>
            public int PositionSort { get; set; }

            /// <summary>目标部门级别排序（过滤 / 降级起点）</summary>
            public int DeptLevelSort { get; set; }
        }

        #endregion

        #region 查询各指派类型审批人员

        /// <summary>
        /// 查询起始步骤审批人员
        /// </summary>
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
        /// 拼接部门Id 列表；雪花 Id 字面量在 T-SQL 中会被当成 numeric，需显式转 bigint 才不致比较列被隐式转换、走不了索引
        /// </summary>
        private static string JoinDeptIds(IEnumerable<long> deptIds) => string.Join(",", deptIds.Select(AsBigInt));

        private static string AsBigInt(long value) => $"CAST({value} AS BIGINT)";

        /// <summary>
        /// 批量精确匹配查询审批人：一次取回多组条件的结果，按 ComboKey 归属回各条件组；
        /// requireReviewAuth 为 false 时不校验审批权限（加审）
        /// </summary>
        private async Task<Dictionary<int, List<UserReview>>> QueryExactReviewUsersBatch(ReviewUserFilter filter, string parentDeptIds, bool isReview, string comboValues, bool requireReviewAuth = true)
        {
            var (actual, agent, concurrent, concurrentAgent, _, _, _, _) = ReviewUserSql.AppointmentEnumStrings();

            string sql = ReviewUserSql.ExactBatchSql(Projection, filter, parentDeptIds, comboValues, isReview, requireReviewAuth);

            var result = await _db.Ado.SqlQueryAsync<BatchUserReview>(sql, new[]
            {
                new SugarParameter("@Now", DateTime.Now),
                new SugarParameter("@Actual", actual),
                new SugarParameter("@Agent", agent),
                new SugarParameter("@Concurrent", concurrent),
                new SugarParameter("@ConcurrentAgent", concurrentAgent),
            });

            return (result ?? new List<BatchUserReview>())
                   .GroupBy(user => user.ComboKey)
                   .ToDictionary(group => group.Key, group => group.Cast<UserReview>().ToList());
        }

        /// <summary>
        /// 自动降级查询审批人：职级自高向低、部门级别自内向外取第一个有人的组合。
        /// 由数据库一次排名取回，无需逐组合往返
        /// </summary>
        private async Task<List<UserReview>> FindDowngradeReviewUsers(string parentDeptIds, int fromPositionSort, int fromDeptLevelSort, bool isReview)
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
                topN: isReview ? "TOP 1" : "",
                orderBy: ReviewUserSql.BuildOrderBy(isReview, isAuto: true));

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
        private static void FillUserReviewResult(long? currentStepId, Dictionary<long, int> stepOrderMap, List<StepReview> reviewFlow, List<FormReviewRecordEntity> reviewRecords)
        {
            // 步骤状态只认有效记录；驳回取最近优先，核准按步骤分组便于逐步骤判断
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
