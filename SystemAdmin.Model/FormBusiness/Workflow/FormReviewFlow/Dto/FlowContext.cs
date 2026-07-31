using SystemAdmin.Model.FormBusiness.FormWorkflow.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Model.FormBusiness.Workflow.FormReviewFlow.Dto
{
    /// <summary>
    /// 流程构建上下文（步骤循环前批量预载，循环内不再往返数据库）
    /// </summary>
    public class FlowContext
    {
        /// <summary>
        /// 步骤资料 (StepId -&gt; 步骤)
        /// </summary>
        public Dictionary<long, WorkflowStepEntity> StepInfoMap { get; set; } = new Dictionary<long, WorkflowStepEntity>();

        /// <summary>
        /// 步骤链 (StepId -&gt; 下一步骤Id)
        /// </summary>
        public Dictionary<long, long?> NextStepMap { get; set; } = new Dictionary<long, long?>();

        /// <summary>
        /// 规则内步骤排序 (StepId -&gt; SortOrder)
        /// </summary>
        public Dictionary<long, int> RuleStepSortMap { get; set; } = new Dictionary<long, int>();

        /// <summary>
        /// 起始步骤Id
        /// </summary>
        public long? FirstStepId { get; set; }

        /// <summary>
        /// 申请人上级部门链（含所在部门）
        /// </summary>
        public List<DepartmentInfoEntity> ApplyParentDept { get; set; } = new List<DepartmentInfoEntity>();

        /// <summary>
        /// 组织架构指派配置 (StepId -&gt; 配置)
        /// </summary>
        public Dictionary<long, WorkflowStepOrgEntity> OrgConfigMap { get; set; } = new Dictionary<long, WorkflowStepOrgEntity>();

        /// <summary>
        /// 指定部门职级指派配置 (StepId -&gt; 配置)
        /// </summary>
        public Dictionary<long, WorkflowStepDeptUserEntity> DeptUserConfigMap { get; set; } = new Dictionary<long, WorkflowStepDeptUserEntity>();

        /// <summary>
        /// 指定人指派配置 (StepId -&gt; 配置)
        /// </summary>
        public Dictionary<long, WorkflowStepUserEntity> UserConfigMap { get; set; } = new Dictionary<long, WorkflowStepUserEntity>();

        /// <summary>
        /// 自定义指派配置 (StepId -&gt; 配置)
        /// </summary>
        public Dictionary<long, WorkflowStepCustomEntity> CustomConfigMap { get; set; } = new Dictionary<long, WorkflowStepCustomEntity>();

        /// <summary>
        /// 配置涉及的部门 (DepartmentId -&gt; 部门)
        /// </summary>
        public Dictionary<long, DepartmentInfoEntity> DeptMap { get; set; } = new Dictionary<long, DepartmentInfoEntity>();

        /// <summary>
        /// 配置涉及的人员 (UserId -&gt; 人员)
        /// </summary>
        public Dictionary<long, UserInfoEntity> UserMap { get; set; } = new Dictionary<long, UserInfoEntity>();

        /// <summary>
        /// 加审指派配置 (StepId -&gt; 配置)
        /// </summary>
        public Dictionary<long, WorkflowStepAddReviewEntity> AddReviewConfigMap { get; set; } = new Dictionary<long, WorkflowStepAddReviewEntity>();

        /// <summary>
        /// 表单加审人 (加审SortOrder -&gt; 加审人Id列表)
        /// </summary>
        public Dictionary<int, List<long>> FormAddReviewMap { get; set; } = new Dictionary<int, List<long>>();

        /// <summary>
        /// 部门级别排序 (DepartmentLevelId -&gt; SortOrder)
        /// </summary>
        public Dictionary<long, int> DeptLevelSortMap { get; set; } = new Dictionary<long, int>();

        /// <summary>
        /// 职级排序 (PositionId -&gt; SortOrder)
        /// </summary>
        public Dictionary<long, int> PositionSortMap { get; set; } = new Dictionary<long, int>();

        /// <summary>
        /// 取部门级别排序，取不到返回 0（该步骤后续查不到审批人）
        /// </summary>
        public int DeptLevelSort(long deptLevelId) => DeptLevelSortMap.TryGetValue(deptLevelId, out var sortOrder) ? sortOrder : 0;

        /// <summary>
        /// 取职级排序，取不到返回 0（该步骤后续查不到审批人）
        /// </summary>
        public int PositionSort(long positionId) => PositionSortMap.TryGetValue(positionId, out var sortOrder) ? sortOrder : 0;

        /// <summary>
        /// 取该加审顺序上的加审人Id，没有则返回空列表
        /// </summary>
        public List<long> AddReviewUserIds(int addReviewSortOrder) => FormAddReviewMap.TryGetValue(addReviewSortOrder, out var userIds) ? userIds : new List<long>();
    }
}
