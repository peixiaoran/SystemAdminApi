using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Dto;
using SystemAdmin.Model.CustMat.RollingForecast.Entity;
using SystemAdmin.Model.CustMat.RollingForecast.Queries;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;
using CompanyNumberDetailDto = SystemAdmin.Model.CustMat.SalesMgmt.Dto.CompanyNumberDetailDto;
using SalesUserDropDto = SystemAdmin.Model.CustMat.SalesMgmt.Dto.SalesUserDropDto;

namespace SystemAdmin.Repository.CustMat.ForecastDetail
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
        /// 按业务人员Id查询料号分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage, long salesUserId)
        {
            var query = _db.Queryable<NumberAssignEntity>()
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
        /// 查询公司料号基础信息
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<CompanyNumberDetailDto> GetCompanyPartNumberInfo(string partNumber)
        {
            return await _db.Queryable<CompanyNumberEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic) => typeDic.DicType == "PartType" && companyNumber.PartType == typeDic.DicCode)
                            .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic) => categoryDic.DicType == "Category" && companyNumber.Category == categoryDic.DicCode)
                            .InnerJoin<DictionaryInfoEntity>((companyNumber, typeDic, categoryDic, sourceDic) => sourceDic.DicType == "SourceType" && companyNumber.SourceType == sourceDic.DicCode)
                            .Where(companyNumber => companyNumber.PartNumber == partNumber)
                            .Select((companyNumber, typeDic, categoryDic, sourceDic) => new CompanyNumberDetailDto
                            {
                                PartNumberId = companyNumber.PartNumberId,
                                PartNumber = companyNumber.PartNumber,
                                PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                Specification = companyNumber.Specification,
                                PartType = companyNumber.PartType,
                                PartTypeName = _lang.Locale == "zh-CN" ? typeDic.DicNameCn : typeDic.DicNameEn,
                                Category = companyNumber.Category,
                                CategoryName = _lang.Locale == "zh-CN" ? categoryDic.DicNameCn : categoryDic.DicNameEn,
                                Model = companyNumber.Model,
                                DrawingNumber = companyNumber.DrawingNumber,
                                Version = companyNumber.Version,
                                Unit = companyNumber.Unit,
                                SourceType = companyNumber.SourceType,
                                SourceTypeName = _lang.Locale == "zh-CN" ? sourceDic.DicNameCn : sourceDic.DicNameEn,
                                Manufacturer = companyNumber.Manufacturer,
                                ManufacturerPartNumber = companyNumber.ManufacturerPartNumber,
                                LotControl = companyNumber.LotControl,
                                Status = companyNumber.Status,
                                Remark = companyNumber.Remark,
                            }).FirstAsync();
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
