using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.FormBusiness.Forms.OverseasTripApp.Entity;
using SystemAdmin.Model.FormBusiness.Workflow.PersonResolver.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 步骤自定义😇
    /// </summary>
    public class WorkflowCustomResolver
    {
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        // guidance(方法名) -> 实际方法
        private readonly Dictionary<string, Func<long, Task<CustomUser>>> _registry;

        public WorkflowCustomResolver(CurrentUser loginuser, SqlSugarScope db, Language lang)
        {
            _loginuser = loginuser;
            _db = db;
            _lang = lang;

            // 登记所有自定义取人方法，新增方法只需在这里加一行
            _registry = new Dictionary<string, Func<long, Task<CustomUser>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(DeparturePlantManager)] = DeparturePlantManager,
                [nameof(DestinationPlantManager)] = DestinationPlantManager,
                [nameof(DepartureFactoryHRManager)] = DepartureFactoryHRManager,
                [nameof(DestinationFactoryHRManager)] = DestinationFactoryHRManager,
            };
        }

        /// <summary>
        /// 按 guidance(配置的方法名) 分发到对应的自定义取人方法。
        /// 解析结果只定位到「部门 + 职级」这一角色，具体由谁审批（实职 / 兼任 / 代理，
        /// 以及精确匹配落空后的降级兜底）统一交给 FormReviewFlow / FormReviewAction
        /// 既有的按部门职级取人逻辑处理，与其他指派方式共用同一套身份优先级
        /// </summary>
        public async Task<CustomUser> Resolve(string guidance, long formId)
        {
            if (string.IsNullOrWhiteSpace(guidance) || !_registry.TryGetValue(guidance, out var handler))
                throw new InvalidOperationException($"未配置的取人方法: {guidance}");

            return await handler(formId);
        }

        #region 出差单
        /// <summary>
        /// 出发厂区厂长：根据出差单的出发厂区，定位该厂区下部门级别为厂级(Plant)、职级为厂长(S08)的角色
        /// </summary>
        public async Task<CustomUser> DeparturePlantManager(long formId)
        {
            var departureFactory = await _db.Queryable<OverseasTripAppEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(trip => trip.FormId == formId)
                                            .Select(trip => trip.DepartureFactory)
                                            .FirstAsync();

            return await ResolvePlantManagerRole(departureFactory);
        }

        /// <summary>
        /// 目的厂区厂长：根据出差单的目的厂区，定位该厂区下部门级别为厂级(Plant)、职级为厂长(S08)的角色
        /// </summary>
        public async Task<CustomUser> DestinationPlantManager(long formId)
        {
            var destinationFactory = await _db.Queryable<OverseasTripAppEntity>()
                                              .With(SqlWith.NoLock)
                                              .Where(trip => trip.FormId == formId)
                                              .Select(trip => trip.DestinationFactory)
                                              .FirstAsync();

            return await ResolvePlantManagerRole(destinationFactory);
        }

        /// <summary>
        /// 按厂区定位部门级别为厂级(Plant)、职级为厂长(S08)的角色
        /// </summary>
        private async Task<CustomUser> ResolvePlantManagerRole(string? factory)
        {
            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .InnerJoin<DepartmentLevelEntity>((dept, level) => dept.DepartmentLevelId == level.DepartmentLevelId)
                                .Where((dept, level) => dept.Factory == factory && level.DepartmentLevelCode == "Plant")
                                .Select((dept, level) => dept)
                                .FirstAsync();

            return await ResolveRole(dept, "S08");
        }

        /// <summary>
        /// 出发厂区人资经理：根据出差单的出发厂区，定位该厂区下部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        public async Task<CustomUser> DepartureFactoryHRManager(long formId)
        {
            var departureFactory = await _db.Queryable<OverseasTripAppEntity>()
                                            .With(SqlWith.NoLock)
                                            .Where(trip => trip.FormId == formId)
                                            .Select(trip => trip.DepartureFactory)
                                            .FirstAsync();

            return await ResolveFactoryHRManagerRole(departureFactory);
        }

        /// <summary>
        /// 目的厂区人资经理：根据出差单的目的厂区，定位该厂区下部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        public async Task<CustomUser> DestinationFactoryHRManager(long formId)
        {
            var destinationFactory = await _db.Queryable<OverseasTripAppEntity>()
                                              .With(SqlWith.NoLock)
                                              .Where(trip => trip.FormId == formId)
                                              .Select(trip => trip.DestinationFactory)
                                              .FirstAsync();

            return await ResolveFactoryHRManagerRole(destinationFactory);
        }

        /// <summary>
        /// 按厂区定位部门职能为人力资源(HumanResources)、职级为人资经理(S06)的角色
        /// </summary>
        private async Task<CustomUser> ResolveFactoryHRManagerRole(string? factory)
        {
            var dept = await _db.Queryable<DepartmentInfoEntity>()
                                .With(SqlWith.NoLock)
                                .Where(dept => dept.Factory == factory && dept.DepartmentFunctions == "HumanResources")
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
    }
}
