using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 审批人查询过滤方式
    /// </summary>
    internal enum ReviewUserFilter
    {
        /// <summary>
        /// 按组织架构（申请人上级部门链 + 部门级别 + 职级）
        /// </summary>
        Org,

        /// <summary>
        /// 按指定部门 + 职级
        /// </summary>
        Dept,

        /// <summary>
        /// 按指定人（自定义解析出的人也走此过滤）
        /// </summary>
        User,
    }

    /// <summary>
    /// 审批人查询投影
    /// </summary>
    /// <param name="WithNames">true = UserReview 完整投影（含姓名、身份名称、排序列）；false = UserAppointment 精简投影</param>
    /// <param name="WithAgent">是否关联代理人（UserAgent）</param>
    /// <param name="IsChinese">姓名、字典名称取中文列还是英文列（仅 WithNames 时生效）</param>
    internal sealed record ReviewUserProjection(bool WithNames, bool WithAgent, bool IsChinese)
    {
        /// <summary>
        /// FormReviewFlow 使用：姓名 + 代理 + 身份名称
        /// </summary>
        internal static ReviewUserProjection Named(bool isChinese) => new(true, true, isChinese);

        /// <summary>
        /// FormReviewAction 使用：身份 + 代理
        /// </summary>
        internal static ReviewUserProjection Appointment { get; } = new(false, true, false);

        /// <summary>
        /// FormReviewAction 使用：仅实/兼身份（初始化待审批人时不关心代理）
        /// </summary>
        internal static ReviewUserProjection AppointmentNoAgent { get; } = new(false, false, false);
    }

    /// <summary>
    /// FormReviewFlow / FormReviewAction 共用的审批人查询 SQL 模板。
    /// 统一结构：专职 UNION ALL 兼职，可选关联生效中的代理人，外层按审批身份优先级 + 入职时间排序。
    /// </summary>
    internal static class ReviewUserSql
    {
        /// <summary>
        /// 精确匹配查询（按步骤配置的部门级别/职级/指定人查找）。
        /// 参数约定：Org → @DeptLevelSort、@PositionSort；Dept → @DepartmentId、@PositionSort；User → @UserId；
        /// 带代理时另需 @Now 及各身份枚举参数。
        /// </summary>
        internal static string ExactSql(ReviewUserProjection projection, ReviewUserFilter filter, string parentDeptIds, string topN, string orderBy)
        {
            // 精简投影按指定人查询时无需组织架构关联；完整投影始终需要（要输出部门级别/职级排序列）
            bool joinOrg = filter != ReviewUserFilter.User || projection.WithNames;

            string fullTimeWhere = filter switch
            {
                ReviewUserFilter.Org => $@"dept.DepartmentId IN ({parentDeptIds})
                  AND deptlevel.SortOrder = @DeptLevelSort
                  AND position.SortOrder = @PositionSort",
                ReviewUserFilter.Dept => @"dept.DepartmentId = @DepartmentId
                  AND position.SortOrder = @PositionSort",
                _ => "[user].UserId = @UserId",
            };
            string partTimeWhere = filter == ReviewUserFilter.User ? "partime.UserId = @UserId" : fullTimeWhere;

            return Compose(projection, joinOrg, fullTimeWhere, partTimeWhere, isAuto: false, topN, orderBy);
        }

        /// <summary>
        /// 自动降级查询：在申请人（或目标部门）的上级部门链内按 @CurrentPositionSort / @CurrentDeptLevelSort 查找。
        /// </summary>
        internal static string AutoSql(ReviewUserProjection projection, string parentDeptIds, string topN, string orderBy)
        {
            string where = $@"dept.DepartmentId IN ({parentDeptIds})
                  AND position.SortOrder = @CurrentPositionSort
                  AND deptlevel.SortOrder = @CurrentDeptLevelSort";

            return Compose(projection, joinOrg: true, where, where, isAuto: true, topN, orderBy);
        }

        /// <summary>
        /// 排序：单审时按审批身份优先级（实 &gt; 代 &gt; 兼 &gt; 兼代）+ 入职时间，其余仅按入职时间
        /// </summary>
        internal static string BuildOrderBy(bool isSingle, bool isAuto)
        {
            if (!isSingle)
            {
                return "ORDER BY t.HireDate DESC";
            }

            string c0 = (isAuto ? AppointmentType.AutoActual : AppointmentType.Actual).ToEnumString();
            string c1 = (isAuto ? AppointmentType.AutoAgent : AppointmentType.Agent).ToEnumString();
            string c2 = (isAuto ? AppointmentType.AutoConcurrent : AppointmentType.Concurrent).ToEnumString();
            string c3 = (isAuto ? AppointmentType.AutoConcurrentAgent : AppointmentType.ConcurrentAgent).ToEnumString();

            return $@"ORDER BY CASE t.AppointmentType
                        WHEN '{c0}' THEN 0
                        WHEN '{c1}' THEN 1
                        WHEN '{c2}' THEN 2
                        WHEN '{c3}' THEN 3
                        ELSE 9
                    END ASC, t.HireDate DESC";
        }

        /// <summary>
        /// 一次性取出所有 AppointmentType 枚举字符串
        /// </summary>
        internal static (string actual, string agent, string concurrent, string concurrentAgent, string autoActual, string autoAgent, string autoConcurrent, string autoConcurrentAgent) AppointmentEnumStrings() =>
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

        private static string Compose(ReviewUserProjection projection, bool joinOrg, string fullTimeWhere, string partTimeWhere, bool isAuto, string topN, string orderBy)
        {
            string outerColumns = projection.WithNames
                ? @"ReviewUserId,
                ReviewUserName,
                AgentUserId,
                AgentUserName,
                AppointmentType,
                AppointmentTypeName,
                DeptLevelSort,
                PositionSort,
                HireDate"
                : projection.WithAgent
                    ? @"t.ReviewUserId,
                t.AgentUserId,
                t.AppointmentType"
                    : @"t.ReviewUserId,
                t.AppointmentType";

            string fullTimeBranch = Branch(projection, partTime: false, joinOrg, fullTimeWhere, isAuto);
            string partTimeBranch = Branch(projection, partTime: true, joinOrg, partTimeWhere, isAuto);

            return $@"
            SELECT {topN}
                {outerColumns}
            FROM (

{fullTimeBranch}

                UNION ALL

{partTimeBranch}

            ) t
            {orderBy}";
        }

        private static string Branch(ReviewUserProjection projection, bool partTime, bool joinOrg, string where, bool isAuto)
        {
            // 专职分支记实/代身份，兼职分支记兼/兼代身份；自动降级时换用 Auto 前缀的身份枚举
            string actualParam = partTime
                ? (isAuto ? "@AutoConcurrent" : "@Concurrent")
                : (isAuto ? "@AutoActual" : "@Actual");
            string agentParam = partTime
                ? (isAuto ? "@AutoConcurrentAgent" : "@ConcurrentAgent")
                : (isAuto ? "@AutoAgent" : "@Agent");

            string userNameCol = projection.IsChinese ? "[user].UserNameCn" : "[user].UserNameEn";
            string agentNameCol = projection.IsChinese ? "agentusers.UserNameCn" : "agentusers.UserNameEn";
            string dicNameCol = projection.IsChinese ? "dic.DicNameCn" : "dic.DicNameEn";

            var columns = new List<string> { "[user].UserId AS ReviewUserId" };

            if (projection.WithNames)
            {
                columns.Add($"{userNameCol} AS ReviewUserName");
            }

            if (projection.WithAgent)
            {
                columns.Add(projection.WithNames
                    ? "agentusers.UserId AS AgentUserId"
                    : "ISNULL(agentusers.UserId, 0) AS AgentUserId");
            }

            if (projection.WithNames)
            {
                columns.Add($"{agentNameCol} AS AgentUserName");
            }

            columns.Add(projection.WithAgent
                ? $@"CASE
                        WHEN agent.AgentUserId IS NOT NULL THEN {agentParam}
                        ELSE {actualParam}
                    END AS AppointmentType"
                : $"{actualParam} AS AppointmentType");

            if (projection.WithNames)
            {
                columns.Add($@"CASE
                        WHEN agent.AgentUserId IS NOT NULL
                            THEN (
                                SELECT {dicNameCol}
                                FROM Basic.DictionaryInfo dic
                                WHERE dic.DicType = 'AppointmentType'
                                  AND dic.DicCode = {agentParam}
                            )
                        ELSE (
                            SELECT {dicNameCol}
                            FROM Basic.DictionaryInfo dic
                            WHERE dic.DicType = 'AppointmentType'
                              AND dic.DicCode = {actualParam}
                        )
                    END AS AppointmentTypeName");
                columns.Add("deptlevel.SortOrder AS DeptLevelSort");
                columns.Add("position.SortOrder AS PositionSort");
            }

            columns.Add("[user].HireDate AS HireDate");

            string from = partTime
                ? @"Basic.UserPartTime partime
                INNER JOIN Basic.UserInfo [user]
                    ON partime.UserId = [user].UserId"
                : "Basic.UserInfo [user]";

            string joins = string.Empty;
            if (joinOrg)
            {
                string deptOn = partTime ? "partime.PartTimeDeptId" : "[user].DepartmentId";
                string positionOn = partTime ? "partime.PartTimePositionId" : "[user].PositionId";

                joins += $@"
                INNER JOIN Basic.DepartmentInfo dept
                    ON {deptOn} = dept.DepartmentId
                INNER JOIN Basic.DepartmentLevel deptlevel
                    ON dept.DepartmentLevelId = deptlevel.DepartmentLevelId
                INNER JOIN Basic.PositionInfo position
                    ON {positionOn} = position.PositionId";
            }

            if (projection.WithAgent)
            {
                joins += @"
                LEFT JOIN Basic.UserAgent agent
                    ON [user].UserId = agent.SubstituteUserId
                   AND agent.StartTime <= @Now
                   AND agent.EndTime >= @Now
                LEFT JOIN Basic.UserInfo agentusers
                    ON agent.AgentUserId = agentusers.UserId";
            }

            return $@"                SELECT
                    {string.Join(@",
                    ", columns)}
                FROM {from}{joins}
                WHERE {where}
                  AND [user].IsReview = 1
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0";
        }
    }
}
