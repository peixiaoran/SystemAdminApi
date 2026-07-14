using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SystemAdmin.Common.Enums.SystemBasicMgmt;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Options;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Commands;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Dto;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using SystemAdmin.Repository.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.Service.SystemBasicMgmt.SystemAuth
{
    public class SysUserOperateService
    {
        private readonly ILogger<SysUserOperateService> _logger;
        private readonly CurrentUser _loginuser;
        private readonly JwtTokenService _jwt;
        private readonly SqlSugarScope _db;
        private readonly SysUserOperateRepository _sysUserOperateRepo;
        private readonly MailKitEmailSender _email;
        private readonly LocalizationService _localization;
        private readonly HybridCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _this = "SystemBasicMgmt.SystemAuth.SysUserOperate";
        private const int CodeLength = 6;
        private static readonly Random _random = new Random();

        public SysUserOperateService(CurrentUser loginuser, JwtTokenService jwt, ILogger<SysUserOperateService> logger, SqlSugarScope db, SysUserOperateRepository sysUserOperateRepo, MailKitEmailSender email, LocalizationService localization, HybridCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _loginuser = loginuser;
            _jwt = jwt;
            _logger = logger;
            _db = db;
            _sysUserOperateRepo = sysUserOperateRepo;
            _email = email;
            _localization = localization;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="httpResponse"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public async Task<Result<SysUserLoginReturnDto>> UserLogin(HttpResponse httpResponse, UserLogin login)
        {
            try
            {
                var ip = GetClientIp();
                var nowTime = DateTime.Now;

                // 查询用户
                var user = await _sysUserOperateRepo.GetUserInfo(login);

                if (user == null)
                {
                    await _db.BeginTranAsync();
                    // 用户不存在
                    await _sysUserOperateRepo.AddUserLoginLogInfo(new UserLogOutEntity
                    {
                        UserId = 0,
                        LoginType = LoginBehavior.AccountNotExist.ToEnumString(),
                        IP = ip,
                        LoginDate = nowTime
                    });
                    await _db.CommitTranAsync();

                    return Result<SysUserLoginReturnDto>.Failure(406, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                // 账号已冻结
                if (user.IsFreeze != 0)
                {
                    await _db.CommitTranAsync();
                    return Result<SysUserLoginReturnDto>.Failure(401, _localization.ReturnMsg($"{_this}LoginFreeze"));
                }

                // 校验密码
                var inputHash = HashPasswordWithArgon2id(login.PassWord,Convert.FromBase64String(user.PwdSalt));

                if (inputHash != user.PassWord)
                {
                    // 记录密码错误
                    await _sysUserOperateRepo.AddUserLoginLogInfo(new UserLogOutEntity
                    {
                        UserId = user.UserId,
                        LoginType = LoginBehavior.IncorrectPassword.ToEnumString(),
                        IP = ip,
                        LoginDate = nowTime
                    });

                    // 获取并累加错误次数
                    var lockInfo = await _sysUserOperateRepo.GetUserLockErrorNumberNow(user.UserId);
                    var newErrors = (lockInfo?.NumberErrors ?? 0) + 1;

                    if (lockInfo == null)
                    {
                        await _sysUserOperateRepo.AddUserLock(new UserLockEntity
                        {
                            UserId = user.UserId,
                            NumberErrors = 1,
                            CreatedDate = nowTime
                        });
                    }
                    else
                    {
                        await _sysUserOperateRepo.AutoUserLockErrorNumber(user.UserId, newErrors);
                    }

                    // 达到阈值 → 冻结账号
                    if (newErrors >= 5)
                    {
                        await _sysUserOperateRepo.UpdateUserFreeze(user.UserId);
                        await _db.CommitTranAsync();

                        return Result<SysUserLoginReturnDto>.Failure(402, _localization.ReturnMsg($"{_this}LoginLock"));
                    }

                    // 未达阈值
                    var remain = 5 - newErrors;
                    await _db.CommitTranAsync();

                    return Result<SysUserLoginReturnDto>.Failure(403, _localization.ReturnMsg($"{_this}LoginFailedRemainTimes", remain));
                }

                // 密码是否过期
                if (user.ExpirationTime < nowTime)
                {
                    await _db.CommitTranAsync();
                    return Result<SysUserLoginReturnDto>.Failure(405, _localization.ReturnMsg($"{_this}PasswordExpiration"));
                }

                // 登录成功日志
                await _sysUserOperateRepo.AddUserLoginLogInfo(new UserLogOutEntity
                {
                    UserId = user.UserId,
                    LoginType = LoginBehavior.LoginSuccessful.ToEnumString(),
                    IP = ip,
                    LoginDate = nowTime
                });

                // 清空锁定记录
                await _sysUserOperateRepo.EmptyUserLock(user.UserId);
                await _db.CommitTranAsync();

                _jwt.SetAuthCookie(httpResponse, userId: user.UserId, userNo: user.UserNo);

                // 返回登录成功信息（JWT Cookie 已在其他层处理）
                return Result<SysUserLoginReturnDto>.Ok(
                    new SysUserLoginReturnDto
                    {
                        UserNo = user.UserNo,
                        UserNameCn = user.UserNameCn,
                        UserNameEn = user.UserNameEn,
                        AvatarAddress = user.AvatarAddress
                    },
                    _localization.ReturnMsg($"{_this}LoginSuccessful")
                );
            }
            catch(Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<SysUserLoginReturnDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 用户登出
        /// </summary>
        /// <returns></returns>
        public async Task<Result<int>> UserLogOut()
        {
            try
            {
                var user = await _sysUserOperateRepo.GetUserInfoForUserLogOut(_loginuser.UserId);

                // 判断用户是否存在
                if (user == null)
                {
                    return Result<int>.Failure(406, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                await _db.BeginTranAsync();
                var logOutLog = new UserLogOutEntity
                {
                    UserId = user.UserId,
                    LoginType = LoginBehavior.LoggedOut.ToEnumString(),
                    IP = GetClientIp(),
                    LoginDate = DateTime.Now
                };
                var insertLogOutCount = await _sysUserOperateRepo.AddUserLogOutInfo(logOutLog);
                await _db.CommitTranAsync();

                return Result<int>.Ok(insertLogOutCount, _localization.ReturnMsg($"{_this}LogOutSuccess"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 解锁账号发送验证码
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public async Task<Result<string>> UnLockSendCode(string userNo)
        {
            try
            {
                var user = await _sysUserOperateRepo.GetUserInfo(userNo);

                // 判断用户是否在职
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}EmailNotFound"));
                }

                // 判断用户是否被冻结
                if (user.IsFreeze == 0)
                {
                    return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}UserNotFreeze"));
                }

                // 先清除旧缓存（HybridCache 不支持直接覆盖）
                await _cache.RemoveAsync(userNo); 

                // 正常发送验证码流程
                var code = GenerateRandomCode();
                await _cache.SetAsync(userNo, code);

                // 发送邮件
                EmailMessage emailMsg = new EmailMessage
                {
                    To = new List<string> { user.Email },
                    Subject = _localization.ReturnMsg($"{_this}EmailAccountUnlock"),
                    Body = _localization.ReturnMsg($"{_this}UnlockSendVcCodeBody", code)
                };
                await _email.SendAsync(emailMsg);

                return Result<string>.Ok("1", _localization.ReturnMsg($"{_this}SendVcCodeSuccess"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}SendVcCodeFailed", ex.Message));
            }
        }

        /// <summary>
        /// 账号解锁
        /// </summary>
        /// <param name="unlock"></param>
        /// <returns></returns>
        public async Task<Result<int>> UserUnLock(UserUnlock unlock)
        {
            try
            {
                var cacheCode = await _cache.GetOrCreateAsync(
                      unlock.UserNo,
                      ct => new ValueTask<string>("")
                );
                if (string.IsNullOrEmpty(unlock.VerificationCode))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VcCodeExpired"));
                }

                if (!string.Equals(cacheCode, unlock.VerificationCode))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VcCodeError"));
                }

                var user = await _sysUserOperateRepo.GetUserInfo(unlock.UserNo);
                if (user == null)
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                // 验证密码格式
                if (!ValidatePassword(unlock.PassWord))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}ValidationPassWrodError"));
                }

                // 密码不能与老密码相同
                await _db.BeginTranAsync();
                var oldHash = HashPasswordWithArgon2id(unlock.PassWord, Convert.FromBase64String(user.PwdSalt));
                if (oldHash == user.PassWord)
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}NotEqualOldPassWord"));
                }
                else
                {
                    // 加密密码
                    byte[] salt = GenerateSalt();
                    string saltString = Convert.ToBase64String(salt);
                    string passwordHash = HashPasswordWithArgon2id(unlock.PassWord, salt);

                    int unlockFreezeCount = await _sysUserOperateRepo.UnlockUserFreeze(user.UserId, passwordHash, saltString, DateTime.Now.AddDays(user.ExpirationDays));
                    await _sysUserOperateRepo.DeleleUserLockLog(user.UserId);
                    await _db.CommitTranAsync();

                    await _cache.RemoveAsync(unlock.UserNo); // 验证通过，清除缓存

                    return unlockFreezeCount >= 1
                            ? Result<int>.Ok(unlockFreezeCount, _localization.ReturnMsg($"{_this}UnlockSuccess"))
                            : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UnlockFailed"));
                }
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 密码过期发送验证码
        /// </summary>
        /// <param name="userNo"></param>
        /// <returns></returns>
        public async Task<Result<string>> UnExpirationSendCode(string userNo)
        {
            try
            {
                var user = await _sysUserOperateRepo.GetUserInfo(userNo);

                // 判断用户是否在职
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}EmailNotFound"));
                }

                // 判断账号密码是否过期
                if (user.ExpirationTime > DateTime.Now)
                {
                    return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}PasswordNotExpiration"));
                }

                await _cache.RemoveAsync(userNo); // 先清除旧缓存（HybridCache 不支持直接覆盖）

                // 正常发送验证码流程
                var code = GenerateRandomCode();
                await _cache.SetAsync(userNo, code);

                // 发送邮件
                EmailMessage emailMsg = new EmailMessage
                {
                    To = new List<string> { user.Email },
                    Subject = _localization.ReturnMsg($"{_this}EmailAccountPasswordEx"),
                    Body = _localization.ReturnMsg($"{_this}PasswordExSendVcCodeBody", code)
                };
                await _email.SendAsync(emailMsg);

                return Result<string>.Ok("SendVcCode Success", _localization.ReturnMsg($"{_this}SendVcCodeSuccess"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<string>.Failure(500, _localization.ReturnMsg($"{_this}SendVcCodeFailed", ex.Message));
            }
        }

        /// <summary>
        /// 密码过期重置
        /// </summary>
        /// <param name="expiration"></param>
        /// <returns></returns>
        public async Task<Result<int>> UserPwdExpiration(PwdExpiration expiration)
        {
            try
            {
                // 查询用户信息
                var user = await _sysUserOperateRepo.GetUserInfo(expiration.UserNo);

                // 判断用户是否在职
                if (user == null)
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UserNotFound"));
                }

                // 获取缓存中的验证码（注意不是创建）
                // 查询存入缓存中的验证码
                var cachedCode = await _cache.GetOrCreateAsync(
                    expiration.UserNo,
                    ct => new ValueTask<string>("")
                );

                // 如果缓存中没有验证码，说明验证码已过期或未发送
                if (string.IsNullOrEmpty(expiration.VerificationCode))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VcCodeExpired"));
                }

                // 验证验证码是否匹配
                if (!string.Equals(cachedCode, expiration.VerificationCode))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}VcCodeError"));
                }

                // 验证密码格式
                if (!ValidatePassword(expiration.PassWord))
                {
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}ValidationPassWrodError"));
                }

                // 密码不能与老密码相同
                await _db.BeginTranAsync();
                var oldHash = HashPasswordWithArgon2id(expiration.PassWord, Convert.FromBase64String(user.PwdSalt));
                if (oldHash == user.PassWord)
                {
                    await _db.RollbackTranAsync();
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_this}NotEqualOldPassWord"));
                }
                else
                {
                    // 加密密码
                    byte[] salt = GenerateSalt();
                    string saltString = Convert.ToBase64String(salt);
                    string passwordHash = HashPasswordWithArgon2id(expiration.PassWord, salt);

                    // 更新用户密码
                    int count = await _sysUserOperateRepo.PwdExpirationUpdate(user.UserId, passwordHash, saltString, DateTime.Now.AddDays(user.ExpirationDays));
                    // 清空用户锁定记录
                    await _sysUserOperateRepo.EmptyUserLock(user.UserId);
                    await _db.CommitTranAsync();

                    // 清除验证码缓存
                    await _cache.RemoveAsync(expiration.UserNo);

                    return count >= 1
                            ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}UpdatePassWrodSuccess"))
                            : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UpdatePassWrodFailed"));
                }
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 生成6位随机验证码
        /// </summary>
        /// <returns></returns>
        private static string GenerateRandomCode()
        {
            return string.Create(CodeLength, _random, (span, r) =>
            {
                for (int i = 0; i < span.Length; i++)
                {
                    span[i] = (char)(r.Next(0, 10) + '0');
                }
            });
        }

        /// <summary>
        /// 获取客户端真实IP地址
        /// </summary>
        public string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "127.0.0.1";

            // 1. 优先取 X-Forwarded-For(可能是多级代理链: "客户端IP, 代理1, 代理2")
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                foreach (var segment in forwardedFor.Split(','))
                {
                    var candidate = segment.Trim();

                    // 可能带端口,如 "1.2.3.4:5678",去掉端口(仅单冒号时,避免误伤 IPv6)
                    if (candidate.Count(c => c == ':') == 1)
                        candidate = candidate.Split(':')[0];

                    if (IPAddress.TryParse(candidate, out var parsed))
                    {
                        if (parsed.IsIPv4MappedToIPv6)
                            parsed = parsed.MapToIPv4();
                        return parsed.ToString();
                    }
                }
            }

            // 2. 其次取 X-Real-IP(Nginx 常用)
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp) && IPAddress.TryParse(realIp.Trim(), out var real))
            {
                if (real.IsIPv4MappedToIPv6)
                    real = real.MapToIPv4();
                return real.ToString();
            }

            // 3. 最后回退到 TCP 连接的远端地址
            var remoteIp = context.Connection.RemoteIpAddress;
            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6)
                    remoteIp = remoteIp.MapToIPv4();
                return remoteIp.ToString();
            }

            return "127.0.0.1";
        }

        /// <summary>
        /// 验证密码是否符合规范（必须为8-16位，包含小写、大写、数字和特殊字符）
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8 || password.Length > 16)
            {
                return false;
            }
            return Regex.IsMatch(password, @"(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9])");
        }

        /// <summary>
        /// 生成安全随机盐（16字节）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] GenerateSalt(int length = 16)
        {
            byte[] salt = new byte[length];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        /// <summary>
        /// 用户密码加密（Argon2Id）
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string HashPasswordWithArgon2id(string password, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            try
            {
                using var argon2 = new Argon2id(passwordBytes)
                {
                    Salt = salt,
                    DegreeOfParallelism = 4, // 固定并行度，而不是使用 Environment.ProcessorCount
                    MemorySize = 65536,      // 固定内存大小(64KB)
                    Iterations = 3           // 固定迭代次数
                };

                byte[] hash = argon2.GetBytes(32); // 256位 hash
                return Convert.ToBase64String(hash);
            }
            finally
            {
                // 安全擦除密码内存
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }
        }
    }
}
