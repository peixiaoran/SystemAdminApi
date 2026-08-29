using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;

namespace SystemAdmin.Repository.CustMat.RollingForecast
{
    public class NumberTrendRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public NumberTrendRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 查询料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage, long salesUserId)
        {
            var query = _db.Queryable<SalesNumberEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<CompanyNumberEntity>((salesNumber, companyNumber) => salesNumber.PartNumber == companyNumber.PartNumber)
                           .Where(salesNumber => salesNumber.SalesUserId == salesUserId)
                           .Where((salesNumber, companyNumber) => companyNumber.Status == 1);

            // 公司料号
            if (!string.IsNullOrEmpty(getPage.PartNumber))
            {
                query = query.Where((salesNumber, companyNumber) => companyNumber.PartNumber.Contains(getPage.PartNumber));
            }

            RefAsync<int> totalCount = 0;
            var page = await query.OrderBy((salesNumber, companyNumber) => companyNumber.PartNumber)
                                  .Select((salesNumber, companyNumber) => new SalesNumberDto
                                  {
                                      PartNumber = salesNumber.PartNumber,
                                      PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                  }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<SalesNumberDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 版本下拉框
        /// </summary>
        /// <returns></returns>
        public async Task<List<ForecastVersionDropDto>> GetForecastVersionDrop()
        {
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .OrderByDescending(version => version.StartDate)
                            .Select(version => new ForecastVersionDropDto
                            {
                                VersionId = version.VersionId,
                                VersionCode = version.VersionCode,
                            }).ToListAsync();
        }

        /// <summary>
        /// 按Id集合查询预测版本
        /// </summary>
        /// <param name="versionIds"></param>
        /// <returns></returns>
        public async Task<List<ForecastVersionEntity>> GetForecastVersionsByIds(List<long> versionIds)
        {
            return await _db.Queryable<ForecastVersionEntity>()
                            .With(SqlWith.NoLock)
                            .Where(version => versionIds.Contains(version.VersionId))
                            .ToListAsync();
        }

        /// <summary>
        /// 按料号统计版本用量，用于按版本统计天/周用量
        /// </summary>
        /// <param name="partNumber"></param>
        /// <param name="versionIds"></param>
        /// <returns></returns>
        public async Task<List<ForecastWeeklyDetailEntity>> GetForecastWeeklyDetailsByVersions(string partNumber, List<long> versionIds)
        {
            return await _db.Queryable<ForecastWeeklyDetailEntity>()
                            .With(SqlWith.NoLock)
                            .Where(detail => detail.PartNumber == partNumber && versionIds.Contains(detail.VersionId))
                            .ToListAsync();
        }
    }
}
