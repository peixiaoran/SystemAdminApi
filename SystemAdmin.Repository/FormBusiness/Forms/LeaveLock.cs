using SqlSugar;

namespace SystemAdmin.Repository.FormBusiness.Forms
{
    /// <summary>
    /// 请假、销假并发锁（事务级应用锁，提交或回滚时自动释放，需在事务内调用）
    /// </summary>
    public static class LeaveLock
    {
        private const int TimeoutMilliseconds = 10000;

        /// <summary>
        /// 按用户加锁
        /// </summary>
        public static Task<bool> LockUsersAsync(SqlSugarScope db, params long?[] userIds)
        {
            // 按 Id 排序加锁，避免死锁
            var resources = userIds.Where(id => id.HasValue)
                                   .Select(id => id!.Value)
                                   .Distinct()
                                   .OrderBy(id => id)
                                   .Select(id => $"Leave:User:{id}");
            return LockAsync(db, resources);
        }

        /// <summary>
        /// 按请假单加锁
        /// </summary>
        public static Task<bool> LockLeaveRequestAsync(SqlSugarScope db, long leaveRequestId)
        {
            return LockAsync(db, new[] { $"Leave:Request:{leaveRequestId}" });
        }

        private static async Task<bool> LockAsync(SqlSugarScope db, IEnumerable<string> resources)
        {
            foreach (var resource in resources)
            {
                // 返回值 >= 0 表示已获得锁
                var code = await db.Ado.GetIntAsync(
                    "DECLARE @code INT; EXEC @code = sp_getapplock @Resource = @Resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = @Timeout; SELECT @code;",
                    new SugarParameter("@Resource", resource),
                    new SugarParameter("@Timeout", TimeoutMilliseconds));

                if (code < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
