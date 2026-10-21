using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.Buffers.Text;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using SystemAdmin.Common.EmailTemplates;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.SystemBasicMgmt.SystemAuth.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Repository.SystemBasicMgmt.SystemAuth;

namespace SystemAdmin.Service.SystemBasicMgmt.SystemAuth
{
    /// <summary>签发 Passkey 绑定链接并发邮件</summary>
    public class PasskeyBindMailService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<PasskeyBindMailService> _logger;
        private readonly SqlSugarScope _db;
        private readonly PasskeyRepository _passkeyRepo;
        private readonly MailKitEmailSender _email;
        private readonly AppUrlOptions _appUrl;
        private readonly PasskeyOptions _passkeyOptions;
        private readonly LocalizationService _localization;
        private readonly string _form = "FormBusiness.Forms.";

        public PasskeyBindMailService(CurrentUser loginuser, ILogger<PasskeyBindMailService> logger, SqlSugarScope db, PasskeyRepository passkeyRepo, MailKitEmailSender email, IOptions<AppUrlOptions> appUrl, IOptions<PasskeyOptions> passkeyOptions, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _passkeyRepo = passkeyRepo;
            _email = email;
            _appUrl = appUrl.Value;
            _passkeyOptions = passkeyOptions.Value;
            _localization = localization;
        }

        /// <summary>
        /// 发送 Passkey 绑定邮件，同时作废该用户旧的未使用链接
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<Result<int>> SendBindMail(UserInfoEntity user, string email)
        {
            try
            {
                email = email?.Trim() ?? string.Empty;

                // 只接受纯地址
                if (!MailAddress.TryCreate(email, out var mailAddress) || mailAddress.Address != email)
                {
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_form}PasskeyBindEmailInvalid"));
                }

                var now = DateTime.Now;

                // 限频
                var latest = await _passkeyRepo.GetLatestBindToken(user.UserId);
                if (latest != null && (now - latest.CreatedDate).TotalSeconds < _passkeyOptions.BindLinkMinIntervalSeconds)
                {
                    return Result<int>.Failure(429, _localization.ReturnMsg($"{_form}PasskeyBindTooFrequent"));
                }

                // 库中只存哈希，明文只在邮件链接里
                var rawToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
                var expiresAt = now.AddMinutes(_passkeyOptions.BindLinkExpiresInMinutes);
                var tokenEntity = new PasskeyBindTokenEntity
                {
                    BindTokenId = SnowFlakeSingle.Instance.NextId(),
                    UserId = user.UserId,
                    TokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken))),
                    Email = email,
                    ExpiresAt = expiresAt,
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = now
                };

                // 作废旧链接
                await _db.BeginTranAsync();
                await _passkeyRepo.RevokeActiveBindTokens(user.UserId);
                await _passkeyRepo.AddBindToken(tokenEntity);
                await _db.CommitTranAsync();

                try
                {
                    await _email.SendAsync(BuildPasskeyBindMail(user.NoticeLanguage, user.UserNameCn, user.UserNameEn, email, rawToken, expiresAt));
                }
                catch (Exception mailEx)
                {
                    // 邮件未发出，撤销链接
                    await _passkeyRepo.RevokeBindToken(tokenEntity.BindTokenId);
                    _logger.LogError(mailEx, mailEx.Message);
                    return Result<int>.Failure(500, _localization.ReturnMsg($"{_form}PasskeyBindMailFailed", (object)mailEx.Message));
                }

                return Result<int>.Ok(1, _localization.ReturnMsg($"{_form}PasskeyBindMailSent", (object)email));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 生成绑定邮件
        /// </summary>
        private EmailMessage BuildPasskeyBindMail(string noticeLanguage, string nameCn, string nameEn, string email, string rawToken, DateTime expiresAt)
        {
            var lang = string.IsNullOrWhiteSpace(noticeLanguage) ? "zh-CN" : noticeLanguage;
            var name = lang == "zh-CN" ? nameCn : nameEn;

            var baseUrl = _appUrl.BaseDomain.TrimEnd('/');
            var bindUrl = $"{baseUrl}/passkey-bind?token={Uri.EscapeDataString(rawToken)}";

            string Text(string key, params object[] args) =>
                WebUtility.HtmlEncode(_localization.ReturnMsg($"{_form}{key}", lang, args));

            var body = EmailTemplateLoader.GetPasskeyBindNotice()
                .Replace("{{Title}}", Text("PasskeyBindMailTitle"))
                .Replace("{{Greeting}}", Text("PasskeyBindMailGreeting", name ?? string.Empty))
                .Replace("{{Intro}}", Text("PasskeyBindMailIntro"))
                .Replace("{{BtnBind}}", Text("PasskeyBindMailButton"))
                .Replace("{{LinkHint}}", Text("PasskeyBindMailLinkHint"))
                .Replace("{{ExpireHint}}", Text("PasskeyBindMailExpireHint", expiresAt.ToString("yyyy-MM-dd HH:mm")))
                .Replace("{{IgnoreHint}}", Text("PasskeyBindMailIgnoreHint"))
                .Replace("{{FooterText}}", Text("PasskeyBindMailFooter"))
                .Replace("{{BindUrl}}", WebUtility.HtmlEncode(bindUrl));

            return new EmailMessage
            {
                To = new List<string> { email },
                Subject = _localization.ReturnMsg($"{_form}PasskeyBindMailSubject", lang),
                Body = body
            };
        }
    }
}
