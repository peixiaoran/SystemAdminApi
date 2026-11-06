using SystemAdmin.Common.Enums.FormBusiness;
using SystemAdmin.Common.Utilities;

namespace SystemAdmin.Repository.FormBusiness.Workflow
{
    /// <summary>
    /// 审批人过滤方式
    /// </summary>
    internal enum ReviewUserFilter
    {
        /// <summary>组织架构（上级部门链 + 部门级别 + 职级）</summary>
        Org,

        /// <summary>指定部门 + 职级</summary>
        Dept,

        /// <summary>指定人</summary>
        User,
    }

    /// <summary>
    /// 审批人查询投影
    /// </summary>
    /// <param name="WithNames">是否输出姓名、身份名称、排序列</param>
    /// <param name="WithAgent">是否关联代理人</param>
    /// <param name="IsChinese">名称取中文还是英文列（仅 WithNames 时生效）</param>
    internal sealed record ReviewUserProjection(bool WithNames, bool WithAgent, bool IsChinese)
    {
        /// <summary>完整投影</summary>
        internal static ReviewUserProjection Named(bool isChinese) => new(true, true, isChinese);

        /// <summary>身份 + 代理</summary>
        internal static ReviewUserProjection Appointment { get; } = new(false, true, false);

        /// <summary>仅身份</summary>
        internal static ReviewUserProjection AppointmentNoAgent { get; } = new(false, false, false);
    }

    /// <summary>
    /// 审批人查询 SQL 模板
    /// </summary>
    internal static class ReviewUserSql
    {
        /// <summary>
        /// 精确匹配查询；requireReviewAuth 为 false 时不校验审批权限（加审）
        /// </summary>
        internal static string ExactSql(ReviewUserProjection projection, ReviewUserFilter filter, string parentDeptIds, string topN, string orderBy, bool requireReviewAuth = true)
        {
            // 指定人且不输出名称时无需关联组织架构
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

            return Compose(projection, joinOrg, fullTimeWhere, partTimeWhere, topN, orderBy, requireReviewAuth);
        }

        /// <summary>
        /// 批量精确匹配查询：一次取回多组条件的审批人，以 ComboKey 区分条件组；
        /// isReview 时每组只取第一笔；requireReviewAuth 为 false 时不校验审批权限（加审）
        /// </summary>
        internal static string ExactBatchSql(ReviewUserProjection projection, ReviewUserFilter filter, string parentDeptIds, string comboValues, bool isReview, bool requireReviewAuth = true)
        {
            bool joinOrg = filter != ReviewUserFilter.User || projection.WithNames;

            string where = filter == ReviewUserFilter.Org
                ? $"dept.DepartmentId IN ({parentDeptIds})"
                : "1 = 1";

            string fullTimeBranch = Branch(projection, partTime: false, joinOrg, where, isAuto: false,
                                           comboJoin: ComboJoin(filter, comboValues, partTime: false), requireReviewAuth: requireReviewAuth);
            string partTimeBranch = Branch(projection, partTime: true, joinOrg, where, isAuto: false,
                                           comboJoin: ComboJoin(filter, comboValues, partTime: true), requireReviewAuth: requireReviewAuth);

            string orderCore = BuildOrderCore(isReview, isAuto: false);
            string candidates = $@"
                {fullTimeBranch}
                UNION ALL
                {partTimeBranch}";

            if (!isReview)
            {
                return $@"
            SELECT
                {OuterColumns(projection)},
                t.ComboKey
            FROM (
                {candidates}
            ) t
            ORDER BY {orderCore}";
            }

            // 单审：每组只留排序第一笔
            return $@"
            SELECT
                {OuterColumns(projection)},
                t.ComboKey
            FROM (
                SELECT t.*,
                       ROW_NUMBER() OVER (PARTITION BY t.ComboKey ORDER BY {orderCore}) AS ComboRank
                FROM (
                    {candidates}
                ) t
            ) t
            WHERE t.ComboRank = 1
            ORDER BY {orderCore}";
        }

