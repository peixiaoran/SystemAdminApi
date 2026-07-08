using SqlSugar;
using SystemAdmin.Model.FormBusiness.Forms.LeaveRequest.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 工作流规则条件😇
    /// </summary>
    public class WorkflowRuleConditions
    {
        private readonly SqlSugarScope _db;

        private readonly Dictionary<string, Func<long, Task<bool>>> _registry;

        public WorkflowRuleConditions(SqlSugarScope db)
        {
            _db = db;

            _registry = new Dictionary<string, Func<long, Task<bool>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(LessOver3)] = LessOver3,
                [nameof(MoreOver3)] = MoreOver3,
            };
        }

        public async Task<bool> Resolve(string guidance, long formId)
        {
            if (string.IsNullOrWhiteSpace(guidance))
            {
                return true;
            }

            if (!_registry.TryGetValue(guidance, out var handler))
            {
                return false;
            }

            return await handler(formId);
        }

        #region 请假单

        public async Task<bool> LessOver3(long formId)
        {
            var leave = await _db.Queryable<LeaveRequestEntity>()
                                 .With(SqlWith.NoLock)
                                 .FirstAsync(x => x.FormId == formId);

            return leave != null && leave.LeaveHours <= 24;
        }

        public async Task<bool> MoreOver3(long formId)
        {
            var leave = await _db.Queryable<LeaveRequestEntity>()
                                 .With(SqlWith.NoLock)
                                 .FirstAsync(x => x.FormId == formId);

            return leave != null && leave.LeaveHours > 24;
        }

        #endregion
    }
}