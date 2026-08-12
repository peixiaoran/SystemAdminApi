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
    }
}
