using SqlSugar;
using SystemAdmin.Model.FormBusiness.Forms.LeaveForm.Entity;

namespace SystemAdmin.Repository.FormBusiness.Workflow.PublickWorkflow
{
    /// <summary>
    /// 步骤结束执行
    /// </summary>
    public class WorkflowStepCompletion
    {
        private readonly SqlSugarScope _db;

        private readonly Dictionary<string, Func<long, Task<bool>>> _registry;

        public WorkflowStepCompletion(SqlSugarScope db)
        {
            _db = db;

            _registry = new Dictionary<string, Func<long, Task<bool>>>(StringComparer.OrdinalIgnoreCase)
            {
                [nameof(SettlementHoliday)] = SettlementHoliday,
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

        public async Task<bool> SettlementHoliday(long formId)
        {
            var leave = await _db.Ado.ExecuteCommandAsync("INSERT INTO [Hr].[UserLeaveAnnual] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (1, 1, N'1', 1.00, 1.00, 1, '2026-06-11 15:02:30.000', NULL, NULL);");
            return leave == 1;
        }

        #endregion
    }
}
