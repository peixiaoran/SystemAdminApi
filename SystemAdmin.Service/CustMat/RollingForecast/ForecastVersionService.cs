using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using SqlSugar;
using SystemAdmin.Common.EmailTemplates;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.RollingForecast.Commands;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Repository.CustMat.RollingForecast;

namespace SystemAdmin.Service.CustMat.RollingForecast
{
    public class ForecastVersionService
    {
        private readonly CurrentUser _loginuser;
        private readonly ILogger<ForecastVersionService> _logger;
        private readonly SqlSugarScope _db;
        private readonly ForecastVersionRepository _forecastVersionRepo;
        private readonly LocalizationService _localization;
        private readonly MailKitEmailSender _email;
        private readonly string _this = "CustMat.RollingForecast.ForecastVersion";

        public ForecastVersionService(CurrentUser loginuser, ILogger<ForecastVersionService> logger, SqlSugarScope db, ForecastVersionRepository forecastVersionRepo, LocalizationService localization, MailKitEmailSender email)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _forecastVersionRepo = forecastVersionRepo;
            _localization = localization;
            _email = email;
        }

        /// <summary>
        /// 新增预测版本
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> InsertForecastVersion(ForecastVersionUpsert upsert)
        {
            try
            {
                var nonLockedVersionCode = await _forecastVersionRepo.GetNonLockedVersionCode();
                if (!string.IsNullOrEmpty(nonLockedVersionCode))
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}HasUnlocked", (object)nonLockedVersionCode));

                var endDate = EndOfDay(upsert.EndDate);
                var overlapping = await _forecastVersionRepo.HasOverlappingForecastVersion(upsert.StartDate, endDate, null);
                if (overlapping)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}Overlap", (object)upsert.VersionCode));

                var entity = new ForecastVersionEntity()
                {
                    VersionId = SnowFlakeSingle.Instance.NextId(),
                    VersionCode = upsert.VersionCode,
                    StartDate = upsert.StartDate,
                    EndDate = endDate,
                    Year = upsert.StartDate.Year,
                    Month = upsert.StartDate.Month,
                    Week = ISOWeek.GetWeekOfYear(upsert.StartDate),
                    IsLatest = 1,
                    Status = ForecastVersionStatus.Preparation.ToEnumString(),
                    CreatedBy = _loginuser.UserId,
                    CreatedDate = DateTime.Now,
                };

