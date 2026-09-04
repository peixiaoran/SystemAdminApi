using System.Globalization;
using System.Net;
using System.Text.Json;
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

                // 锁定时按业务人员归档周明细，归档失败则连同锁定一起回滚
                if (count >= 1 && status == ForecastVersionStatus.Lock)
                {
                    await ArchiveForecastWeeklyDetails(id);
                }
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
        /// 锁定版本时，把每个业务人员的周明细（与 GetFoWeeklyDetail 返回结构一致）序列化为JSON归档到 ForecastWeeklyArchive
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        private async Task ArchiveForecastWeeklyDetails(long versionId)
        {
            var version = await _forecastVersionRepo.GetForecastVersion(versionId);
            if (version == null)
                return;

            var periods = BuildPeriods(version.StartDate.Date);

            // 最新版本按人员料号对照查询，非最新版本直接取当时导入数据中的公司料号
            var rows = version.IsLatest == 1
                ? await _forecastVersionRepo.GetAllSalesPartNumbers()
                : await _forecastVersionRepo.GetAllImportedPartNumbers(version.VersionId);

            // 重新锁定时覆盖旧归档
            await _forecastVersionRepo.DeleteForecastWeeklyArchives(version.VersionId);
            if (rows.Count == 0)
                return;

            foreach (var row in rows)
            {
                row.Quantities = periods.ToDictionary(period => period.PeriodKey, period => 0m);
            }

            var partNumbers = rows.Select(row => row.PartNumber).ToList();
            var periodKeyOfDate = periods.ToDictionary(period => period.StartDate, period => period.PeriodKey);
            var rowOfPartNumber = rows.ToDictionary(row => row.PartNumber);

            var details = await _forecastVersionRepo.GetForecastWeeklyDetails(version.VersionId, partNumbers);
            foreach (var detail in details)
            {
                if (!rowOfPartNumber.TryGetValue(detail.PartNumber, out var row))
                    continue;
                if (!periodKeyOfDate.TryGetValue(detail.HorizonDays.Date, out var periodKey))
                    continue;
                row.Quantities[periodKey] += detail.Qty;
            }

            await FillTotalsAndChangeRates(version, periods, rows);

            var now = DateTime.Now;
            var archives = rows.GroupBy(row => row.SalesUserId ?? 0)
                               .Select(group => new ForecastWeeklyArchiveEntity
                               {
                                   VersionId = version.VersionId,
                                   SalesUserId = group.Key,
                                   ForecastDetail = JsonSerializer.Serialize(new FoWeeklyDetailDto
                                   {
                                       VersionId = version.VersionId,
                                       VersionCode = version.VersionCode,
                                       StartDate = version.StartDate.Date,
                                       Periods = periods,
                                       Rows = [.. group],
                                   }),
                                   CreatedDate = now,
                               }).ToList();

            await _forecastVersionRepo.InsertForecastWeeklyArchiveList(archives);
        }

        /// <summary>
        /// 按料号填充天/周数量合计，以及环比上周的变化百分比（保留2位小数，上周数量为0时为空）
        /// </summary>
        /// <param name="version"></param>
        /// <param name="periods"></param>
        /// <param name="rows"></param>
        private async Task FillTotalsAndChangeRates(ForecastVersionEntity version, List<FoWeeklyPeriodDto> periods, List<FoWeeklyRowDto> rows)
        {
            var dayType = ForecastPeriodType.Day.ToEnumString();
            var weekType = ForecastPeriodType.Week.ToEnumString();
            var dayKeys = periods.Where(period => period.PeriodType == dayType).Select(period => period.PeriodKey).ToHashSet();
            var weekKeys = periods.Where(period => period.PeriodType == weekType).Select(period => period.PeriodKey).ToHashSet();

            foreach (var row in rows)
            {
                row.DayTotal = dayKeys.Sum(key => row.Quantities[key]);
                row.WeekTotal = weekKeys.Sum(key => row.Quantities[key]);
            }

            var previousVersion = await _forecastVersionRepo.GetPreviousVersion(version.StartDate);
            if (previousVersion == null)
                return;

            var previousDetails = await _forecastVersionRepo.GetForecastWeeklyDetails(previousVersion.VersionId, [.. rows.Select(row => row.PartNumber)]);
            var previousQtyOfPartNumber = previousDetails
                .GroupBy(detail => (detail.PartNumber, detail.PeriodType))
                .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Qty));

            foreach (var row in rows)
            {
                var previousDayQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, dayType), 0m);
                var previousWeekQty = previousQtyOfPartNumber.GetValueOrDefault((row.PartNumber, weekType), 0m);

                row.DayQtyChangeRate = previousDayQty == 0 ? null : Math.Round((row.DayTotal - previousDayQty) / previousDayQty * 100, 2);
                row.WeekQtyChangeRate = previousWeekQty == 0 ? null : Math.Round((row.WeekTotal - previousWeekQty) / previousWeekQty * 100, 2);
            }
        }

        /// <summary>
        /// 生成预测周明细的列，前21列按天，之后13列按周
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        private static List<FoWeeklyPeriodDto> BuildPeriods(DateTime startDate)
        {
            const int dayCount = 21;
            const int weekCount = 13;
            var periods = new List<FoWeeklyPeriodDto>(dayCount + weekCount);

            for (int i = 0; i < dayCount; i++)
            {
                periods.Add(new FoWeeklyPeriodDto
                {
                    PeriodKey = $"D{i + 1}",
                    PeriodType = ForecastPeriodType.Day.ToEnumString(),
                    StartDate = startDate.AddDays(i),
                });
            }

            // 按天的部分结束后紧接着按周，版本开始日期为周一，因此每周仍以周一起算
            var weekStartDate = startDate.AddDays(dayCount);
            for (int i = 0; i < weekCount; i++)
            {
                periods.Add(new FoWeeklyPeriodDto
                {
                    PeriodKey = $"W{i + 1}",
                    PeriodType = ForecastPeriodType.Week.ToEnumString(),
                    StartDate = weekStartDate.AddDays(i * 7),
                });
            }

            return periods;
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
