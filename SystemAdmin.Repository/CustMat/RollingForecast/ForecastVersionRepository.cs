using Mapster;
using SqlSugar;
using SystemAdmin.Common.Enums.CustMat;
using SystemAdmin.Common.Utilities;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.RollingForecast
{
    public class ForecastVersionRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        /// <summary>
        /// 预测版本状态所属字典类型
        /// </summary>
        private const string StatusDicType = "ForecastStatus";

        public ForecastVersionRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 新增预测版本
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertForecastVersion(ForecastVersionEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除预测版本
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<int> DeleteForecastVersion(long versionId)
        {
            return await _db.Deleteable<ForecastVersionEntity>()
                            .Where(version => version.VersionId == versionId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 将当前最新的预测版本标记清除
        /// </summary>
        /// <returns></returns>
        public async Task<int> ClearLatestForecastVersion()
        {
            return await _db.Updateable<ForecastVersionEntity>()
                            .SetColumns(version => new ForecastVersionEntity { IsLatest = 0 })
                            .Where(version => version.IsLatest == 1)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改预测版本
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateForecastVersion(ForecastVersionEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(version => new
                            {
                                version.VersionId,
                                version.IsLatest,
                                version.Status,
                                version.CreatedBy,
                                version.CreatedDate,
                            }).Where(version => version.VersionId == entity.VersionId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询状态不为锁定的预测版本编号（若存在多个，只取一个）
        /// </summary>
        /// <returns></returns>
        public async Task<string?> GetNonLockedVersionCode()
        {
            var lockStatus = ForecastVersionStatus.Lock.ToEnumString();
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .Where(version => version.Status != lockStatus)
                            .Select(version => version.VersionCode)
                            .FirstAsync();
        }

        /// <summary>
        /// 指定时间范围内是否已存在重叠的预测版本
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="excludeVersionId">修改时排除自身</param>
        /// <returns></returns>
        public async Task<bool> HasOverlappingForecastVersion(DateTime startDate, DateTime endDate, long? excludeVersionId)
        {
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .WhereIF(excludeVersionId.HasValue, version => version.VersionId != excludeVersionId!.Value)
                            // 区间重叠判断：现有记录的开始 <= 新记录的结束 且 现有记录的结束 >= 新记录的开始
                            .Where(version => version.StartDate <= endDate && version.EndDate >= startDate)
                            .AnyAsync();
        }

        /// <summary>
        /// 修改预测版本状态
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="status"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public async Task<int> UpdateForecastVersionStatus(long versionId, string status, long modifiedBy, DateTime modifiedDate)
        {
            return await _db.Updateable<ForecastVersionEntity>()
                            .SetColumns(version => new ForecastVersionEntity { Status = status, ModifiedBy = modifiedBy, ModifiedDate = modifiedDate })
                            .Where(version => version.VersionId == versionId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询在职业务人员的邮箱地址及邮件通知语言（业务人员关联用户信息表）
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesUserEmailDto>> GetSalesUserEmails()
        {
            return await _db.Queryable<SalesUserEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<UserInfoEntity>((salesUser, user) => salesUser.SalesUserId == user.UserId)
                            .Where((salesUser, user) => user.IsEmployed == 1 && !string.IsNullOrEmpty(user.Email))
                            .Select((salesUser, user) => new SalesUserEmailDto
                            {
                                Email = user.Email,
                                NoticeLanguage = user.NoticeLanguage,
                            })
                            .Distinct()
                            .ToListAsync();
        }

        /// <summary>
        /// 查询预测版本
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<ForecastVersionEntity?> GetForecastVersion(long versionId)
        {
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .Where(version => version.VersionId == versionId)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询指定开始时间之前最近的一个预测版本（上一周版本）
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public async Task<ForecastVersionEntity?> GetPreviousVersion(DateTime startDate)
        {
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .Where(version => version.StartDate < startDate)
                            .OrderByDescending(version => version.StartDate)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询全部业务人员负责的公司料号（含所属业务人员），用于最新版本归档
        /// </summary>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetAllSalesPartNumbers()
        {
            return await _db.Queryable<NumberAssignEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CompanyNumberEntity>((numberAssign, companyNumber) => numberAssign.PartNumber == companyNumber.PartNumber)
                            .InnerJoin<UserInfoEntity>((numberAssign, companyNumber, user) => numberAssign.SalesUserId == user.UserId)
                            .Where((numberAssign, companyNumber) => companyNumber.Status == 1)
                            .OrderBy((numberAssign, companyNumber, user) => new { user.UserNo, companyNumber.PartNumber })
                            .Select((numberAssign, companyNumber, user) => new FoWeeklyRowDto
                            {
                                PartNumber = companyNumber.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                SalesUserId = numberAssign.SalesUserId,
                                SalesUserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询指定版本下全部业务人员导入时的公司料号（含所属业务人员），用于非最新版本归档
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetAllImportedPartNumbers(long versionId)
        {
            // SELECT DISTINCT 时 ORDER BY 列必须与 SELECT 列表中的列完全一致，因此按 detail 表的列排序
            return await _db.Queryable<ForecastWeeklyDetailEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CompanyNumberEntity>((detail, companyNumber) => detail.PartNumber == companyNumber.PartNumber)
                            .InnerJoin<UserInfoEntity>((detail, companyNumber, user) => detail.SalesUserId == user.UserId)
                            .Where((detail, companyNumber) => detail.VersionId == versionId && companyNumber.Status == 1)
                            .OrderBy((detail, companyNumber, user) => new { detail.SalesUserId, detail.PartNumber })
                            .Select((detail, companyNumber, user) => new FoWeeklyRowDto
                            {
                                PartNumber = detail.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                SalesUserId = detail.SalesUserId,
                                SalesUserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
                            })
                            .Distinct()
                            .ToListAsync();
        }

        /// <summary>
        /// 查询指定版本下给定公司料号的预测周明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<ForecastWeeklyDetailEntity>> GetForecastWeeklyDetails(long versionId, List<string> partNumbers)
        {
            return await _db.Queryable<ForecastWeeklyDetailEntity>()
                            .With(SqlWith.NoLock)
                            .Where(detail => detail.VersionId == versionId && partNumbers.Contains(detail.PartNumber))
                            .ToListAsync();
        }

        /// <summary>
        /// 清空指定版本下的预测周明细归档（重新锁定时覆盖）
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<int> DeleteForecastWeeklyArchives(long versionId)
        {
            return await _db.Deleteable<ForecastWeeklyArchiveEntity>()
                            .Where(archive => archive.VersionId == versionId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量新增预测周明细归档
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<int> InsertForecastWeeklyArchiveList(List<ForecastWeeklyArchiveEntity> list)
        {
            return await _db.Insertable(list).ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询预测版本实体
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async Task<ForecastVersionDto> GetForecastVersionEntity(long versionId)
        {
            var entity = await _db.Queryable<ForecastVersionEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(version => version.VersionId == versionId)
                                  .FirstAsync();
            return entity.Adapt<ForecastVersionDto>();
        }

        /// <summary>
        /// 查询预测版本分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<ForecastVersionDto>> GetForecastVersionPage(GetForecastVersionPage getPage)
        {
            var query = _db.Queryable<ForecastVersionEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<DictionaryInfoEntity>((version, statusDic) => statusDic.DicType == StatusDicType && version.Status == statusDic.DicCode);

            // 版本编号
            if (!string.IsNullOrEmpty(getPage.VersionCode))
            {
                query = query.Where(version => version.VersionCode.Contains(getPage.VersionCode));
            }

            RefAsync<int> totalCount = 0;
            var page = await query.OrderByDescending(version => version.StartDate)
                                  .Select((version, statusDic) => new ForecastVersionDto
                                  {
                                      VersionId = version.VersionId,
                                      VersionCode = version.VersionCode,
                                      StartDate = version.StartDate,
                                      EndDate = version.EndDate,
                                      Year = version.Year,
                                      Month = version.Month,
                                      Week = version.Week,
                                      IsLatest = version.IsLatest,
                                      Status = version.Status,
                                      StatusName = _lang.Locale == "zh-CN" ? statusDic.DicNameCn : statusDic.DicNameEn,
                                  }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<ForecastVersionDto>.Ok(page, totalCount, "");
        }
    }
}