                await _db.BeginTranAsync();
                // 新增版本前，将其余版本的最新标记清除
                await _forecastVersionRepo.ClearLatestForecastVersion();
                int count = await _forecastVersionRepo.InsertForecastVersion(entity);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}InsertSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}InsertFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 修改预测版本
        /// </summary>
        /// <param name="upsert"></param>
        /// <returns></returns>
        public async Task<Result<int>> UpdateForecastVersion(ForecastVersionUpsert upsert)
        {
            try
            {
                var versionId = long.Parse(upsert.VersionId);
                var endDate = EndOfDay(upsert.EndDate);
                var overlapping = await _forecastVersionRepo.HasOverlappingForecastVersion(upsert.StartDate, endDate, versionId);
                if (overlapping)
                    return Result<int>.Failure(400, _localization.ReturnMsg($"{_this}Overlap", (object)upsert.VersionCode));

                var entity = new ForecastVersionEntity()
                {
                    VersionId = versionId,
                    VersionCode = upsert.VersionCode,
                    StartDate = upsert.StartDate,
                    EndDate = endDate,
                    Year = upsert.StartDate.Year,
                    Month = upsert.StartDate.Month,
                    Week = ISOWeek.GetWeekOfYear(upsert.StartDate),
                    ModifiedBy = _loginuser.UserId,
                    ModifiedDate = DateTime.Now,
                };

                await _db.BeginTranAsync();
                int count = await _forecastVersionRepo.UpdateForecastVersion(entity);
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}UpdateSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}UpdateFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 解锁预测版本
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public Task<Result<int>> UnlockForecastVersion(string versionId)
        {
            return ChangeForecastVersionStatus(versionId, ForecastVersionStatus.Unlock, $"{_this}UnlockSuccess", $"{_this}UnlockFailed");
        }

        /// <summary>
        /// 锁定预测版本
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public Task<Result<int>> LockForecastVersion(string versionId)
        {
            return ChangeForecastVersionStatus(versionId, ForecastVersionStatus.Lock, $"{_this}LockSuccess", $"{_this}LockFailed");
        }

        /// <summary>
        /// 修改预测版本状态
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="status"></param>
        /// <param name="successKey"></param>
        /// <param name="failureKey"></param>
        /// <returns></returns>
        private async Task<Result<int>> ChangeForecastVersionStatus(string versionId, ForecastVersionStatus status, string successKey, string failureKey)
        {
            try
            {
                var id = long.Parse(versionId);
                await _db.BeginTranAsync();
                int count = await _forecastVersionRepo.UpdateForecastVersionStatus(id, status.ToEnumString(), _loginuser.UserId, DateTime.Now);
                await _db.CommitTranAsync();

                if (count >= 1)
                {
                    await NotifySalesUsersByStatusChange(id, status);
                }

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg(successKey))
                        : Result<int>.Failure(500, _localization.ReturnMsg(failureKey));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 预测版本解锁/锁定后，邮件通知业务人员
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private async Task NotifySalesUsersByStatusChange(long versionId, ForecastVersionStatus status)
        {
            try
            {
                var version = await _forecastVersionRepo.GetForecastVersionEntity(versionId);
                if (version == null || string.IsNullOrEmpty(version.VersionCode))
                    return;

                var recipients = await _forecastVersionRepo.GetSalesUserEmails();
                if (recipients.Count == 0)
                    return;

                var dateRange = $"{version.StartDate:yyyy-MM-dd} ~ {version.EndDate:yyyy-MM-dd}";
                var subjectKey = status == ForecastVersionStatus.Unlock ? $"{_this}UnlockEmailSubject" : $"{_this}LockEmailSubject";
                var bodyKey = status == ForecastVersionStatus.Unlock ? $"{_this}UnlockEmailBody" : $"{_this}LockEmailBody";
                var template = EmailTemplateLoader.GetForecastVersionNotice();

                // 按语言缓存已渲染好的邮件内容，避免同语言用户重复渲染
                var renderedByLanguage = new Dictionary<string, (string Subject, string Html)>();

                foreach (var recipient in recipients)
                {
                    var lang = string.IsNullOrWhiteSpace(recipient.NoticeLanguage) ? "zh-CN" : recipient.NoticeLanguage;

                    if (!renderedByLanguage.TryGetValue(lang, out var rendered))
                    {
                        var subject = _localization.ReturnMsg(subjectKey, lang, version.VersionCode);
                        var greeting = _localization.ReturnMsg($"{_this}EmailVersionRange", lang, version.VersionCode, dateRange);
                        var message = WebUtility.HtmlEncode(_localization.ReturnMsg(bodyKey, lang, version.VersionCode, dateRange));
                        var footer = _localization.ReturnMsg($"{_this}EmailFooter", lang);

                        var html = template.Replace("{{Title}}", WebUtility.HtmlEncode(subject))
                                            .Replace("{{Greeting}}", WebUtility.HtmlEncode(greeting))
                                            .Replace("{{Message}}", message)
                                            .Replace("{{FooterText}}", WebUtility.HtmlEncode(footer));

                        rendered = (subject, html);
                        renderedByLanguage[lang] = rendered;
                    }

                    var emailMsg = new EmailMessage
                    {
                        To = new List<string> { recipient.Email },
                        Subject = rendered.Subject,
                        Body = rendered.Html,
                        IsHtml = true,
                    };
                    await _email.SendAsync(emailMsg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        /// <summary>
        /// 取指定日期当天的最后一刻（23:59:59）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static DateTime EndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// 删除预测版本
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<Result<int>> DeleteForecastVersion(string versionId)
        {
            try
            {
                await _db.BeginTranAsync();
                int count = await _forecastVersionRepo.DeleteForecastVersion(long.Parse(versionId));
                await _db.CommitTranAsync();

                return count >= 1
                        ? Result<int>.Ok(count, _localization.ReturnMsg($"{_this}DeleteSuccess"))
                        : Result<int>.Failure(500, _localization.ReturnMsg($"{_this}DeleteFailed"));
            }
            catch (Exception ex)
            {
                await _db.RollbackTranAsync();
                _logger.LogError(ex, ex.Message);
                return Result<int>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询预测版本实体
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<Result<ForecastVersionDto>> GetForecastVersionEntity(string versionId)
        {
            try
            {
                var entity = await _forecastVersionRepo.GetForecastVersionEntity(long.Parse(versionId));
                return Result<ForecastVersionDto>.Ok(entity, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result<ForecastVersionDto>.Failure(500, ex.Message);
            }
        }

        /// <summary>
        /// 查询预测版本分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage(GetForecastVersionPage getPage)
        {
            try
            {
                return await _forecastVersionRepo.GetForecastVersionPage(getPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResultPaged<ForecastVersionDto>.Failure(500, ex.Message);
            }
        }
    }
}
