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
        /// 查询指定业务负责人所负责的公司料号
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetSalesPartNumbers(long salesUserId)
        {
            return await _db.Queryable<NumberAssignEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CompanyNumberEntity>((salesNumber, companyNumber) => salesNumber.PartNumber == companyNumber.PartNumber)
                            .Where(salesNumber => salesNumber.SalesUserId == salesUserId)
                            .Where((salesNumber, companyNumber) => companyNumber.Status == 1)
                            .OrderBy(salesNumber => salesNumber.PartNumber)
                            .Select((salesNumber, companyNumber) => new FoWeeklyRowDto
                            {
                                PartNumber = salesNumber.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询指定版本下公司料号明细
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<List<FoWeeklyRowDto>> GetImportedPartNumbers(long versionId, long salesUserId)
        {
            return await _db.Queryable<ForecastWeeklyDetailEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CompanyNumberEntity>((detail, companyNumber) => detail.PartNumber == companyNumber.PartNumber)
                            .Where(detail => detail.VersionId == versionId && detail.SalesUserId == salesUserId)
                            .Where((detail, companyNumber) => companyNumber.Status == 1)
                            .OrderBy(detail => detail.PartNumber)
                            .Select((detail, companyNumber) => new FoWeeklyRowDto
                            {
                                PartNumber = detail.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                            })
                            .Distinct()
                            .ToListAsync();
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

        /// <summary>
        /// 查询指定版本下指定业务人员的预测周明细归档
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<ForecastWeeklyArchiveEntity?> GetForecastWeeklyArchive(long versionId, long salesUserId)
        {
            return await _db.Queryable<ForecastWeeklyArchiveEntity>()
                            .With(SqlWith.NoLock)
                            .Where(archive => archive.VersionId == versionId && archive.SalesUserId == salesUserId)
                            .FirstAsync();
        }

        /// <summary>
        /// 查询给定公司料号列表中，已存在的有效料号
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingCompanyNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<CompanyNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(companyNumber => partNumbers.Contains(companyNumber.PartNumber))
                            .Select(companyNumber => companyNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 查询给定公司料号列表中，已配置为指定业务负责人的料号
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetAssignedPartNumbers(List<string> partNumbers, long salesUserId)
        {
            return await _db.Queryable<NumberAssignEntity>()
                            .With(SqlWith.NoLock)
                            .Where(salesNumber => partNumbers.Contains(salesNumber.PartNumber) && salesNumber.SalesUserId == salesUserId)
                            .Select(salesNumber => salesNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 清空指定版本下、指定业务人员自己的预测周明细（仅清空导入人自己的数据，不影响同一版本下其他业务人员已导入的数据）
        /// </summary>
        /// <param name="versionId"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<int> DeleteForecastWeeklyDetails(long versionId, long salesUserId)
        {
            return await _db.Deleteable<ForecastWeeklyDetailEntity>()
                            .Where(detail => detail.VersionId == versionId && detail.SalesUserId == salesUserId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量新增预测周明细
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<int> InsertForecastWeeklyDetailList(List<ForecastWeeklyDetailEntity> list)
        {
            return await _db.Insertable(list).ExecuteCommandAsync();
        }
    }
}
