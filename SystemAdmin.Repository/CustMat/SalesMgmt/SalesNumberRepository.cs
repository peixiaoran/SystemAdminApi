using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemConfig.Entity;

namespace SystemAdmin.Repository.CustMat.SalesMgmt
{
    public class SalesNumberRepository
    {
        private readonly SqlSugarScope _db;
        private readonly Language _lang;

        public SalesNumberRepository(SqlSugarScope db, Language lang)
        {
            _db = db;
            _lang = lang;
        }

        /// <summary>
        /// 查询业务负责料号分页（仅查询属于自己负责的公司料号）
        /// </summary>
        /// <param name="getPage"></param>
        /// <param name="salesUserId">当前登录人Id</param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage, long salesUserId)
        {
            var query = _db.Queryable<NumberAssignEntity>()
                           .With(SqlWith.NoLock)
                           .Where(numberAssign => numberAssign.SalesUserId == salesUserId)
                           .InnerJoin<CompanyNumberEntity>((numberAssign, companyNumber) => numberAssign.PartNumber == companyNumber.PartNumber);

            // 料号
            if (!string.IsNullOrEmpty(getPage.PartNumber))
            {
                query = query.Where((numberAssign, companyNumber) => companyNumber.PartNumber.Contains(getPage.PartNumber));
            }

            RefAsync<int> totalCount = 0;
            var page = await query.OrderBy((numberAssign, companyNumber) => companyNumber.CreatedDate)
                                  .Select((numberAssign, companyNumber) => new SalesNumberDto
                                  {
                                      PartNumber = companyNumber.PartNumber,
                                      PartNameCn = companyNumber.PartNameCn,
                                      PartNameEn = companyNumber.PartNameEn,
                                      Specification = companyNumber.Specification,
                                      Status = companyNumber.Status,
                                  }).ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<SalesNumberDto>.Ok(page, totalCount, "");
        }

        /// <summary>
        /// 根据公司料号查询详情
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<CompanyNumberDetailDto> GetPartNumberDetail(string partNumber)
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
    }
}
