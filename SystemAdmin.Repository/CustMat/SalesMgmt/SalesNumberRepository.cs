using SqlSugar;
using SystemAdmin.CommonSetup.Security;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Dto;
using SystemAdmin.Model.CustMat.SalesMgmt.Entity;
using SystemAdmin.Model.CustMat.SalesMgmt.Queries;
using SystemAdmin.Model.SystemBasicMgmt.SystemBasicData.Entity;
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
        /// 新增人员料号信息
        /// </summary>
        /// <param name="salesNumberEntity"></param>
        /// <returns></returns>
        public async Task<int> InsertSalesNumber(SalesNumberEntity salesNumberEntity)
        {
            return await _db.Insertable(salesNumberEntity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除人员料号信息
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<int> DeleteSalesNumber(string partNumber)
        {
            return await _db.Deleteable<SalesNumberEntity>()
                            .Where(salesNumber => salesNumber.PartNumber == partNumber)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改人员料号信息
        /// </summary>
        /// <param name="salesNumberEntity"></param>
        /// <param name="originalPartNumber">原公司料号，用于定位原记录（料号可重新选择，主键随之变更）</param>
        /// <returns></returns>
        public async Task<int> UpdateSalesNumber(SalesNumberEntity salesNumberEntity, string originalPartNumber)
        {
            return await _db.Updateable(salesNumberEntity)
                            .IgnoreColumns(salesNumber => new
                            {
                                salesNumber.CreatedBy,
                                salesNumber.CreatedDate,
                            }).Where(salesNumber => salesNumber.PartNumber == originalPartNumber)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 公司料号是否已配置业务负责人
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<bool> ExistsSalesNumber(string partNumber)
        {
            return await _db.Queryable<SalesNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(salesNumber => salesNumber.PartNumber == partNumber)
                            .AnyAsync();
        }

        /// <summary>
        /// 指定人员是否已配置为业务人员
        /// </summary>
        /// <param name="salesUserId"></param>
        /// <returns></returns>
        public async Task<bool> ExistsSalesUser(long salesUserId)
        {
            return await _db.Queryable<SalesUserEntity>()
                            .With(SqlWith.NoLock)
                            .Where(salesUser => salesUser.SalesUserId == salesUserId)
                            .AnyAsync();
        }

        /// <summary>
        /// 查询人员料号信息实体
        /// </summary>
        /// <param name="partNumber"></param>
        /// <returns></returns>
        public async Task<SalesNumberDto> GetSalesNumberEntity(string partNumber)
        {
            var entity = await _db.Queryable<SalesNumberEntity>()
                                  .With(SqlWith.NoLock)
                                  .InnerJoin<CompanyNumberEntity>((salesNumber, companyNumber) => salesNumber.PartNumber == companyNumber.PartNumber)
                                  .InnerJoin<UserInfoEntity>((salesNumber, companyNumber, user) => salesNumber.SalesUserId == user.UserId)
                                  .Where(salesNumber => salesNumber.PartNumber == partNumber)
                                  .Select((salesNumber, companyNumber, user) => new SalesNumberDto
                                  {
                                      PartNumber = salesNumber.PartNumber,
                                      PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                      SalesUserId = salesNumber.SalesUserId,
                                      UserNo = user.UserNo,
                                      UserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
                                  })
                                  .FirstAsync();
            return entity;
        }

        /// <summary>
        /// 查询人员料号信息分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<SalesNumberDto>> GetSalesNumberPage(GetSalesNumberPage getPage)
        {
            var query = _db.Queryable<SalesNumberEntity>()
                           .With(SqlWith.NoLock)
                           .InnerJoin<CompanyNumberEntity>((salesNumber, companyNumber) => salesNumber.PartNumber == companyNumber.PartNumber)
                           .InnerJoin<UserInfoEntity>((salesNumber, companyNumber, user) => salesNumber.SalesUserId == user.UserId);

            // 公司料号
            if (!string.IsNullOrEmpty(getPage.PartNumber))
            {
                query = query.Where((salesNumber, companyNumber) => companyNumber.PartNumber.Contains(getPage.PartNumber));
            }

            // 业务负责人Id
            if (!string.IsNullOrEmpty(getPage.SalesUserId))
            {
                query = query.Where(salesNumber => salesNumber.SalesUserId == long.Parse(getPage.SalesUserId));
            }

            // 业务负责人姓名
            if (!string.IsNullOrEmpty(getPage.UserName))
            {
                query = query.Where((salesNumber, companyNumber, user) => user.UserNameCn.Contains(getPage.UserName) || user.UserNameEn.Contains(getPage.UserName));
            }

            RefAsync<int> totalCount = 0;
            var page = await query.OrderBy(salesNumber => salesNumber.CreatedDate)
                                  .Select((salesNumber, companyNumber, user) => new SalesNumberDto
                                  {
                                      PartNumber = salesNumber.PartNumber,
                                      PartName = _lang.Locale == "zh-CN" ? companyNumber.PartNameCn : companyNumber.PartNameEn,
                                      SalesUserId = salesNumber.SalesUserId,
                                      UserNo = user.UserNo,
                                      UserName = _lang.Locale == "zh-CN" ? user.UserNameCn : user.UserNameEn,
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
        /// 公司料号下拉
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<List<CompanyNumberDropDto>> GetCompanyPartNumberDrop(string keyword)
        {
            var query = _db.Queryable<CompanyNumberEntity>()
                           .With(SqlWith.NoLock);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(companyNumber => companyNumber.PartNumber.Contains(keyword));
            }

            return await query.OrderBy(companyNumber => companyNumber.PartNumber)
                              .Select(companyNumber => new CompanyNumberDropDto
                              {
                                  PartNumber = companyNumber.PartNumber,
                              })
                              .Take(20)
                              .ToListAsync();
        }

        /// <summary>
        /// 客户信息下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<CustomerDropDto>> GetCustomerDrop()
        {
            return await _db.Queryable<CustomerInfoEntity>()
                            .With(SqlWith.NoLock)
                            .OrderBy(customer => customer.CustomerCode)
                            .Select(customer => new CustomerDropDto
                            {
                                CustomerId = customer.CustomerId,
                                CustomerName = _lang.Locale == "zh-CN" ? customer.CustomerNameCn : customer.CustomerNameEn,
                            }).ToListAsync();
        }

        /// <summary>
        /// 查询指定客户下、经料号对照关联到的全部公司料号
        /// 关联链路：客户信息 → 客户料号 → 料号对照 → 公司料号（不判断生效失效时间）
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetCompanyPartNumbersByCustomer(long customerId)
        {
            return await _db.Queryable<CustomerInfoEntity>()
                            .With(SqlWith.NoLock)
                            .InnerJoin<CustomerNumberEntity>((customer, customerNumber) => customer.CustomerCode == customerNumber.CustomerCode)
                            .InnerJoin<NumberMappingEntity>((customer, customerNumber, mapping) => customerNumber.PartNumber == mapping.CustomerPartNumber)
                            .InnerJoin<CompanyNumberEntity>((customer, customerNumber, mapping, companyNumber) => mapping.CompanyPartNumber == companyNumber.PartNumber)
                            .Where(customer => customer.CustomerId == customerId)
                            .Select((customer, customerNumber, mapping, companyNumber) => companyNumber.PartNumber)
                            .Distinct()
                            .ToListAsync();
        }

        /// <summary>
        /// 查询给定公司料号列表中，已配置过业务负责人的料号
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetExistingPartNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<SalesNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(salesNumber => partNumbers.Contains(salesNumber.PartNumber))
                            .Select(salesNumber => salesNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 批量新增人员料号信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<int> InsertSalesNumberList(List<SalesNumberEntity> list)
        {
            return await _db.Insertable(list).ExecuteCommandAsync();
        }

        /// <summary>
        /// 批量将指定公司料号绑定到同一业务负责人
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <param name="salesUserId"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public async Task<int> UpdateSalesUserByPartNumbers(List<string> partNumbers, long salesUserId, long modifiedBy, string modifiedDate)
        {
            return await _db.Updateable<SalesNumberEntity>()
                            .SetColumns(salesNumber => new SalesNumberEntity { SalesUserId = salesUserId, ModifiedBy = modifiedBy, ModifiedDate = modifiedDate })
                            .Where(salesNumber => partNumbers.Contains(salesNumber.PartNumber))
                            .ExecuteCommandAsync();
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
