using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Buffers.Text;
using SystemAdmin.Common.Enums.SystemBasicMgmt;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.Service.SystemBasicMgmt.SystemAuth
{
    public class PasskeyService
    {
        private readonly ILogger<PasskeyService> _logger;
        private readonly CurrentUser _loginuser;
        private readonly SqlSugarScope _db;
        private readonly IFido2 _fido2;
        private readonly PasskeyOptions _options;
        private readonly PasskeyRepository _passkeyRepo;
        private readonly SysUserOperateRepository _sysUserOperateRepo;
        private readonly SysUserOperateService _sysUserOperateService;
        private readonly LocalizationService _localization;
        private readonly HybridCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _this = "SystemBasicMgmt.SystemAuth.Passkey";
        private const string UserOperateKey = "SystemBasicMgmt.SystemAuth.SysUserOperate";
        public const int MaxPasskeysPerUser = 10;
        private const int MaxDeviceNameLength = 100;

        public PasskeyService(ILogger<PasskeyService> logger, CurrentUser loginuser, SqlSugarScope db, IFido2 fido2, IOptions<PasskeyOptions> options, PasskeyRepository passkeyRepo, SysUserOperateRepository sysUserOperateRepo, SysUserOperateService sysUserOperateService, LocalizationService localization, HybridCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _loginuser = loginuser;
            _db = db;
            _fido2 = fido2;
            _options = options.Value;
            _passkeyRepo = passkeyRepo;
            _sysUserOperateRepo = sysUserOperateRepo;
            _sysUserOperateService = sysUserOperateService;
            _localization = localization;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 注册 Passkey：生成注册选项（需已登录）
        /// </summary>
        /// <returns></returns>
        public async Task<Result<CredentialCreateOptions>> PasskeyRegisterOptions()
        {
            try
            {
                var user = await _passkeyRepo.GetEmployedUser(_loginuser.UserId);
                if (user == null)
                {
                    return Result<CredentialCreateOptions>.Failure(406, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                var existing = await _passkeyRepo.GetPasskeysByUser(user.UserId);
                if (existing.Count >= MaxPasskeysPerUser)
                {
                    return Result<CredentialCreateOptions>.Failure(400, _localization.ReturnMsg($"{_this}LimitReached", MaxPasskeysPerUser));
                }

                var options = BuildRegisterOptions(user, existing);

                await SetChallenge(RegisterCacheKey(user.UserId), options.ToJson());

                return Result<CredentialCreateOptions>.Ok(options, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<CredentialCreateOptions>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 注册 Passkey：校验认证器响应并保存凭据（需已登录）
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public async Task<Result<int>> PasskeyRegisterComplete(PasskeyRegisterComplete complete)
        {
            try
            {
                var userId = _loginuser.UserId;

                var optionsJson = await TakeChallenge(RegisterCacheKey(userId));
                if (string.IsNullOrEmpty(optionsJson))
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}ChallengeExpired"));
                }

                var entity = await VerifyRegistration(userId, complete.Attestation, optionsJson, complete.DeviceName);
                var count = await _passkeyRepo.AddPasskey(entity);

                return Result<int>.Ok(count, _localization.ReturnMsg($"{_this}RegisterSuccess"));
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "Passkey 注册校验失败");
                return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}RegisterFailed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// Passkey 登录：生成登录选项（匿名，可发现凭据，无需先输入账号）
        /// </summary>
        /// <returns></returns>
        public async Task<Result<PasskeyLoginOptionsDto>> PasskeyLoginOptions()
        {
            try
            {
                var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
                {
                    AllowedCredentials = new List<PublicKeyCredentialDescriptor>(),
                    UserVerification = UserVerificationRequirement.Required
                });

                var sessionId = Guid.NewGuid().ToString("N");
                await SetChallenge(LoginCacheKey(sessionId), options.ToJson());

                return Result<PasskeyLoginOptionsDto>.Ok(new PasskeyLoginOptionsDto
                {
                    SessionId = sessionId,
                    Options = options
                }, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<PasskeyLoginOptionsDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// Passkey 登录：校验断言并签发会话（匿名）
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public async Task<Result<SysUserLoginReturnDto>> PasskeyLoginComplete(HttpResponse httpResponse, PasskeyLoginComplete complete)
        {
            try
            {
                var ip = _sysUserOperateService.GetClientIp();
                var nowTime = DateTime.Now;

                var optionsJson = await TakeChallenge(LoginCacheKey(complete.SessionId));
                if (string.IsNullOrEmpty(optionsJson))
                {
                    return Result<SysUserLoginReturnDto>.Failure(400, _localization.ReturnMsg($"{_this}ChallengeExpired"));
                }

                var stored = await _passkeyRepo.GetPasskeyByCredentialId(Base64Url.EncodeToString(complete.Assertion.RawId));
                if (stored == null)
                {
                    return Result<SysUserLoginReturnDto>.Failure(401, _localization.ReturnMsg($"{_this}LoginFailed"));
                }

                var assertion = await _fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = complete.Assertion,
                    OriginalOptions = AssertionOptions.FromJson(optionsJson),
                    StoredPublicKey = stored.PublicKey,
                    StoredSignatureCounter = (uint)stored.SignCount,
                    // 认证器返回的 userHandle 必须属于该凭据的所有者
                    IsUserHandleOwnerOfCredentialIdCallback = (args, ct) =>
                        Task.FromResult(args.UserHandle != null && args.UserHandle.AsSpan().SequenceEqual(ToUserHandle(stored.UserId)))
                });

                // 与密码登录保持一致：离职/冻结账号不可登录（Passkey 不涉及密码，故不校验密码过期）
                var user = await _passkeyRepo.GetEmployedUser(stored.UserId);
                if (user == null)
                {
                    return Result<SysUserLoginReturnDto>.Failure(401, _localization.ReturnMsg($"{_this}LoginFailed"));
                }

                if (user.IsFreeze != 0)
                {
                    return Result<SysUserLoginReturnDto>.Failure(401, _localization.ReturnMsg($"{UserOperateKey}LoginFreeze"));
                }

                await _db.BeginTranAsync();
                await _passkeyRepo.UpdatePasskeyUsage(stored.PasskeyId, assertion.SignCount, nowTime);
                await _sysUserOperateRepo.AddUserLoginLogInfo(new UserLogOutEntity
                {
                    UserId = user.UserId,
                    LoginType = LoginBehavior.PasskeyLoginSuccessful.ToEnumString(),
                    IP = ip,
                    LoginDate = nowTime
                });
                await _sysUserOperateRepo.EmptyUserLock(user.UserId);
                await _db.CommitTranAsync();

                var loginReturn = await _sysUserOperateService.IssueLoginSession(httpResponse, user, nowTime, ip);

                return Result<SysUserLoginReturnDto>.Ok(loginReturn, _localization.ReturnMsg($"{UserOperateKey}LoginSuccessful"));
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "Passkey 登录校验失败");
                return Result<SysUserLoginReturnDto>.Failure(401, _localization.ReturnMsg($"{_this}LoginFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<SysUserLoginReturnDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询当前用户的 Passkey 列表
        /// </summary>
        /// <returns></returns>
        public async Task<Result<List<PasskeyDto>>> GetPasskeyList()
        {
            try
            {
                var list = await _passkeyRepo.GetPasskeysByUser(_loginuser.UserId);
                var dtos = list.Select(passkey => new PasskeyDto
                {
                    PasskeyId = passkey.PasskeyId.ToString(),
                    DeviceName = passkey.DeviceName,
                    CreatedDate = passkey.CreatedDate,
                    LastUsedDate = passkey.LastUsedDate
                }).ToList();

                return Result<List<PasskeyDto>>.Ok(dtos, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<List<PasskeyDto>>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 重命名 Passkey
        /// </summary>
        /// <param name="rename"></param>
        /// <returns></returns>
        public async Task<Result<int>> RenamePasskey(PasskeyRename rename)
        {
            try
            {
                var deviceName = rename.DeviceName?.Trim() ?? string.Empty;
                if (!long.TryParse(rename.PasskeyId, out var passkeyId) || deviceName.Length == 0)
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}NotFound"));
                }

                if (deviceName.Length > MaxDeviceNameLength)
                {
                    deviceName = deviceName[..MaxDeviceNameLength];
                }

                var count = await _passkeyRepo.RenamePasskey(passkeyId, _loginuser.UserId, deviceName);

                return count >= 1
                    ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}RenameSuccess"))
                    : Result<int>.Failure(404, _localization.ReturnMsg($"{_this}NotFound"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 删除 Passkey
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeletePasskey(PasskeyDelete delete)
        {
            try
            {
                if (!long.TryParse(delete.PasskeyId, out var passkeyId))
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}NotFound"));
                }

                var count = await _passkeyRepo.DeletePasskey(passkeyId, _loginuser.UserId);

                return count >= 1
                    ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}DeleteSuccess"))
                    : Result<int>.Failure(404, _localization.ReturnMsg($"{_this}NotFound"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 构造注册选项：可发现凭据 + 必须用户验证，并排除该用户已有凭据
        /// </summary>
        /// <param name="user"></param>
        /// <param name="existing"></param>
        /// <returns></returns>
        public CredentialCreateOptions BuildRegisterOptions(UserInfoEntity user, List<UserPasskeyEntity> existing)
        {
            var fidoUser = new Fido2User
            {
                Id = ToUserHandle(user.UserId),
                Name = user.LoginNo,
                DisplayName = string.IsNullOrWhiteSpace(user.UserNameCn) ? user.UserNameEn : user.UserNameCn
            };

            return _fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fidoUser,
                ExcludeCredentials = existing
                    .Select(passkey => new PublicKeyCredentialDescriptor(Base64Url.DecodeFromChars(passkey.CredentialId)))
                    .ToList(),
                // 可发现凭据 + 必须用户验证，支持免输账号的 Passkey 登录
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    ResidentKey = ResidentKeyRequirement.Required,
                    UserVerification = UserVerificationRequirement.Required
                },
                AttestationPreference = AttestationConveyancePreference.None
            });
        }

        /// <summary>
        /// 校验认证器的注册响应并生成待保存的凭据实体（校验失败抛出 Fido2VerificationException）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="attestation"></param>
        /// <param name="optionsJson">注册选项的 JSON</param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public async Task<UserPasskeyEntity> VerifyRegistration(long userId, AuthenticatorAttestationRawResponse attestation, string optionsJson, string? deviceName)
        {
            var registered = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
            {
                AttestationResponse = attestation,
                OriginalOptions = CredentialCreateOptions.FromJson(optionsJson),
                // 凭据Id全局唯一
                IsCredentialIdUniqueToUserCallback = async (args, ct) =>
                    await _passkeyRepo.GetPasskeyByCredentialId(Base64Url.EncodeToString(args.CredentialId)) == null
            });

            var name = string.IsNullOrWhiteSpace(deviceName) ? "Passkey" : deviceName.Trim();
            if (name.Length > MaxDeviceNameLength)
            {
                name = name[..MaxDeviceNameLength];
            }

            return new UserPasskeyEntity
            {
                PasskeyId = SnowFlakeSingle.Instance.NextId(),
                UserId = userId,
                CredentialId = Base64Url.EncodeToString(registered.Id),
                PublicKey = registered.PublicKey,
                SignCount = registered.SignCount,
                AaGuid = registered.AaGuid,
                DeviceName = name,
                CreatedDate = DateTime.Now
            };
        }

        private static string RegisterCacheKey(long userId) => $"passkey:reg:{userId}";

        private static string LoginCacheKey(string sessionId) => $"passkey:login:{sessionId}";

        /// <summary>用户Id转 WebAuthn userHandle（8 字节）</summary>
        private static byte[] ToUserHandle(long userId) => BitConverter.GetBytes(userId);

        /// <summary>写入 challenge 缓存（先清除旧值，HybridCache 不支持直接覆盖）</summary>
        public async Task SetChallenge(string key, string optionsJson)
        {
            await _cache.RemoveAsync(key);
            await _cache.SetAsync(key, optionsJson, new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(_options.ChallengeExpiresInSeconds)
            });
        }

        /// <summary>取出 challenge 并立即作废（一次性使用），不存在时返回空串</summary>
        public async Task<string> TakeChallenge(string key)
        {
            var value = await _cache.GetOrCreateAsync(key, ct => new ValueTask<string>(string.Empty));
            await _cache.RemoveAsync(key);
            return value;
        }
    }
}
