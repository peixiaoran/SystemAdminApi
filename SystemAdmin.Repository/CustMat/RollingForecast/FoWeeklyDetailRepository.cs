using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.RollingForecast
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
        /// 查询指定业务负责人所负责的公司料号
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetSalesPartNumbers(long salesUserId)
        {
            return await _db.Queryable<SalesNumberEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CompanyNumberEntity>((salesNumber, companyNumber) => salesNumber.PartNumber == companyNumber.PartNumber)
                            .Where(salesNumber => salesNumber.SalesUserId == salesUserId)
                            .OrderBy(salesNumber => salesNumber.PartNumber)
                            .Select((salesNumber, companyNumber) => new FoWeeklyRowDto
                            {
                                PartNumber = salesNumber.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询指定版本下业务负责人的预测周明细
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
