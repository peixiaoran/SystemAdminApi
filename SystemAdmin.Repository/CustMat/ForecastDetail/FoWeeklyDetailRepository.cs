using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.ForecastDetail
{
    public class FoWeeklyDetailRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        /// <summary>
        /// 预测版本状态所属字典类型
        /// </summary>
        private const string StatusDicType = "ForecastStatus";

        public FoWeeklyDetailRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
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
        /// 查询指定版本下全部（或指定业务人员）的预测周明细归档
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId">为空时查询全部业务人员</param>
        /// <returns></returns>
        public async Task<List<ForecastWeeklyArchiveEntity>> GetForecastWeeklyArchives(long versionId, long? salesUserId)
        {
            return await _db.Queryable<ForecastWeeklyArchiveEntity>()
                            .With(SqlWith.NoLock)
                            .Where(archive => archive.VersionId == versionId)
                            .WhereIF(salesUserId.HasValue, archive => archive.SalesUserId == salesUserId!.Value)
                            .OrderBy(archive => archive.SalesUserId)
                            .ToListAsync();
        }

        /// <summary>
        /// 业务人员下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<SalesUserDropDto>> GetSalesUserDrop()
        {
            return await _db.Queryable<SalesUserEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<UserInfoEntity>((salesUser, user) => salesUser.SalesUserId == user.UserId)
                            .OrderBy((salesUser, user) => user.UserNo)
                            .Select((salesUser, user) => new SalesUserDropDto
                            {
                                SalesUserId = salesUser.SalesUserId,
                                UserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询全部（或指定业务人员）负责的公司料号，用于最新版本
        /// </summary>
        /// <param name="salesUserId">为空时查询全部业务人员</param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetAllSalesPartNumbers(long? salesUserId)
        {
            var query = _db.Queryable<NumberAssignEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<CompanyNumberEntity>((numberAssign, companyNumber) => numberAssign.PartNumber == companyNumber.PartNumber)
                           .InnerJoin<UserInfoEntity>((numberAssign, companyNumber, user) => numberAssign.SalesUserId == user.UserId)
                           .Where((numberAssign, companyNumber) => companyNumber.Status == 1);

            if (salesUserId.HasValue)
            {
                query = query.Where(numberAssign => numberAssign.SalesUserId == salesUserId.Value);
            }

            return await query.OrderBy((numberAssign, companyNumber, user) => new { user.UserNo, companyNumber.PartNumber })
                              .Select((numberAssign, companyNumber, user) => new FoWeeklyRowDto
                              {
                                  PartNumber = companyNumber.PartNumber,
                                  PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                  SalesUserId = numberAssign.SalesUserId,
                                  SalesUserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
                              }).ToListAsync();
        }

        /// <summary>
        /// 查询指定版本下指定业务人员料号明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetAllImportedPartNumbers(long versionId, long? salesUserId)
        {
            var query = _db.Queryable<ForecastWeeklyDetailEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<CompanyNumberEntity>((detail, companyNumber) => detail.PartNumber == companyNumber.PartNumber)
                           .InnerJoin<UserInfoEntity>((detail, companyNumber, user) => detail.SalesUserId == user.UserId)
                           .Where((detail, companyNumber) => detail.VersionId == versionId && companyNumber.Status == 1);

            if (salesUserId.HasValue)
            {
                query = query.Where(detail => detail.SalesUserId == salesUserId.Value);
            }

            // SELECT DISTINCT 时 ORDER BY 表达式必须与 SELECT 列表中的列完全一致（同一张表的同一列），
            // 因此这里按 detail.PartNumber 排序，而不是（虽然值相等但列不同的）companyNumber.PartNumber
            return await query.OrderBy((detail, companyNumber, user) => new { detail.SalesUserId, detail.PartNumber })
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
    }
}
