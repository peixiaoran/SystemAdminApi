using Fido2NetLib;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Security.Cryptography;
using System.Text;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity;
using SystemAdmin.Repository.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.Service.SystemBasicMgmt.SystemAuth
{
    /// <summary>凭邮件中的绑定链接令牌绑定 Passkey（匿名访问，身份由令牌证明）</summary>
    public class PasskeyBindService
    {
        private readonly ILogger<PasskeyBindService> _logger;
        private readonly SqlSugarScope _db;
        private readonly PasskeyRepository _passkeyRepo;
        private readonly PasskeyService _passkeyService;
        private readonly SysUserOperateService _sysUserOperateService;
        private readonly LocalizationService _localization;
        private readonly string _this = "SystemBasicMgmt.SystemAuth.Passkey";

        public PasskeyBindService(ILogger<PasskeyBindService> logger, SqlSugarScope db, PasskeyRepository passkeyRepo, PasskeyService passkeyService, SysUserOperateService sysUserOperateService, LocalizationService localization)
        {
            _logger = logger;
            _db = db;
            _passkeyRepo = passkeyRepo;
            _passkeyService = passkeyService;
            _sysUserOperateService = sysUserOperateService;
            _localization = localization;
        }

        /// <summary>
        /// 凭令牌查询用户信息，供绑定页面展示
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<PasskeyBindUserInfoDto>> GetBindUserInfo(PasskeyBindQuery query)
        {
            try
            {
                var (token, error) = await ValidateToken(query.Token);
                if (token == null)
                {
                    return Result<PasskeyBindUserInfoDto>.Failure(400, error!);
                }

                var user = await _passkeyRepo.GetEmployedUser(token.UserId);
                if (user == null)
                {
                    return Result<PasskeyBindUserInfoDto>.Failure(406, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                var passkeys = await _passkeyRepo.GetPasskeysByUser(user.UserId);

                return Result<PasskeyBindUserInfoDto>.Ok(new PasskeyBindUserInfoDto
                {
                    UserNo = user.UserNo,
                    UserNameCn = user.UserNameCn,
                    UserNameEn = user.UserNameEn,
                    Email = MaskEmail(token.Email),
                    ExpiresAt = token.ExpiresAt,
                    PasskeyCount = passkeys.Count
                }, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<PasskeyBindUserInfoDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 凭令牌生成 Passkey 注册选项
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Result<CredentialCreateOptions>> PasskeyBindOptions(PasskeyBindQuery query)
        {
            try
            {
                var (token, error) = await ValidateToken(query.Token);
                if (token == null)
                {
                    return Result<CredentialCreateOptions>.Failure(400, error!);
                }

                var user = await _passkeyRepo.GetEmployedUser(token.UserId);
                if (user == null)
                {
                    return Result<CredentialCreateOptions>.Failure(406, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                var existing = await _passkeyRepo.GetPasskeysByUser(user.UserId);
                if (existing.Count >= PasskeyService.MaxPasskeysPerUser)
                {
                    return Result<CredentialCreateOptions>.Failure(400, _localization.ReturnMsg($"{_this}LimitReached", PasskeyService.MaxPasskeysPerUser));
                }

                var options = _passkeyService.BuildRegisterOptions(user, existing);
                await _passkeyService.SetChallenge(BindCacheKey(token.BindTokenId), options.ToJson());

                return Result<CredentialCreateOptions>.Ok(options, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CredentialCreateOptions>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 凭令牌完成 Passkey 绑定：校验认证器响应、保存凭据并让令牌作废
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public async Task<Result<int>> PasskeyBindComplete(PasskeyBindComplete complete)
        {
            try
            {
                var (token, error) = await ValidateToken(complete.Token);
                if (token == null)
                {
                    return Result<int>.Failure(400, error!);
                }

                var optionsJson = await _passkeyService.TakeChallenge(BindCacheKey(token.BindTokenId));
                if (string.IsNullOrEmpty(optionsJson))
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}ChallengeExpired"));
                }

                var existing = await _passkeyRepo.GetPasskeysByUser(token.UserId);
                if (existing.Count >= PasskeyService.MaxPasskeysPerUser)
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}LimitReached", PasskeyService.MaxPasskeysPerUser));
                }

                var entity = await _passkeyService.VerifyRegistration(token.UserId, complete.Attestation, optionsJson, complete.DeviceName);

                // 令牌作废与凭据保存在同一事务内；令牌被并发使用或刚好过期时，条件更新影响 0 行
                await _db.BeginTranAsync();
                var consumed = await _passkeyRepo.ConsumeBindToken(token.BindTokenId, _sysUserOperateService.GetClientIp(), DateTime.Now);
                if (consumed == 0)
                {
                    await _db.RollbackTranAsync();
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}BindTokenUsed"));
                }

                var count = await _passkeyRepo.AddPasskey(entity);
                await _db.CommitTranAsync();

                return Result<int>.Ok(count, _localization.ReturnMsg($"{_this}BindSuccess"));
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "Passkey 绑定校验失败");
                return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}RegisterFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 校验令牌：存在、未被替换或撤销、未使用、未过期；通过返回令牌实体，否则返回提示文案
        /// </summary>
        private async Task<(PasskeyBindTokenEntity? Token, string? Error)> ValidateToken(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                return (null, _localization.ReturnMsg($"{_this}BindTokenInvalid"));
            }

            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken.Trim())));
            var token = await _passkeyRepo.GetBindTokenByHash(tokenHash);
            if (token == null)
            {
                return (null, _localization.ReturnMsg($"{_this}BindTokenInvalid"));
            }

            if (token.UsedDate != null)
            {
                return (null, _localization.ReturnMsg($"{_this}BindTokenUsed"));
            }

            if (token.RevokedDate != null)
            {
                return (null, _localization.ReturnMsg($"{_this}BindTokenRevoked"));
            }

            if (token.ExpiresAt <= DateTime.Now)
            {
                return (null, _localization.ReturnMsg($"{_this}BindTokenExpired"));
            }

            return (token, null);
        }

        private static string BindCacheKey(long bindTokenId) => $"passkey:bind:{bindTokenId}";

        /// <summary>邮箱脱敏：保留首字符与域名，如 z***@example.com</summary>
        private static string MaskEmail(string email)
        {
            var at = email.IndexOf('@');
            if (at <= 0)
            {
                return email;
            }

            return $"{email[0]}***{email[at..]}";
        }
    }
}