        /// <summary>
        /// 批量条件表：以值列表与候选人关联，同时把 ComboKey 带进结果
        /// </summary>
        private static string ComboJoin(ReviewUserFilter filter, string comboValues, bool partTime)
        {
            var (columns, on) = filter switch
            {
                ReviewUserFilter.Org => ("ComboKey, DeptLevelSort, PositionSort",
                                         @"combo.DeptLevelSort = deptlevel.SortOrder
                   AND combo.PositionSort = position.SortOrder"),
                ReviewUserFilter.Dept => ("ComboKey, DepartmentId, PositionSort",
                                          @"combo.DepartmentId = dept.DepartmentId
                   AND combo.PositionSort = position.SortOrder"),
                _ => ("ComboKey, UserId", partTime ? "combo.UserId = partime.UserId" : "combo.UserId = [user].UserId"),
            };

            return $@"
                INNER JOIN (VALUES {comboValues}) AS combo({columns})
                    ON {on}";
        }

        /// <summary>
        /// 自动降级查询：取上级部门链内职级最高、部门级别最内的一组人
        /// </summary>
        internal static string AutoRankedSql(ReviewUserProjection projection, string parentDeptIds, string topN, string orderBy)
        {
            string where = $@"dept.DepartmentId IN ({parentDeptIds})
                  AND position.SortOrder BETWEEN 1 AND @MaxPositionSort
                  AND deptlevel.SortOrder BETWEEN 1 AND @MaxDeptLevelSort";

            string fullTimeBranch = Branch(projection, partTime: false, joinOrg: true, where, isAuto: true, withSortColumns: true);
            string partTimeBranch = Branch(projection, partTime: true, joinOrg: true, where, isAuto: true, withSortColumns: true);

            // 排名第一即首个命中的 (职级, 部门级别)
            return $@"
            SELECT {topN}
                {OuterColumns(projection)}
            FROM (
                SELECT candidate.*,
                       DENSE_RANK() OVER (ORDER BY candidate.PositionSort DESC, candidate.DeptLevelSort DESC) AS DowngradeRank
                FROM (
                    {fullTimeBranch}
                    UNION ALL
                    {partTimeBranch}
                ) candidate
            ) t
            WHERE t.DowngradeRank = 1
            {orderBy}";
        }

        /// <summary>
        /// 排序：单审按身份优先级（实 &gt; 代 &gt; 兼 &gt; 兼代）+ 入职时间，其余仅按入职时间
        /// </summary>
        internal static string BuildOrderBy(bool isReview, bool isAuto) => $"ORDER BY {BuildOrderCore(isReview, isAuto)}";

        /// <summary>
        /// 排序表达式（不含 ORDER BY）
        /// </summary>
        internal static string BuildOrderCore(bool isReview, bool isAuto)
        {
            if (!isReview)
            {
                return "t.HireDate DESC";
            }

            string c0 = (isAuto ? AppointmentType.AutoActual : AppointmentType.Actual).ToEnumString();
            string c1 = (isAuto ? AppointmentType.AutoAgent : AppointmentType.Agent).ToEnumString();
            string c2 = (isAuto ? AppointmentType.AutoConcurrent : AppointmentType.Concurrent).ToEnumString();
            string c3 = (isAuto ? AppointmentType.AutoConcurrentAgent : AppointmentType.ConcurrentAgent).ToEnumString();

            return $@"CASE t.AppointmentType
                        WHEN '{c0}' THEN 0
                        WHEN '{c1}' THEN 1
                        WHEN '{c2}' THEN 2
                        WHEN '{c3}' THEN 3
                        ELSE 9
                    END ASC, t.HireDate DESC";
        }

        /// <summary>
        /// 身份优先级（实 &gt; 代 &gt; 兼 &gt; 兼代），数值越小越优先
        /// </summary>
        internal static int AppointmentPriority(string appointmentType)
        {
            if (appointmentType == AppointmentType.Actual.ToEnumString()) return 0;
            if (appointmentType == AppointmentType.Agent.ToEnumString()) return 1;
            if (appointmentType == AppointmentType.Concurrent.ToEnumString()) return 2;
            if (appointmentType == AppointmentType.ConcurrentAgent.ToEnumString()) return 3;
            return 9;
        }

        /// <summary>
        /// 取出所有 AppointmentType 枚举字符串
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

        private static string Compose(ReviewUserProjection projection, bool joinOrg, string fullTimeWhere, string partTimeWhere, string topN, string orderBy, bool requireReviewAuth)
        {
            string fullTimeBranch = Branch(projection, partTime: false, joinOrg, fullTimeWhere, isAuto: false, requireReviewAuth: requireReviewAuth);
            string partTimeBranch = Branch(projection, partTime: true, joinOrg, partTimeWhere, isAuto: false, requireReviewAuth: requireReviewAuth);

            return $@"
            SELECT {topN}
                {OuterColumns(projection)}
            FROM (
                {fullTimeBranch}
                UNION ALL
                {partTimeBranch}
            ) t
            {orderBy}";
        }

        private static string OuterColumns(ReviewUserProjection projection)
        {
            return projection.WithNames
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
        }

        private static string Branch(ReviewUserProjection projection, bool partTime, bool joinOrg, string where, bool isAuto, bool withSortColumns = false, string comboJoin = "", bool requireReviewAuth = true)
        {
            // 专职记实/代身份，兼职记兼/兼代身份；自动降级换用 Auto 前缀枚举
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
            // 精简投影下降级排名仍需排序列参与内层计算
            else if (withSortColumns)
            {
                columns.Add("deptlevel.SortOrder AS DeptLevelSort");
                columns.Add("position.SortOrder AS PositionSort");
            }

            columns.Add("[user].HireDate AS HireDate");

            // 批量查询带出条件组标识
            if (!string.IsNullOrEmpty(comboJoin))
            {
                columns.Add("combo.ComboKey AS ComboKey");
            }

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

            // 条件表放在最后关联，可引用前面所有关联表
            joins += comboJoin;

            // 加审为点名指派，不校验审批权限；在职与冻结状态仍需校验
            string reviewWhere = requireReviewAuth
                ? @"
                  AND [user].IsReview = 1"
                : string.Empty;

            return $@"                SELECT
                    {string.Join(@",
                    ", columns)}
                FROM {from}{joins}
                WHERE {where}{reviewWhere}
                  AND [user].IsEmployed = 1
                  AND [user].IsFreeze = 0";
        }
    }
}
