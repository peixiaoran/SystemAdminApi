using Mapster;
using SqlSugar;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Dto;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Entity;
using SystemAdmin.Model.CustMat.CustMatBasicInfo.Queries;

namespace SystemAdmin.Repository.CustMat.CustMatBasicInfo
{
    public class NumberMappingRepository
    {
        private readonly SqlSugarScope _db;

        public NumberMappingRepository(SqlSugarScope db)
        {
            _db = db;
        }

        /// <summary>
        /// 新增客户料号与公司料号对照
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> InsertNumberMapping(NumberMappingEntity entity)
        {
            return await _db.Insertable(entity).ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除客户料号与公司料号对照
        /// </summary>
        /// <param name="mappingId"></param>
        /// <returns></returns>
        public async Task<int> DeleteNumberMapping(long mappingId)
        {
            return await _db.Deleteable<NumberMappingEntity>()
                            .Where(mapping => mapping.MappingId == mappingId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 修改客户料号与公司料号对照
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<int> UpdateNumberMapping(NumberMappingEntity entity)
        {
            return await _db.Updateable(entity)
                            .IgnoreColumns(mapping => new
                            {
                                mapping.MappingId,
                                mapping.CreatedBy,
                                mapping.CreatedDate,
                            }).Where(mapping => mapping.MappingId == entity.MappingId)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 查询客户料号与公司料号对照实体
        /// </summary>
        /// <param name="mappingId"></param>
        /// <returns></returns>
        public async Task<NumberMappingDto> GetNumberMappingEntity(long mappingId)
        {
            var entity = await _db.Queryable<NumberMappingEntity>()
                                  .With(SqlWith.NoLock)
                                  .Where(mapping => mapping.MappingId == mappingId)
                                  .FirstAsync();
            return entity.Adapt<NumberMappingDto>();
        }

        /// <summary>
        /// 查询客户料号与公司料号对照分页
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<ResultPaged<NumberMappingDto>> GetNumberMappingPage(GetNumberMappingPage getPage)
        {
            RefAsync<int> totalCount = 0;
            var query = _db.Queryable<NumberMappingEntity>()
                           .With(SqlWith.NoLock);

            // 客户料号
            if (!string.IsNullOrEmpty(getPage.CustomerPartNumber))
            {
                query = query.Where(mapping => mapping.CustomerPartNumber.Contains(getPage.CustomerPartNumber));
            }

            // 公司料号
            if (!string.IsNullOrEmpty(getPage.CompanyPartNumber))
            {
                query = query.Where(mapping => mapping.CompanyPartNumber.Contains(getPage.CompanyPartNumber));
            }

            // 状态
            if (getPage.Status.HasValue)
            {
                query = query.Where(mapping => mapping.Status == getPage.Status.Value);
            }

            // 排序
            query = query.OrderBy(mapping => mapping.CreatedDate);

            var mappingPage = await query.ToPageListAsync(getPage.PageIndex, getPage.PageSize, totalCount);
            return ResultPaged<NumberMappingDto>.Ok(mappingPage.Adapt<List<NumberMappingDto>>(), totalCount, "");
        }

        /// <summary>
        /// 按查询条件查询客户料号与公司料号对照信息列表
        /// </summary>
        /// <param name="getPage"></param>
        /// <returns></returns>
        public async Task<List<NumberMappingDto>> GetNumberMappingList(GetNumberMappingPage getPage)
        {
            var query = _db.Queryable<NumberMappingEntity>()
                           .With(SqlWith.NoLock);

            // 客户料号
            if (!string.IsNullOrEmpty(getPage.CustomerPartNumber))
            {
                query = query.Where(mapping => mapping.CustomerPartNumber.Contains(getPage.CustomerPartNumber));
            }

            // 公司料号
            if (!string.IsNullOrEmpty(getPage.CompanyPartNumber))
            {
                query = query.Where(mapping => mapping.CompanyPartNumber.Contains(getPage.CompanyPartNumber));
            }

            // 状态
            if (getPage.Status.HasValue)
            {
                query = query.Where(mapping => mapping.Status == getPage.Status.Value);
            }

            var mappingList = await query.OrderBy(mapping => mapping.CreatedDate).ToListAsync();
            return mappingList.Adapt<List<NumberMappingDto>>();
        }

        /// <summary>
        /// 同一公司料号在指定生效时间范围内是否已存在重叠的对照关系（不论对照的客户料号是否相同）
        /// </summary>
        /// <param name="companyPartNumber"></param>
        /// <param name="effectiveFrom"></param>
        /// <param name="effectiveTo"></param>
        /// <param name="excludeMappingId">修改时排除自身</param>
        /// <returns></returns>
        public async Task<bool> HasOverlappingMapping(string companyPartNumber, DateTime effectiveFrom, DateTime? effectiveTo, long? excludeMappingId)
        {
            var effectiveToOrMax = effectiveTo ?? DateTime.MaxValue;

            return await _db.Queryable<NumberMappingEntity>()
                            .With(SqlWith.NoLock)
                            .Where(mapping => mapping.CompanyPartNumber == companyPartNumber)
                            .WhereIF(excludeMappingId.HasValue, mapping => mapping.MappingId != excludeMappingId!.Value)
                            // 区间重叠判断：现有记录的开始 <= 新记录的结束 且 现有记录的结束（为空则视为无限期）>= 新记录的开始
                            .Where(mapping => mapping.EffectiveFrom <= effectiveToOrMax && (mapping.EffectiveTo == null || mapping.EffectiveTo >= effectiveFrom))
                            .AnyAsync();
        }

        /// <summary>
        /// 客户料号下拉（模糊查询，配合 el-autocomplete 使用）
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<List<string>> GetCustomerPartNumberDrop(string keyword)
        {
            var query = _db.Queryable<CustomerNumberEntity>()
                           .With(SqlWith.NoLock);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(customerNumber => customerNumber.PartNumber.Contains(keyword));
            }

            return await query.OrderBy(customerNumber => customerNumber.PartNumber)
                              .Select(customerNumber => customerNumber.PartNumber)
                              .Take(20)
                              .ToListAsync();
        }

        /// <summary>
        /// 公司料号下拉（模糊查询，配合 el-autocomplete 使用）
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public async Task<List<string>> GetCompanyPartNumberDrop(string keyword)
        {
            var query = _db.Queryable<CompanyNumberEntity>()
                           .With(SqlWith.NoLock);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(companyNumber => companyNumber.PartNumber.Contains(keyword));
            }

            return await query.OrderBy(companyNumber => companyNumber.PartNumber)
                              .Select(companyNumber => companyNumber.PartNumber)
                              .Take(20)
                              .ToListAsync();
        }

        /// <summary>
        /// 批量新增客户料号与公司料号对照列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<int> InsertNumberMappingList(List<NumberMappingEntity> list)
        {
            return await _db.Insertable(list).ExecuteCommandAsync();
        }

        /// <summary>
        /// 一次性查询给定客户料号列表中，已启用且存在的客户料号
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetValidCustomerPartNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<CustomerNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(customerNumber => partNumbers.Contains(customerNumber.PartNumber) && customerNumber.Status == 1)
                            .Select(customerNumber => customerNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 一次性查询给定公司料号列表中，已启用且存在的公司料号
        /// </summary>
        /// <param name="partNumbers"></param>
        /// <returns></returns>
        public async Task<List<string>> GetValidCompanyPartNumbers(List<string> partNumbers)
        {
            return await _db.Queryable<CompanyNumberEntity>()
                            .With(SqlWith.NoLock)
                            .Where(companyNumber => partNumbers.Contains(companyNumber.PartNumber) && companyNumber.Status == 1)
                            .Select(companyNumber => companyNumber.PartNumber)
                            .ToListAsync();
        }

        /// <summary>
        /// 一次性查询给定公司料号列表中，数据库已存在的对照记录
        /// </summary>
        /// <param name="companyPartNumbers"></param>
        /// <returns></returns>
        public async Task<List<NumberMappingEntity>> GetMappingsByCompanyPartNumbers(List<string> companyPartNumbers)
        {
            return await _db.Queryable<NumberMappingEntity>()
                            .With(SqlWith.NoLock)
                            .Where(mapping => companyPartNumbers.Contains(mapping.CompanyPartNumber))
                            .ToListAsync();
        }

        /// <summary>
        /// 删除指定公司料号下的全部对照关系（公司料号被删除时联动清理）
        /// </summary>
        /// <param name="companyPartNumber"></param>
        /// <returns></returns>
        public async Task<int> DeleteMappingsByCompanyPartNumber(string companyPartNumber)
        {
            return await _db.Deleteable<NumberMappingEntity>()
                            .Where(mapping => mapping.CompanyPartNumber == companyPartNumber)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除指定客户料号下的全部对照关系（客户料号被删除时联动清理）
        /// </summary>
        /// <param name="customerPartNumber"></param>
        /// <returns></returns>
        public async Task<int> DeleteMappingsByCustomerPartNumber(string customerPartNumber)
        {
            return await _db.Deleteable<NumberMappingEntity>()
                            .Where(mapping => mapping.CustomerPartNumber == customerPartNumber)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 删除指定客户料号列表下的全部对照关系（客户被删除、其下客户料号被联动清理时使用）
        /// </summary>
        /// <param name="customerPartNumbers"></param>
        /// <returns></returns>
        public async Task<int> DeleteMappingsByCustomerPartNumbers(List<string> customerPartNumbers)
        {
            return await _db.Deleteable<NumberMappingEntity>()
                            .Where(mapping => customerPartNumbers.Contains(mapping.CustomerPartNumber))
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 将指定公司料号下、当前仍启用的对照关系全部置为失效（公司料号被修改为失效时联动更新）
        /// </summary>
        /// <param name="companyPartNumber"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public async Task<int> InvalidateMappingsByCompanyPartNumber(string companyPartNumber, long modifiedBy, string modifiedDate)
        {
            return await _db.Updateable<NumberMappingEntity>()
                            .SetColumns(mapping => new NumberMappingEntity { Status = 0, ModifiedBy = modifiedBy, ModifiedDate = modifiedDate })
                            .Where(mapping => mapping.CompanyPartNumber == companyPartNumber && mapping.Status != 0)
                            .ExecuteCommandAsync();
        }

        /// <summary>
        /// 将指定客户料号下、当前仍启用的对照关系全部置为失效（客户料号被修改为失效时联动更新）
        /// </summary>
        /// <param name="customerPartNumber"></param>
        /// <param name="modifiedBy"></param>
        /// <param name="modifiedDate"></param>
        /// <returns></returns>
        public async Task<int> InvalidateMappingsByCustomerPartNumber(string customerPartNumber, long modifiedBy, string modifiedDate)
        {
            return await _db.Updateable<NumberMappingEntity>()
                            .SetColumns(mapping => new NumberMappingEntity { Status = 0, ModifiedBy = modifiedBy, ModifiedDate = modifiedDate })
                            .Where(mapping => mapping.CustomerPartNumber == customerPartNumber && mapping.Status != 0)
                            .ExecuteCommandAsync();
        }
    }
}
