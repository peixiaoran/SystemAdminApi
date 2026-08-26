using System.Globalization;
using Microsoft.Extensions.Logging;
using SqlSugar;
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
        private readonly string _this = "CustMat.RollingForecast.ForecastVersion";

        public ForecastVersionService(CurrentUser loginuser, ILogger<ForecastVersionService> logger, SqlSugarScope db, ForecastVersionRepository forecastVersionRepo, LocalizationService localization)
        {
            _loginuser = loginuser;
            _logger = logger;
            _db = db;
            _forecastVersionRepo = forecastVersionRepo;
            _localization = localization;
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
                await _db.BeginTranAsync();
                int count = await _forecastVersionRepo.UpdateForecastVersionStatus(long.Parse(versionId), status.ToEnumString(), _loginuser.UserId, DateTime.Now);
                await _db.CommitTranAsync();

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
