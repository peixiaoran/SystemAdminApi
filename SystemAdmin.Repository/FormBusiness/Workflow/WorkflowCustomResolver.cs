using SqlSugar;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤自定义😇
    /// </summary>
    public class WorkflowCustomResolver
    {
        private readonly SqlSugarScope _db;

        // guidance(方法名) -> 实际方法
        private readonly Dictionary<string, Func<long, Task<CustomUser>>> _registry;

        public WorkflowCustomResolver(SqlSugarScope db)
        {
            _db = db;

            // 登记所有自定义取人方法，新增方法只需在这里加一行
            _registry = new Dictionary<string, Func<long, Task<CustomUser>>>(StringComparer.OrdinalIgnoreCase)
            {
                // 出差单
                [nameof(DepartureSiteManager)] = DepartureSiteManager,
                [nameof(DestinationSiteManager)] = DestinationSiteManager,
                [nameof(DepartureSiteHRManager)] = DepartureSiteHRManager,
                [nameof(DestinationSiteHRManager)] = DestinationSiteHRManager,
                // 资讯需求单
                [nameof(EstimatedTimeHandler)] = EstimatedTimeHandler,
                [nameof(HandlerConfirmation)] = HandlerConfirmation,
                [nameof(ApplicationConfirmation)] = ApplicationConfirmation,
            };
        }

        /// <summary>
        /// 按 guidance(配置的方法名) 分发到对应的自定义取人方法。
        /// 解析结果定位到「部门 + 职级」这一角色，或直接点名人员（UserIds）；具体由谁审批（实职 / 兼任 / 代理，
        /// 以及部门职级精确匹配落空后的降级兜底）统一交给 FormReviewFlow / FormReviewAction
        /// 既有的取人逻辑处理，与其他指派方式共用同一套身份优先级
        /// </summary>
        public async Task<CustomUser> Resolve(string guidance, long formId)
        {
            if (string.IsNullOrWhiteSpace(guidance) || !_registry.TryGetValue(guidance, out var handler))
                throw new InvalidOperationException($"未配置的取人方法: {guidance}");

            return await handler(formId);
        }

        #region 出差单
        /// <summary>
        /// 出发厂区厂长：根据出差单的出发厂区，定位该厂区下部门级别为厂级(Site)、职级为厂长(S08)的角色
        /// </summary>
        public async Task<CustomUser> DepartureSiteManager(long formId)
        {
            var departureSite = await _db.Queryable<OverseasTripAppEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(trip => trip.FormId == formId)
                                         .Select(trip => trip.DepartureSite)
                                         .FirstAsync();

            return await ResolveSiteManagerRole(departureSite);
        }

        /// <summary>
        /// 目的厂区厂长：根据出差单的目的厂区，定位该厂区下部门级别为厂级(Site)、职级为厂长(S08)的角色
        /// </summary>
        public async Task<CustomUser> DestinationSiteManager(long formId)
        {
            var destinationSite = await _db.Queryable<OverseasTripAppEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(trip => trip.FormId == formId)
                                           .Select(trip => trip.DestinationSite)
                                           .FirstAsync();

            return await ResolveSiteManagerRole(destinationSite);
        }

        /// <summary>
        /// 按厂区定位部门级别为厂级(Site)、职级为厂长(S08)的角色
        /// </summary>
        private async Task<CustomUser> ResolveSiteManagerRole(string? site)
        {
            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<DepartmentLevelEntity>((dept, level) => dept.DepartmentLevelId == level.DepartmentLevelId)
                                .Where((dept, level) => dept.Site == site && level.DepartmentLevelCode == "Site")
                                .Select((dept, level) => dept)
                                .FirstAsync();

            return await ResolveRole(dept, "S08");
        }

        /// <summary>
        /// 出发厂区人资经理：根据出差单的出发厂区，定位该厂区下部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        public async Task<CustomUser> DepartureSiteHRManager(long formId)
        {
            var departureSite = await _db.Queryable<OverseasTripAppEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(trip => trip.FormId == formId)
                                         .Select(trip => trip.DepartureSite)
                                         .FirstAsync();

            return await ResolveSiteHRManagerRole(departureSite);
        }

        /// <summary>
        /// 目的厂区人资经理：根据出差单的目的厂区，定位该厂区下部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        public async Task<CustomUser> DestinationSiteHRManager(long formId)
        {
            var destinationSite = await _db.Queryable<OverseasTripAppEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(trip => trip.FormId == formId)
                                           .Select(trip => trip.DestinationSite)
                                           .FirstAsync();

            return await ResolveSiteHRManagerRole(destinationSite);
        }

        /// <summary>
        /// 按厂区定位部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        private async Task<CustomUser> ResolveSiteHRManagerRole(string? site)
        {
            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.Site == site && dept.DepartmentFunctions == "HumanResources")
                                .FirstAsync();

            return await ResolveRole(dept, "S06");
        }

        /// <summary>
        /// 组装角色结果：部门 + 该部门级别 + 目标职级编码对应的职级Id
        /// </summary>
        private async Task<CustomUser> ResolveRole(DepartmentInfoEntity? dept, string positionNo)
        {
            if (dept == null)
                return null!;

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionNo == positionNo)
                                    .FirstAsync();

            if (position == null)
                return null!;

            return new CustomUser()
            {
                DepartmentId = dept.DepartmentId,
                DepartmentLevelId = dept.DepartmentLevelId,
                PositionId = position.PositionId
            };
        }
        #endregion

        #region 资讯需求单
        /// <summary>
        /// 预估处理时间人员：根据资讯需求单选择的需求类别，取 InforCategoryConfig 配置的负责人
        /// </summary>
        public async Task<CustomUser> EstimatedTimeHandler(long formId)
        {
            return await ResolveCategoryHandler(formId);
        }

        /// <summary>
        /// 处理人确认：根据资讯需求单选择的需求类别，取 InforCategoryConfig 配置的负责人
        /// </summary>
        public async Task<CustomUser> HandlerConfirmation(long formId)
        {
            return await ResolveCategoryHandler(formId);
        }

        /// <summary>
        /// 申请人确认：表单的发起申请人
        /// </summary>
        public async Task<CustomUser> ApplicationConfirmation(long formId)
        {
            var applicantUserId = await _db.Queryable<FormInstanceEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(instance => instance.FormId == formId)
                                           .Select(instance => instance.ApplicantUserId)
                                           .FirstAsync();

            return ResolveUsers(applicantUserId == 0 ? new List<long>() : new List<long> { applicantUserId });
        }

        /// <summary>
        /// 按资讯需求单的需求类别取负责人
        /// </summary>
        private async Task<CustomUser> ResolveCategoryHandler(long formId)
        {
            var handlerIds = await _db.Queryable<InformationRequestEntity>()
                                      .With(SqlWith.NoLock)
                                      .InnerJoin<InforCategoryConfigEntity>((info, config) => info.Category == config.System)
                                      .Where((info, config) => info.FormId == formId)
                                      .OrderBy((info, config) => config.SortOrder)
                                      .Select((info, config) => config.PersonChargeId)
                                      .ToListAsync();

            return ResolveUsers(handlerIds);
        }

        /// <summary>
        /// 组装点名人员结果；无人返回 null（步骤跳过）
        /// </summary>
        private static CustomUser ResolveUsers(List<long> userIds)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0)
                return null!;

            return new CustomUser()
            {
                UserIds = distinctIds
            };
        }
        #endregion
    }
}
