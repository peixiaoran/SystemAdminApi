using SqlSugar;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;

namespace SystemAdmin.Repository.SystemBasicMgmt.SystemAuth
{
    public class PasskeyRepository
    {
        private readonly SqlSugarScope _db;

        public PasskeyRepository(SqlSugarScope db)
        {
            _db = db;
        }

        /// <summary>
        /// 查询用户信息（含在职过滤）
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserInfoEntity> GetEmployedUser(long userId)
        {
            return await _db.Queryable<UserInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(user => user.UserId == userId && user.IsEmployed == 1)
                            .FirstAsync();
        }

        /// <summary>
        /// 按工号查询在职用户
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public async Task<UserInfoEntity> GetEmployedUserByUserNo(string userNo)
        {
            return await _db.Queryable<UserInfoEntity>()
                            .With(SqlWith.NoLock)
                            .Where(user => user.UserNo == userNo && user.IsEmployed == 1)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询用户最近一次签发的绑定令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<PasskeyBindTokenEntity> GetLatestBindToken(long userId)
        {
            return await _db.Queryable<PasskeyBindTokenEntity>()
                            .With(SqlWith.NoLock)
                            .Where(token => token.UserId == userId)
                            .OrderBy(token => token.CreatedDate, OrderByType.Desc)
                            .FirstAsync();
        }

        /// <summary>
        /// 新增绑定令牌
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> AddBindToken(PasskeyBindTokenEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 按哈希值查询绑定令牌
        /// </summary>
        /// <param name="tokenHash"></param>
        /// <returns></returns>
        public async Task<PasskeyBindTokenEntity> GetBindTokenByHash(string tokenHash)
        {
            return await _db.Queryable<PasskeyBindTokenEntity>()
                            .With(SqlWith.NoLock)
                            .Where(token => token.TokenHash == tokenHash)
                            .FirstAsync();
        }

        /// <summary>
        /// 使用绑定令牌：仅当令牌仍未使用、未撤销、未过期时才会成功，返回受影响行数（0 表示已不可用）
        /// </summary>
        /// <param name="bindTokenId"></param>
        /// <param name="usedByIp"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public async Task<int> ConsumeBindToken(long bindTokenId, string usedByIp, DateTime now)
        {
            return await _db.Updateable<PasskeyBindTokenEntity>()
                            .SetColumns(token => new PasskeyBindTokenEntity { UsedDate = now, UsedByIp = usedByIp })
                            .Where(token => token.BindTokenId == bindTokenId
                                         && token.UsedDate == null
                                         && token.RevokedDate == null
                                         && token.ExpiresAt > now)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 撤销用户名下所有未使用且未撤销的绑定令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> RevokeActiveBindTokens(long userId)
        {
            return await _db.Updateable<PasskeyBindTokenEntity>()
                            .SetColumns(token => new PasskeyBindTokenEntity { RevokedDate = DateTime.Now })
                            .Where(token => token.UserId == userId && token.UsedDate == null && token.RevokedDate == null)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 撤销指定绑定令牌
        /// </summary>
        /// <param name="bindTokenId"></param>
        /// <returns></returns>
        public async Task<int> RevokeBindToken(long bindTokenId)
        {
            return await _db.Updateable<PasskeyBindTokenEntity>()
                            .SetColumns(token => new PasskeyBindTokenEntity { RevokedDate = DateTime.Now })
                            .Where(token => token.BindTokenId == bindTokenId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询用户全部 Passkey
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<UserPasskeyEntity>> GetPasskeysByUser(long userId)
        {
            return await _db.Queryable<UserPasskeyEntity>()
                            .With(SqlWith.NoLock)
                            .Where(passkey => passkey.UserId == userId)
                            .OrderBy(passkey => passkey.CreatedDate)
                            .ToListAsync();
        }

        /// <summary>
        /// 按凭据Id查询 Passkey
        /// </summary>
        /// <param name="credentialId">Base64Url 凭据Id</param>
        /// <returns></returns>
        public async Task<UserPasskeyEntity> GetPasskeyByCredentialId(string credentialId)
        {
            return await _db.Queryable<UserPasskeyEntity>()
                            .With(SqlWith.NoLock)
                            .Where(passkey => passkey.CredentialId == credentialId)
                            .FirstAsync();
        }

        /// <summary>
        /// 新增 Passkey
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> AddPasskey(UserPasskeyEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 登录成功后更新签名计数与最后使用时间
        /// </summary>
        /// <param name="passkeyId"></param>
        /// <param name="signCount"></param>
        /// <param name="lastUsedDate"></param>
        /// <returns></returns>
        public async Task<int> UpdatePasskeyUsage(long passkeyId, long signCount, DateTime lastUsedDate)
        {
            return await _db.Updateable<UserPasskeyEntity>()
                            .SetColumns(passkey => new UserPasskeyEntity
                            {
                                SignCount = signCount,
                                LastUsedDate = lastUsedDate
                            })
                            .Where(passkey => passkey.PasskeyId == passkeyId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 重命名 Passkey（限定本人）
        /// </summary>
        /// <param name="passkeyId"></param>
        /// <param name="userId"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public async Task<int> RenamePasskey(long passkeyId, long userId, string deviceName)
        {
            return await _db.Updateable<UserPasskeyEntity>()
                            .SetColumns(passkey => new UserPasskeyEntity { DeviceName = deviceName })
                            .Where(passkey => passkey.PasskeyId == passkeyId && passkey.UserId == userId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除 Passkey（限定本人）
        /// </summary>
        /// <param name="passkeyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<int> DeletePasskey(long passkeyId, long userId)
        {
            return await _db.Deleteable<UserPasskeyEntity>()
                            .Where(passkey => passkey.PasskeyId == passkeyId && passkey.UserId == userId)
                            .ExecuteCommandAsync();
        }
    }
}
