using SqlSugar;
using SystemAdmin.Model.FormBusiness.Forms.InformationRequest.Entity;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity;
using SystemAdmin.Model.FormBusiness.Forms.PublicForm.Entity;
using SystemAdmin.Model.FormBusiness.Forms.SystemAccountRequest.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤自定义
    /// </summary>
    public class WorkflowCustomResolver
    {
        private readonly SqlSugarScope _db;

        // guidance(方法名) -> 实际方法
        private readonly Dictionary<string, Func<long, Task<CustomUser?>>> _registry;

        public WorkflowCustomResolver(SqlSugarScope db)
        {
            _db = db;

            // 新增取人方法在此登记
            _registry = new Dictionary<string, Func<long, Task<CustomUser?>>>(StringComparer.OrdinalIgnoreCase)
            {
                // 出差单
                [nameof(DepartureSiteManager)] = DepartureSiteManager,
                [nameof(DestinationSiteManager)] = DestinationSiteManager,
                [nameof(DepartureSiteHRManager)] = DepartureSiteHRManager,
                [nameof(DestinationSiteHRManager)] = DestinationSiteHRManager,
                // 资讯需求单
                [nameof(InforRequestEstimatedTimeHandler)] = InforRequestEstimatedTimeHandler,
                [nameof(InforRequestPersonInCharge)] = InforRequestPersonInCharge,
                [nameof(InforRequestApplicantRating)] = InforRequestApplicantRating,
                // 系统账号申请单
                [nameof(SystemAccountPersonCharge)] = SystemAccountPersonCharge,
                [nameof(SystemAccountApplicantRating)] = SystemAccountApplicantRating,
            };
        }

        /// <summary>
        /// 按 guidance(方法名) 分发到对应取人方法。结果为「部门 + 职级」角色或点名人员(UserIds)，
        /// 具体由谁审批（身份优先级、降级兜底）交给 FormReviewFlow / FormReviewAction
        /// </summary>
        /// <returns>找不到角色或人员时返回 null，步骤跳过</returns>
        public async Task<CustomUser?> Resolve(string guidance, long formId)
        {
            if (string.IsNullOrWhiteSpace(guidance) || !_registry.TryGetValue(guidance, out var handler))
                throw new InvalidOperationException($"未配置的取人方法: {guidance}");

            return await handler(formId);
        }

        #region 出差单
        /// <summary>
        /// 出发厂区厂长
        /// </summary>
        public async Task<CustomUser?> DepartureSiteManager(long formId)
        {
            var departureSite = await _db.Queryable<OverseasTripAppEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(trip => trip.FormId == formId)
                                         .Select(trip => trip.DepartureSite)
                                         .FirstAsync();

            return await ResolveSiteManagerRole(departureSite);
        }

        /// <summary>
        /// 目的厂区厂长
        /// </summary>
        public async Task<CustomUser?> DestinationSiteManager(long formId)
        {
            var destinationSite = await _db.Queryable<OverseasTripAppEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(trip => trip.FormId == formId)
                                           .Select(trip => trip.DestinationSite)
                                           .FirstAsync();

            return await ResolveSiteManagerRole(destinationSite);
        }

        /// <summary>
        /// 厂长角色：该厂区厂级(Site)部门 + 职级 S08
        /// </summary>
        private async Task<CustomUser?> ResolveSiteManagerRole(string? site)
        {
            if (string.IsNullOrWhiteSpace(site))
            {
                return null;
            }

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<DepartmentLevelEntity>((dept, level) => dept.DepartmentLevelId == level.DepartmentLevelId)
                                .Where((dept, level) => dept.Site == site && level.DepartmentLevelCode == "Site")
                                .Select((dept, level) => dept)
                                .FirstAsync();

            return await ResolveRole(dept, "S08");
        }

        /// <summary>
        /// 出发厂区人资经理
        /// </summary>
        public async Task<CustomUser?> DepartureSiteHRManager(long formId)
        {
            var departureSite = await _db.Queryable<OverseasTripAppEntity>()
                                         .With(SqlWith.NoLock)
                                         .Where(trip => trip.FormId == formId)
                                         .Select(trip => trip.DepartureSite)
                                         .FirstAsync();

            return await ResolveSiteHRManagerRole(departureSite);
        }

        /// <summary>
        /// 目的厂区人资经理
        /// </summary>
        public async Task<CustomUser?> DestinationSiteHRManager(long formId)
        {
            var destinationSite = await _db.Queryable<OverseasTripAppEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(trip => trip.FormId == formId)
                                           .Select(trip => trip.DestinationSite)
                                           .FirstAsync();

            return await ResolveSiteHRManagerRole(destinationSite);
        }

        /// <summary>
        /// 人资经理角色：该厂区人力资源(HumanResources)部门 + 职级 S06
        /// </summary>
        private async Task<CustomUser?> ResolveSiteHRManagerRole(string? site)
        {
            if (string.IsNullOrWhiteSpace(site))
            {
                return null;
            }

            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.Site == site && dept.DepartmentFunctions == "HumanResources")
                                .FirstAsync();

            return await ResolveRole(dept, "S06");
        }

        /// <summary>
        /// 组装角色结果：部门 + 部门级别 + 职级
        /// </summary>
        private async Task<CustomUser?> ResolveRole(DepartmentInfoEntity? dept, string positionNo)
        {
            if (dept == null)
                return null;

            var position = await _db.Queryable<PositionInfoEntity>()
                                    .With(SqlWith.NoLock)
                                    .Where(position => position.PositionNo == positionNo)
                                    .FirstAsync();

            if (position == null)
                return null;

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
        /// 预估处理时间人员：需求类别对应的负责人
        /// </summary>
        public async Task<CustomUser?> InforRequestEstimatedTimeHandler(long formId)
        {
            return await ResolveCategoryHandler(formId);
        }

        /// <summary>
        /// 处理人确认：需求类别对应的负责人
        /// </summary>
        public async Task<CustomUser?> InforRequestPersonInCharge(long formId)
        {
            return await ResolveCategoryHandler(formId);
        }

        /// <summary>
        /// 申请人确认：表单的发起申请人
        /// </summary>
        public async Task<CustomUser?> InforRequestApplicantRating(long formId)
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
        private async Task<CustomUser?> ResolveCategoryHandler(long formId)
        {
            var handlerIds = await _db.Queryable<InformationRequestEntity>()
                                      .With(SqlWith.NoLock)
                                      .InnerJoin<PersonInChargeConfigEntity>((info, config) => info.Category == config.System && config.Type == "InforRequest")
                                      .Where((info, config) => info.FormId == formId)
                                      .OrderBy((info, config) => config.SortOrder)
                                      .Select((info, config) => config.PersonChargeId)
                                      .ToListAsync();

            return ResolveUsers(handlerIds);
        }
        #endregion

        #region 系统账号申请单
        /// <summary>
        /// 系统负责人：明细所选系统对应的负责人
        /// </summary>
        public async Task<CustomUser?> SystemAccountPersonCharge(long formId)
        {
            var chargeIds = await _db.Queryable<SystemAccountRequestDetailsEntity>()
                                     .With(SqlWith.NoLock)
                                     .InnerJoin<PersonInChargeConfigEntity>((detail, config) => detail.System == config.System && config.Type == "SystemAccount")
                                     .Where((detail, config) => detail.FormId == formId)
                                     .OrderBy((detail, config) => config.SortOrder)
                                     .Select((detail, config) => config.PersonChargeId)
                                     .ToListAsync();

            return ResolveUsers(chargeIds);
        }

        /// <summary>
        /// 申请人确认：表单的发起申请人
        /// </summary>
        public async Task<CustomUser?> SystemAccountApplicantRating(long formId)
        {
            var applicantUserId = await _db.Queryable<FormInstanceEntity>()
                                           .With(SqlWith.NoLock)
                                           .Where(instance => instance.FormId == formId)
                                           .Select(instance => instance.ApplicantUserId)
                                           .FirstAsync();

            return ResolveUsers(applicantUserId == 0 ? new List<long>() : new List<long> { applicantUserId });
        }
        #endregion

        #region 公共
        /// <summary>
        /// 组装点名人员结果；无人返回 null（步骤跳过）
        /// </summary>
        private static CustomUser? ResolveUsers(List<long> userIds)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0)
                return null;

            return new CustomUser()
            {
                UserIds = distinctIds
            };
        }
        #endregion
    }
}
